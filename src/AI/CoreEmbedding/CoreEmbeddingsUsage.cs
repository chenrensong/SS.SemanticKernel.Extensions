namespace Microsoft.SemanticKernel.AI.Embeddings {
    public partial class CoreEmbeddingsUsage {
        /// <summary> Initializes a new instance of EmbeddingsUsage. </summary>
        /// <param name="promptTokens"> Number of tokens sent in the original request. </param>
        /// <param name="totalTokens"> Total number of tokens transacted in this request/response. </param>
        internal CoreEmbeddingsUsage(int promptTokens, int totalTokens) {
            PromptTokens = promptTokens;
            TotalTokens = totalTokens;
        }

        /// <summary> Number of tokens sent in the original request. </summary>
        public int PromptTokens { get; }
        /// <summary> Total number of tokens transacted in this request/response. </summary>
        public int TotalTokens { get; }
    }
}
