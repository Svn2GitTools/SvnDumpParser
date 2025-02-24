namespace SvnDumpParser.Models;

/// <summary>
/// Represents information about the content of a node.
/// </summary>
public class ContentInfo
{
    public int ContentLength { get; set; } // Total content length.
    public int? FileContentLength { get; set; } // Length of text content (if applicable).

    // Hashes for text content
    public string? TextContentMD5 { get; set; }
    public string? TextContentSHA1 { get; set; }

    public string? TextContent { get; set; } // For text content

    public byte[]? BinaryContent { get; set; } // For binary content
    public bool IsBinary => BinaryContent != null;
}
