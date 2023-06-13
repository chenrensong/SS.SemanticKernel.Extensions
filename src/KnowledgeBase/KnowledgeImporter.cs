using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel
{
    public class KnowledgeImporter
    {
        private List<ITransform> _Transforms = new();

        private List<IDataSource> _Datasources = new();

        private readonly IKernel _SemanticKernel;

        public KnowledgeImporter(IKernel sk)
        {
            _SemanticKernel = sk;
        }

        public void AddDataSource(IDataSource dataSource)
        {
            _Datasources.Add(dataSource);
        }

        public void AddTransform(ITransform transform)
        {
            _Transforms.Add(transform);
        }

        public async Task ProcessAsync(string destinationCollection)
        {
            if (!_Datasources.Any())
            {
                throw new Exception("Must have at least one data source defined before invoking run");
            }

            foreach (var ds in _Datasources)
            {
                //data sources may load into one or more resource instances
                var resources = await ds.Load();

                //only supporting text resources for now
                var inputResources = resources.OfType<TextResource>();
                var processedResources = new List<TextResource>();

                if (!_Transforms.Any())
                {
                    processedResources = inputResources as List<TextResource>;
                }
                else
                {
                    var resourceQueue = new Queue<ContentResource>(inputResources);

                    while (resourceQueue.Count > 0)
                    {
                        var resource = resourceQueue.Dequeue();
                        var state = await _RunTransforms(resource);

                        if (state.Completed.Any())
                        {
                            processedResources.AddRange(state.Completed.Cast<TextResource>());
                        }

                        //anything pending was newly created, needs to go into the queue
                        foreach (var item in state.Pending)
                        {
                            resourceQueue.Enqueue(item);
                        }
                    }
                }

                foreach (var resource in processedResources)
                {
                    //once all transforms are complete, generate embeddings and store
                    await _SemanticKernel.Memory.SaveInformationAsync(destinationCollection, resource.Value, resource.Id);
                }
            }
        }

        private async Task<TransformState> _RunTransforms(ContentResource resource)
        {
            var state = new TransformState();

            foreach (var tf in _Transforms)
            {
                state.Completed.AddRange(await tf.Run(resource));
            }

            return state;
        }

        private class TransformState
        {
            public List<ContentResource> Completed { get; set; } = new();

            public Queue<ContentResource> Pending { get; set; } = new();
        }
    }
}
