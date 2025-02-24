namespace SvnDumpParser.Models;

/// <summary>
/// Represents information about a copied node.
/// </summary>
public class CopyInfo
{
    public string? CopyFromPath { get; set; } // Source path of the copy operation.
    public int? CopyFromRevision { get; set; } // Source revision of the copy operation.

    // Hashes for text content from the copy source
    public string? TextCopySourceMD5 { get; set; }
    public string? TextCopySourceSHA1 { get; set; }
}
