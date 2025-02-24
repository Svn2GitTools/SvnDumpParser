using System.Text;

namespace SvnDumpParser.Models;

//public class DumpNodeChange
//{
//    public EDumpChangeAction Action { get; set; } // "add", "modify", "delete", etc.
//    public EDumpNodeKind Kind { get; set; } // Type of the node (e.g., file, directory).

//    public byte[]? BinaryContent { get; set; } // For binary content

//    public string? CopyFromPath { get; set; } // Source path if this file is copied

//    public int? CopyFromRevision { get; set; } // Source revision if this file is copied

//    public bool IsBinary => BinaryContent != null;

//    public string Path { get; set; }

//    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

//    public string? TextContent { get; set; } // For text content

//    /// <summary>Returns a string that represents the current object.</summary>
//    /// <returns>A string that represents the current object.</returns>
//    public override string ToString()
//    {
//        StringBuilder sb = new StringBuilder();
//        sb.AppendLine($"  {Kind} {Action} {Path}");
//        if (Properties.Count > 0)
//        {
//            sb.AppendLine("  Properties:");
//            foreach (var prop in Properties)
//            {
//                sb.AppendLine($"    {prop.Key}: {prop.Value}");
//            }
//        }

//        if (IsBinary)
//        {
//            sb.AppendLine(
//                $"  Binary Content Length: {BinaryContent.Length} bytes");
//        }
//        else if (!string.IsNullOrEmpty(TextContent))
//        {
//            sb.AppendLine($"  Text Content: {TextContent.Length} bytes");
//        }

//        return sb.ToString();
//    }
//}
