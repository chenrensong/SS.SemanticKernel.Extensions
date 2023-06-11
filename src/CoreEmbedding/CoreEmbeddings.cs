using System.Collections.Generic;
using System.Linq;

namespace Microsoft.SemanticKernel.AI.Embeddings {
    /// <summary>
    /// Representation of the response data from an embeddings request.
    /// Embeddings measure the relatedness of text strings and are commonly used for search, clustering,
    /// recommendations, and other similar scenarios.
    /// </summary>
    public partial class CoreEmbeddings {
        /// <summary> Initializes a new instance of Embeddings. </summary>
        /// <param name="data"> Embedding values for the prompts submitted in the request. </param>
        /// <param name="usage"> Usage counts for tokens input using the embeddings API. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="data"/> or <paramref name="usage"/> is null. </exception>
        internal CoreEmbeddings(IEnumerable<CoreEmbeddingItem> data, CoreEmbeddingsUsage usage) {
            Data = data.ToList();
            Usage = usage;
        }

        /// <summary> Initializes a new instance of Embeddings. </summary>
        /// <param name="data"> Embedding values for the prompts submitted in the request. </param>
        /// <param name="usage"> Usage counts for tokens input using the embeddings API. </param>
        internal CoreEmbeddings(IReadOnlyList<CoreEmbeddingItem> data, CoreEmbeddingsUsage usage) {
            Data = data;
            Usage = usage;
        }

        /// <summary> Embedding values for the prompts submitted in the request. </summary>
        public IReadOnlyList<CoreEmbeddingItem> Data { get; }
        /// <summary> Usage counts for tokens input using the embeddings API. </summary>
        public CoreEmbeddingsUsage Usage { get; }
    }
}
