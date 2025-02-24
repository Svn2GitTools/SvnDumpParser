using System.Globalization;
using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;

namespace SvnDumpParser.Parsers
{
    public class HeaderParser : ParserBase
    {
        private const string FormatVersionKey = "SVN-fs-dump-format-version:";

        private const string UuidKey = "UUID:";

       
        public DumpFileHeader Header { get; init; }

        public HeaderParser(DumpFileHeader header, ParserOptions options):base(options)
        {
            Header = header;
        }

        public override string? Parse(
            IMixedDataReader reader,
            IParserActions parserActions,
            string? currentLine = null)
        {
            Log("Parsing Header...");

            string line;
            int lineNumber = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                line = line.Trim();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (line.StartsWith(FormatVersionKey))
                {
                    if (TryParseVersion(line, out int version))
                    {
                        Header.Version = version;
                        Log($"  Format Version: {version}");
                    }
                    else
                    {
                        // Throw directly with formatted message and line number
                        throw new InvalidDataException(
                            $"Error parsing header (line {lineNumber}): Invalid format version: {line.Substring(FormatVersionKey.Length).Trim()}");
                    }
                }
                else if (line.StartsWith(UuidKey))
                {
                    Header.UUID = line.Substring(UuidKey.Length).Trim();
                    Log($"  UUID: {Header.UUID}");
                    break;
                }
                else
                {
                    throw new InvalidDataException(
                        $"Error parsing header (line {lineNumber}): Unexpected line in header: {line}");
                }
            }
            reader.SkipEmptyLine();

            //Log(Header.ToString());
            return null;
        }

        private void Log(string message)
        {
            if (_options.Verbose)
            {
                Console.WriteLine(message);
            }
        }

        private bool TryParseVersion(string line, out int version)
        {
            string versionString = line.Substring(FormatVersionKey.Length).Trim();
            return int.TryParse(
                versionString,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out version);
        }
    }
}
