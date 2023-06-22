using Microsoft.SemanticKernel.Text;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel
{
    /// <summary>
    /// Takes an input resource and chunks it down to fit the given max length
    /// </summary>
    public class ChunkingTransform : ITransform
    {
        public int MaxSize { get; private set; }

        public ChunkingTransform(int maxSize = 1024)
        {
            this.MaxSize = maxSize;
        }

        public Task<IEnumerable<ContentResource>> Run(ContentResource input)
        {
            var textResource = input as TextResource;

            if (textResource == null)
            {
                throw new Exception("Only TextResources are currently supported.");
            }

            var toReturn = new List<ContentResource>();

            if (textResource.Value.Length > this.MaxSize)
            {
                List<string> lines;
                List<string> paragraphs;

                switch (textResource.ContentType)
                {
                    case "text/markdown":
                        {
                            lines = TextChunker.SplitMarkDownLines(textResource.Value, this.MaxSize);
                            paragraphs = TextChunker.SplitMarkdownParagraphs(lines, this.MaxSize);

                            break;
                        }
                    default:
                        {
                            lines = TextChunker.SplitPlainTextLines(textResource.Value, this.MaxSize);
                            paragraphs = TextChunker.SplitPlainTextParagraphs(lines, this.MaxSize);

                            break;
                        }
                }

                for (int i = 0; i < paragraphs.Count; i++)
                {
                    toReturn.Add(new TextResource
                    {
                        ContentType = textResource.ContentType,
                        Id = $"{textResource.Id}_{i}",
                        Value = paragraphs[i]
                    });
                }
            }
            else
            {
                toReturn.Add(input);
            }

            return Task.FromResult(toReturn.AsEnumerable());
        }
    }
}

