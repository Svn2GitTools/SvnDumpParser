using SvnDumpParser.Models;

namespace SvnDumpParser.Interfaces
{
    public interface IParserActions
    {
        void AddBinaryFile(string fileName);

        void AddNode(DumpNode node);

        void AddRevision(DumpRevision revision);

        DumpNode CreateNode();

        DumpRevision CreateRevision();

        void EndParsing();

        bool IsBinaryFile(string fileName);

        void NodeContentUpdated(DumpNode node);

        void StartParsing(string dumpFilePath);
    }
}
