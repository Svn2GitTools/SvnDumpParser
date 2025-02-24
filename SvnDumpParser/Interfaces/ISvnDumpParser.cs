using SvnDumpParser.Models;

namespace SvnDumpParser.Interfaces
{
    public interface ISvnDumpParser
    {
        void Parse(
            string dumpFilePath,
            IParserActions parserActions,
            ParserOptions options);
    }
}
