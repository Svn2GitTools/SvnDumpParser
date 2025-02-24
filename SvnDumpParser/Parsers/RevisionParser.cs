using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;

namespace SvnDumpParser.Parsers
{
    public class RevisionParser : ParserBase
    {
        private const string ContentLengthKey = "Content-length:";

        private const string NodePathKey = "Node-path:";

        private const string PropContentLengthKey = "Prop-content-length:";

        public const string RevisionNumberKey = "Revision-number:";

        public RevisionParser(ParserOptions options)
            : base(options)
        {
        }

        public override string? Parse(
            IMixedDataReader reader,
            IParserActions parserActions,
            string? currentLine = null)
        {
            Log("Parsing Revision...");
            //var revision = new Revision();
            DumpRevision revision = parserActions.CreateRevision();
            var nodeParser = new NodeParser(_options);
            string? line = currentLine ?? reader.ReadLine(); // Use provided line or read a new one
            if (line == null)
            {
                return null; // Or throw an exception, depending on your error handling strategy
            }

            if (line.StartsWith(RevisionNumberKey))
            {
                revision.Number = ParseIntValue(line, RevisionNumberKey);
                Log($"  Revision Number: {revision.Number}");
            }
            else
            {
                throw new InvalidDataException(
                    $"Expected '{RevisionNumberKey}', but found '{line}'");
            }

            // Expect Prop-content-length:
            line = reader.ReadLine();
            if (line != null && line.StartsWith(PropContentLengthKey))
            {
                int propContentLength = ParseIntValue(line, PropContentLengthKey);
                Log($"  Prop Content Length: {propContentLength}");
                //     ParseProperties(reader, propContentLength);
            }
            else
            {
                throw new InvalidDataException(
                    $"Expected '{PropContentLengthKey}', but found '{line}'");
            }

            //Expect Content-Length
            string contentLine = reader.ReadLine();
            if (contentLine != null && contentLine.StartsWith(ContentLengthKey))
            {
                int contentLength = ParseIntValue(contentLine, ContentLengthKey);
                Log($"  Content Length: {contentLength}");
                ParseProperties(reader, contentLength, revision.Properties);
                // line end symbol after properties
                reader.SkipEmptyLine();
                // empty line after properties
                reader.SkipEmptyLine();

                //Skip Content
                //ReadExact(reader, contentLength);
            }
            else
            {
                throw new InvalidDataException(
                    $"Expected '{ContentLengthKey}', but found '{line}'");
            }

            MapValues(revision);

            bool isRevisionNext = false;

            line = null;

            //Parse Node Entries
            while (!reader.EndOfStream)
            {
                if (line == null)
                {
                    line = reader.ReadLine();
                }

                if (line == null)
                {
                    break;
                }

                if (line.StartsWith(RevisionNumberKey))
                {
                    isRevisionNext = true;
                    break;
                }

                if (line.StartsWith(NodePathKey))
                {
                    line = nodeParser.Parse(reader, parserActions, line);
                }
                else if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                else
                {
                    break;
                }
            }

            parserActions.AddRevision(revision);
            //Revisions.Add(revision);

            if (isRevisionNext)
            {
                return line;
            }

            return null;
        }

        private void MapValues(DumpRevision revision)
        {
            if (revision.Properties.ContainsKey("svn:author"))
            {
                revision.Author = revision.Properties["svn:author"];
            }

            if (revision.Properties.ContainsKey("svn:date"))
            {
                if (DateTime.TryParse(revision.Properties["svn:date"], out DateTime date))
                {
                    revision.Date = date;
                }
            }

            if (revision.Properties.ContainsKey("svn:log"))
            {
                revision.Comment = revision.Properties["svn:log"];
            }
        }

        //private void ParseProperties(StringReader reader, int propContentLength)
        //{
        //    Log("  Parsing Properties...");

        //    string line;
        //    int bytesRead = 0;

        //    while ((line = reader.ReadLine()) != null)
        //    {
        //        line = line.Trim();
        //        //Check if props end
        //        if (line == PropsEndKey)
        //        {
        //            break;
        //        }

        //        if (line.StartsWith(PropertyKey))
        //        {
        //            var pair = ReadProperty(reader, line, PropertyKey);
        //            Revision.Properties.Add(pair.Key, pair.Value);

        //            Log($"   Key: {pair.Key}, Value: {pair.Value}");
        //        }
        //        else
        //        {
        //            throw new InvalidDataException(
        //                $"Expected property key ('{PropertyKey}') or '{PropsEndKey}', but found '{line}'");
        //        }

        //        if (bytesRead > propContentLength)
        //        {
        //            throw new InvalidDataException(
        //                $"Prop Content Length exceeded. Expected:{propContentLength} Actual:{bytesRead}");
        //        }
        //    }

        //    Log("  Properties Parsed.");
        //}

        //private (string Key, string Value) ReadProperty(
        //    StringReader reader,
        //    string line,
        //    string propertyKey)
        //{
        //    int keyLength = ParseIntValue(line, propertyKey);
        //    // Read key
        //    string key = ReadExact(reader, keyLength);
        //    line = reader.ReadLine(); //Remove the newline after the key

        //    // Read next
        //    line = reader.ReadLine();

        //    if (line != null && line.StartsWith(ValueKey))
        //    {
        //        int valueLength = ParseIntValue(line, ValueKey);
        //        // Read Value
        //        string value = ReadExact(reader, valueLength);
        //        line = reader.ReadLine(); //Remove the newline after the value
        //        return (key, value);
        //    }

        //    throw new InvalidDataException(
        //        $"Expected value key ('{ValueKey}'), but found '{line}'");
        //}
    }
}
