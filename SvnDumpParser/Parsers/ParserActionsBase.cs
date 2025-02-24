using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;

namespace SvnDumpParser.Parsers
{
    public abstract class ParserActionsBase : IParserActions
    {
        private readonly HashSet<string> _fileNames;

        protected ParserActionsBase()
        {
            _fileNames = new HashSet<string>();
        }

        public abstract void AddNode(DumpNode node);

        public abstract void AddRevision(DumpRevision revision);

        public abstract DumpNode CreateNode();

        public abstract DumpRevision CreateRevision();

        public virtual void EndParsing()
        {
            //do nothing
        }

        public abstract void NodeContentUpdated(DumpNode node);

        public virtual void StartParsing(string dumpFilePath)
        {
            //do nothing
        }

        public void AddBinaryFile(string fileName)
        {
            _fileNames.Add(fileName);
        }

        public bool IsBinaryFile(string fileName)
        {
            return _fileNames.Contains(fileName); // Use HashSet.Contains
        }
    }
}
