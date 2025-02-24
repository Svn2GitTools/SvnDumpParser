using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;
using SvnDumpParser.Parsers;

namespace SvnDumpParser
{
    public class SvnDumpParserService : ISvnDumpParser
    {
        public void Parse(
            string dumpFilePath,
            IParserActions parserActions,
            ParserOptions options)
        {
            //List<Revision> revisions = new();
            parserActions.StartParsing(dumpFilePath);

            if (!File.Exists(dumpFilePath))
            {
                throw new FileNotFoundException($"File not found: {dumpFilePath}");
            }

            try
            {
                using (MixedDataReader reader = new(dumpFilePath))
                {
                    var header = new DumpFileHeader();
                    var headerParser = new HeaderParser(header, options);
                    headerParser.Parse(reader, parserActions);
                    string? line = null;

                    while (!reader.EndOfStream)
                    {
                        if (line == null)
                        {
                            line = reader.ReadLine();
                        }

                        if (line != null && line.StartsWith(RevisionParser.RevisionNumberKey))
                        {
                            var revisionParser = new RevisionParser(options);
                            line = revisionParser.Parse(reader, parserActions, line);
                        }
                        else
                        {
                            throw new InvalidDataException(
                                $"Expected '{RevisionParser.RevisionNumberKey}', but found '{line}'");
                        }
                    }

                    parserActions.EndParsing();
                }
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw;
            }

            //return revisions;
        }
    }
}
