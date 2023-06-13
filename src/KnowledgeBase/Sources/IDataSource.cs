using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDataSource
{
    Task<IEnumerable<ContentResource>> Load();
}

public abstract class ContentResource
{
    public string Id { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public virtual object Value { get; set; } = new();
}

public class TextResource : ContentResource
{
    public new string Value { get; set; } = string.Empty;
}