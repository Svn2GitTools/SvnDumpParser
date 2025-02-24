using System.Text;

namespace SvnDumpParser.Models
{
    /// <summary>
    /// Represents a node in an SVN dump file.
    /// </summary>
    public class DumpNode
    {
        // Content Information
        public ContentInfo ContentInfo { get; set; } = new ContentInfo();

        // Copy Information (if this node is copied from another location)
        public CopyInfo? CopyInfo { get; set; } = null;

        public EDumpChangeAction
            NodeAction { get; set; } // Action performed on the node (e.g., add, delete, modify).

        public EDumpNodeKind NodeKind { get; set; } // Type of the node (e.g., file, directory).

        // General Node Information
        public string NodePath { get; set; } // Path of the node in the repository.

        // Property Information
        public int PropContentLength { get; set; } // Length of property content.

        public Dictionary<string, string> Properties { get; set; } =
            new Dictionary<string, string>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  {NodeKind} {NodeAction} {NodePath}");
            if (Properties.Count > 0)
            {
                sb.AppendLine("  Properties:");
                foreach (var prop in Properties)
                {
                    sb.AppendLine($"    {prop.Key}: {prop.Value}");
                }
            }

            if (ContentInfo.IsBinary)
            {
                sb.AppendLine(
                    $"  Binary Content Length: {ContentInfo.BinaryContent.Length} bytes");
            }
            else if (!string.IsNullOrEmpty(ContentInfo.TextContent))
            {
                sb.AppendLine($"  Text Content: {ContentInfo.TextContent.Length} bytes");
            }

            return sb.ToString();
        }
    }
}
