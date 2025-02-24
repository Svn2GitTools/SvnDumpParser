using SvnDumpParser;
using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;

namespace SvnDumpParserDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Path to SVN dump file
            string dumpFilePath = String.Empty;
            bool verbose = false; // Add verbose flag

            Console.WriteLine("SVN Dump Parser Demo");

            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v" || args[i] == "--verbose")
                {
                    verbose = true;
                }
                else if (string.IsNullOrEmpty(dumpFilePath))
                {
                    dumpFilePath = args[i];
                }
            }

            if (string.IsNullOrEmpty(dumpFilePath))
            {
                Console.WriteLine("Please provide a valid SVN dump file name as an argument.");
                return;
            }

            // Create a parser service instance
            ISvnDumpParser svnDumpParser = new SvnDumpParserService();

            // Create step-based parser actions
            var parserActions = new StepParserActions();

            parserActions.OnRevisionAdded = revision =>
                {
                    if (!verbose)
                    {
                        string output = $"Revision: {revision.Number} - {revision.Author} - {revision.Date} - {revision.Comment}";

                        if (output.EndsWith("\r\n"))
                        {
                            Console.WriteLine(output.Substring(0, output.Length - 2)); // Remove \r\n
                        }
                        else if (output.EndsWith("\n"))
                        {
                            Console.WriteLine(output.Substring(0, output.Length - 1)); // Remove \n
                        }
                        else
                        {
                            Console.WriteLine(output);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"-------------------------------------");
                    }
                };

            parserActions.OnParsingEnded = () =>
                {
                    Console.WriteLine("Parsing completed successfully.");
                };

            // Configure parser options (optional)
            var parserOptions = new ParserOptions
                                    {
                                        Verbose = verbose 
                                    };

            try
            {
                // Parse the dump file
                svnDumpParser.Parse(dumpFilePath, parserActions, parserOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during parsing: {ex.Message}");
            }

        }
    }
}
