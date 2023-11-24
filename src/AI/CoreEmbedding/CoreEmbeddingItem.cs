using System.Collections.Generic;
using System.Linq;

namespace Microsoft.SemanticKernel.AI.Embeddings {
    /// <summary> Representation of a single embeddings relatedness comparison. </summary>
    public partial class CoreEmbeddingItem {
        /// <summary> Initializes a new instance of EmbeddingItem. </summary>
        /// <param name="embedding">
        /// List of embeddings value for the input prompt. These represent a measurement of the
        /// vector-based relatedness of the provided input.
        /// </param>
        /// <param name="index"> Index of the prompt to which the EmbeddingItem corresponds. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="embedding"/> is null. </exception>
        internal CoreEmbeddingItem(IEnumerable<float> embedding, int index) {
            Embedding = embedding.ToList();
            Index = index;
        }

        /// <summary> Initializes a new instance of EmbeddingItem. </summary>
        /// <param name="embedding">
        /// List of embeddings value for the input prompt. These represent a measurement of the
        /// vector-based relatedness of the provided input.
        /// </param>
        /// <param name="index"> Index of the prompt to which the EmbeddingItem corresponds. </param>
        internal CoreEmbeddingItem(IReadOnlyList<float> embedding, int index) {
            Embedding = embedding;
            Index = index;
        }

        /// <summary>
        /// List of embeddings value for the input prompt. These represent a measurement of the
        /// vector-based relatedness of the provided input.
        /// </summary>
        public IReadOnlyList<float> Embedding { get; }
        /// <summary> Index of the prompt to which the EmbeddingItem corresponds. </summary>
        public int Index { get; }
    }
}
