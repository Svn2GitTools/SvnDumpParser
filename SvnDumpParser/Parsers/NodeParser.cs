using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;

namespace SvnDumpParser.Parsers;

public sealed class NodeParser : ParserBase
{
    //private readonly List<NodeChange> _nodeChanges;

    public NodeParser(ParserOptions options)
        : base(options)
    {
        //_nodeChanges = fileChanges ?? throw new ArgumentNullException(nameof(fileChanges));
    }

    private static class NodeHeaders
    {
        public const string ContentLength = "Content-length:";

        public const string NodeAction = "Node-action:";

        public const string NodeCopyFromPath = "Node-copyfrom-path:";

        public const string NodeCopyFromRev = "Node-copyfrom-rev:";

        public const string NodeKind = "Node-kind:";

        public const string NodePath = "Node-path:";

        public const string PropContentLength = "Prop-content-length:";

        public const string TextContentLength = "Text-content-length:";

        public const string TextContentMD5 = "Text-content-md5:";

        public const string TextContentSHA1 = "Text-content-sha1:";

        public const string TextCopySourceMD5 = "Text-copy-source-md5:";

        public const string TextCopySourceSHA1 = "Text-copy-source-sha1:";
    }

    public override string? Parse(
        IMixedDataReader reader,
        IParserActions parserActions,
        string? currentLine)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        Log("Parsing Node...");

        //var node = new DumpFileNode();
        DumpNode node = parserActions.CreateNode();
        string? line = ReadNodeHeader(reader, currentLine, node);

        //var nodeChange = CreateNodeChange(node);
        ParseProperties(reader, node.PropContentLength, node.Properties);

        line = ProcessNodeContent(reader, node, parserActions);

        //_nodeChanges.Add(nodeChange);
        parserActions.AddNode(node);
        Log(node.ToString());

        return string.IsNullOrEmpty(line) ? reader.ReadNextNonEmptyLine() : line;
    }

    //private DumpNodeChange CreateNodeChange(DumpNode node) =>
    //    new()
    //        {
    //            Action = node.NodeAction,
    //            Path = node.NodePath,
    //            Kind = node.NodeKind,
    //            CopyFromPath = node.CopyInfo?.CopyFromPath,
    //            CopyFromRevision = node.CopyInfo?.CopyFromRevision,
    //        };

    private static CopyInfo InitializeNodeCopyInfo(DumpNode node)
    {
        return node.CopyInfo ??= new CopyInfo();
    }

    private void Log(string message)
    {
        if (_options.Verbose)
        {
            Console.WriteLine(message);
        }
    }

    private EDumpNodeKind ParseNodeKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return EDumpNodeKind.None;
        }

        // Normalize the kind string.
        var normalizedKind = kind.Trim().ToLowerInvariant();
        return normalizedKind switch
            {
                "file" => EDumpNodeKind.File,
                "dir" or "directory" => EDumpNodeKind.Directory,
                "symlink" or "symboliclink" => EDumpNodeKind.SymbolicLink,
                _ => EDumpNodeKind.Unknown
            };
    }

    private string? ProcessAddProperties(
        IMixedDataReader reader,
        string? currentLine,
        DumpNode node)
    {
        var line = ProcessCopyFromProperties(reader, currentLine, node);

        if (node.NodeKind == EDumpNodeKind.Directory && node.CopyInfo != null)
        {
            // For directories with copy info, we expect no more properties
            return null;
        }

        return line;
    }

    private string? ProcessBasicNodeProperties(
        IMixedDataReader reader,
        string? currentLine,
        DumpNode node)
    {
        // Action conversion logic
        Action<string> assignAction = action =>
            {
                // Normalize the action string.
                var normalizedAction = action;
                if (string.Equals(action, "change", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedAction = "modify";
                }

                if (Enum.TryParse<EDumpChangeAction>(normalizedAction, true, out var parsedAction))
                {
                    node.NodeAction = parsedAction;
                }
                else
                {
                    node.NodeAction = EDumpChangeAction.None;
                }
            };

        ReadNodeProperty(currentLine, NodeHeaders.NodePath, path => node.NodePath = path);

        var line = ReadNodeProperty(
            reader,
            NodeHeaders.NodeKind,
            kind => { node.NodeKind = ParseNodeKind(kind); });

        if (line == null)
        {
            line = ReadNodeProperty(reader, NodeHeaders.NodeAction, assignAction);
        }
        else
        {
            ReadNodeProperty(line, NodeHeaders.NodeAction, assignAction);
        }

        return line;
    }

    private string? ProcessContentHashProperties(
        IMixedDataReader reader,
        string? line,
        DumpNode node)
    {
        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.TextContentMD5,
                md5 => node.ContentInfo.TextContentMD5 = md5);
        }
        else
        {
            ReadNodeProperty(
                line,
                NodeHeaders.TextContentMD5,
                md5 => node.ContentInfo.TextContentMD5 = md5);
        }

        line = ReadNodeProperty(
            reader,
            NodeHeaders.TextContentSHA1,
            sha1 => node.ContentInfo.TextContentSHA1 = sha1);

        return line;
    }

    private string? ProcessCopyFromProperties(
        IMixedDataReader reader,
        string? currentLine,
        DumpNode node)
    {
        var line = currentLine;

        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.NodeCopyFromRev,
                value => InitializeNodeCopyInfo(node).CopyFromRevision = int.Parse(value));
        }
        else
        {
            ReadNodeProperty(
                line,
                NodeHeaders.NodeCopyFromRev,
                value => InitializeNodeCopyInfo(node).CopyFromRevision = int.Parse(value),
                true);
        }

        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.NodeCopyFromPath,
                path => InitializeNodeCopyInfo(node).CopyFromPath = path);
        }
        else
        {
            ReadNodeProperty(
                line,
                NodeHeaders.NodeCopyFromPath,
                path => InitializeNodeCopyInfo(node).CopyFromPath = path,
                true);
        }

        return line;
    }

    private string? ProcessCopySourceProperties(
        IMixedDataReader reader,
        string? line,
        DumpNode node)
    {
        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.TextCopySourceMD5,
                md5 => InitializeNodeCopyInfo(node).TextCopySourceMD5 = md5);
        }
        else
        {
            ReadNodeProperty(
                line,
                NodeHeaders.TextCopySourceMD5,
                md5 => InitializeNodeCopyInfo(node).TextCopySourceMD5 = md5,
                true);
        }

        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.TextCopySourceSHA1,
                sha1 => InitializeNodeCopyInfo(node).TextCopySourceSHA1 = sha1);
        }
        else
        {
            ReadNodeProperty(
                line,
                NodeHeaders.TextCopySourceSHA1,
                sha1 => InitializeNodeCopyInfo(node).TextCopySourceSHA1 = sha1,
                true);
        }

        return line;
    }

    private string? ProcessFileProperties(
        IMixedDataReader reader,
        string? currentLine,
        DumpNode node)
    {
        var line = ProcessCopySourceProperties(reader, currentLine, node);
        line = ProcessContentHashProperties(reader, line, node);
        return line;
    }

    private string? ProcessLengthProperties(
        IMixedDataReader reader,
        string? line,
        DumpNode node)
    {
        line = ReadOptionalPropContentLength(reader, line, node);
        line = ReadOptionalTextContentLength(reader, line, node);
        line = ReadOptionalContentLength(reader, line, node);

        return line;
    }

    private string? ProcessNodeContent(
        IMixedDataReader reader,
        DumpNode node,
        IParserActions parserActions)
    {
        string? line = reader.ReadLine();

        if (!node.ContentInfo.FileContentLength.HasValue || node.NodeKind != EDumpNodeKind.File)
        {
            return line;
        }

        var fileName = Path.GetFileName(node.NodePath);
        var isBinaryFile = node.Properties.ContainsKey("svn:mime-type");
        if (parserActions.IsBinaryFile(fileName))
        {
            isBinaryFile = true;
        }
        else if (isBinaryFile)
        {
            parserActions.AddBinaryFile(fileName);
        }

        if (isBinaryFile)
        {
            node.ContentInfo.BinaryContent =
                reader.ReadBinary(node.ContentInfo.FileContentLength.Value);
        }
        else
        {
            node.ContentInfo.TextContent =
                reader.ReadText(node.ContentInfo.FileContentLength.Value);
        }

        parserActions.NodeContentUpdated(node);
        return line;
    }

    private string? ReadNodeHeader(IMixedDataReader reader, string? currentLine, DumpNode node)
    {
        var line = ProcessBasicNodeProperties(reader, currentLine, node);

        if (node.NodeAction == EDumpChangeAction.Delete)
        {
            return null;
        }

        if (node.NodeAction == EDumpChangeAction.Add)
        {
            line = ProcessAddProperties(reader, line, node);
        }

        if (node.NodeKind == EDumpNodeKind.File)
        {
            line = ProcessFileProperties(reader, line, node);
        }

        line = ProcessLengthProperties(reader, line, node);
        return line;
    }

    private string? ReadNodeProperty(
        IMixedDataReader reader,
        string expectedKey,
        Action<string> setter)
    {
        var line = reader.ReadLine();
        if (line != null && line.StartsWith(expectedKey))
        {
            setter(line[expectedKey.Length..].Trim());
            return null;
        }

        return line;
    }

    private string? ReadNodeProperty(
        string? currentLine,
        string expectedKey,
        Action<string> setter,
        bool optional = false)
    {
        if (currentLine != null && currentLine.StartsWith(expectedKey))
        {
            setter(currentLine[expectedKey.Length..].Trim());
            return null;
        }

        if (!optional)
        {
            throw new InvalidDataException($"Expected '{expectedKey}', but found '{currentLine}'");
        }

        return currentLine;
    }

    private string? ReadOptionalContentLength(
        IMixedDataReader reader,
        string? line,
        DumpNode node)
    {
        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.ContentLength,
                value =>
                    {
                        node.ContentInfo.ContentLength = int.Parse(value);
                        Log($"  Content Length: {node.ContentInfo.ContentLength}");
                    });
        }
        else
        {
            line = ReadNodeProperty(
                line,
                NodeHeaders.ContentLength,
                value =>
                    {
                        node.ContentInfo.ContentLength = int.Parse(value);
                        Log($"  Text Content Length: {node.ContentInfo.ContentLength}");
                    },
                true);
        }

        return line;
    }

    private string? ReadOptionalPropContentLength(
        IMixedDataReader reader,
        string? line,
        DumpNode node)
    {
        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.PropContentLength,
                value =>
                    {
                        node.PropContentLength = int.Parse(value);
                        Log($"  Prop Content Length: {node.PropContentLength}");
                    });
        }
        else
        {
            line = ReadNodeProperty(
                line,
                NodeHeaders.PropContentLength,
                value =>
                    {
                        node.PropContentLength = int.Parse(value);
                        Log($"  Prop Content Length: {node.PropContentLength}");
                    },
                true);
        }

        return line;
    }

    private string? ReadOptionalTextContentLength(
        IMixedDataReader reader,
        string? line,
        DumpNode node)
    {
        if (line == null)
        {
            line = ReadNodeProperty(
                reader,
                NodeHeaders.TextContentLength,
                value =>
                    {
                        node.ContentInfo.FileContentLength = int.Parse(value);
                        Log($"  Text Content Length: {node.ContentInfo.FileContentLength}");
                    });
        }
        else
        {
            line = ReadNodeProperty(
                line,
                NodeHeaders.TextContentLength,
                value =>
                    {
                        node.ContentInfo.FileContentLength = int.Parse(value);
                        Log($"  Text Content Length: {node.ContentInfo.FileContentLength}");
                    },
                true);
        }

        return line;
    }
}
