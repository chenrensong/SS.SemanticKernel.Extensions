using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class MarkdownDataSource : IDataSource
{
    public string Directory { get; private set; }

    public bool FileContainersHeaderRow { get; private set; }

    public MarkdownDataSource(string directory, bool fileContainersHeaderRow = true)
    {
        this.Directory = directory;
        this.FileContainersHeaderRow = fileContainersHeaderRow;
    }

    public async Task<IEnumerable<ContentResource>> Load()
    {
        var toReturn = new List<TextResource>();

        var allMdFiles = System.IO.Directory.GetFiles(this.Directory, "*.md", SearchOption.AllDirectories);

        foreach (var file in allMdFiles)
        {
            using (var sr = new StreamReader(new FileStream(file, FileMode.Open)))
            {
                var headerRow = string.Empty;

                if (this.FileContainersHeaderRow)
                {
                    headerRow = await sr.ReadLineAsync();
                }

                var row = await sr.ReadLineAsync();

                while (row != null)
                {
                    var rowSplit = row.Split(',');
                    var headerSplit = this.FileContainersHeaderRow && headerRow != null ? headerRow.Split(',') : null;

                    //join the header and the row values in a key:value pair
                    var joined = headerSplit != null ? headerSplit.Zip(rowSplit, (h, r) => $"{h}:{r}") : rowSplit;

                    toReturn.Add(new TextResource
                    {
                        Id = file,
                        Value = string.Join(",", joined),
                        ContentType = "text/markdown"
                    });

                    row = await sr.ReadLineAsync();
                }
            }
        }

        return toReturn;
    }
}