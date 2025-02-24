namespace SvnDumpParser.Models;

public class DumpFileHeader
{
    public string UUID { get; set; }

    public int Version { get; set; }
    public override string ToString()
    {
        return $"Version: {Version}, UUID: {UUID}";
    }
}
