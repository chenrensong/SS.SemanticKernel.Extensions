

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

internal class PdfDataSource : IDataSource {
    public string Directory { get; private set; }

    public PdfDataSource(string directory) {
        this.Directory = directory;
    }

    public Task<IEnumerable<ContentResource>> Load() {
        var toReturn = new List<TextResource>();

        var allPdfFiles = System.IO.Directory.GetFiles(this.Directory, "*.pdf", SearchOption.AllDirectories);

        foreach (var file in allPdfFiles) {
            using (var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(file)) {
                foreach (var pdfPage in pdfDocument.GetPages()) {
                    var pageText = ContentOrderTextExtractor.GetText(pdfPage);

                    //map each page to a single resource
                    toReturn.Add(new TextResource {
                        ContentType = "application/pdf",
                        Id = Guid.NewGuid().ToString(),
                        Value = pageText
                    });
                }
            }
        }

        return Task.FromResult<IEnumerable<ContentResource>>(toReturn);
    }
}
