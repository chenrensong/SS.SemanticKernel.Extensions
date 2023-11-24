using BlingFire;
using SS.SemanticKernel.Extensions.KnowledgeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel
{
    /// <summary>
    /// Takes an input resource and chunks it down to fit the given max length
    /// </summary>
    public class SummaryTransform : ITransform
    {
        private readonly IKernel _Kernel;

        public int MaxSize { get; private set; }

        public SummaryTransform(IKernel sk, int maxSize = 1024)
        {
            _Kernel = sk;

            this.MaxSize = maxSize;

            sk.CreateSemanticFunction(promptTemplate: SkillConst.Load("Summary.skprompt.txt"),
                                      functionName: "Summarise",
                                      pluginName: "Summary", description: null, requestSettings: null);
        }

        public async Task<IEnumerable<ContentResource>> Run(ContentResource input)
        {
            var textResource = input as TextResource;

            if (textResource == null)
            {
                throw new Exception("Only TextResources are currently supported.");
            }

            var toSummarise = new List<string>();
            var allsentences = BlingFireUtils.GetSentences(textResource.Value);

            var sb = new StringBuilder();

            foreach (var sentence in allsentences)
            {
                if (sb.Length + sentence.Length < this.MaxSize)
                {
                    sb.Append(sentence);
                }
                else
                {
                    toSummarise.Add(sb.ToString());

                    sb = new StringBuilder();
                }
            }

            var toReturn = new List<ContentResource>();

            foreach (var item in toSummarise)
            {
                var result = await _Kernel.RunAsync(item, _Kernel.Skills.GetFunction("Summary", "Summarise"));
                var resultString = result.GetValue<string>();

                toReturn.Add(new TextResource
                {
                    ContentType = textResource.ContentType,
                    Id = $"{textResource.Id}_{Guid.NewGuid()}",
                    Value = resultString
                });
            }

            return toReturn.AsEnumerable();
        }
    }
}

