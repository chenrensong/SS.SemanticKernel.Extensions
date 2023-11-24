using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class CsvDataSource : IDataSource
{
    public string Directory { get; private set; }

    public bool FileContainsHeaderRow { get; private set; }

    public CsvDataSource(string directory, bool fileContainersHeaderRow = true)
    {
        this.Directory = directory;
        this.FileContainsHeaderRow = fileContainersHeaderRow;
    }

    public async Task<IEnumerable<ContentResource>> Load()
    {
        var toReturn = new List<TextResource>();

        var allCsvFiles = System.IO.Directory.GetFiles(this.Directory, "*.csv",SearchOption.AllDirectories);

        foreach (var file in allCsvFiles)
        {
            using (var sr = new StreamReader(new FileStream(file, FileMode.Open)))
            {
                var headerRow = string.Empty;

                if (this.FileContainsHeaderRow)
                {
                    headerRow = await sr.ReadLineAsync();
                }

                var row = await sr.ReadLineAsync();

                while (row != null)
                {
                    var rowSplit = row.Split(',');
                    var headerSplit = this.FileContainsHeaderRow && headerRow != null ? headerRow.Split(',') : null;

                    //join the header and the row values in a key:value pair
                    var joined = headerSplit != null ? headerSplit.Zip(rowSplit, (h, r) => $"{h}:{r}") : rowSplit;

                    toReturn.Add(new TextResource
                    {
                        Id = file,
                        Value = string.Join(",", joined),
                        ContentType = "text/csv"
                    });
                    
                    row = await sr.ReadLineAsync();
                }
            }            
        }
        return toReturn;
    }
}