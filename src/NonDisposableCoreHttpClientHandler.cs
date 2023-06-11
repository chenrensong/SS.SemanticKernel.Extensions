using System.Net.Http;

namespace Microsoft.SemanticKernel {
    internal sealed class NonDisposableCoreHttpClientHandler : HttpClientHandler {
        public static NonDisposableCoreHttpClientHandler Instance { get; } = new NonDisposableCoreHttpClientHandler();

        private NonDisposableCoreHttpClientHandler() {
            base.CheckCertificateRevocationList = true;
        }

        protected override void Dispose(bool disposing) {
        }
    }
}
