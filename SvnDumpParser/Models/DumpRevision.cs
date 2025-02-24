namespace SvnDumpParser.Models;

public class DumpRevision
{
    public string Author { get; set; }

    public List<DumpNode> Changes { get; } = new();

    public DateTime Date { get; set; }

    public string Comment { get; set; }

    public long Number { get; set; }

    public Dictionary<string, string> Properties { get; } = new();
 

    public void AddNode(DumpNode node)
    {
        Changes.Add(node);
    }
}
