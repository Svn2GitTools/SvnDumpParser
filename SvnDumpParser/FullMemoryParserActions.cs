using SvnDumpParser.Models;
using SvnDumpParser.Parsers;

namespace SvnDumpParser
{
    public class FullMemoryParserActions : ParserActionsBase
    {
        public DumpNode CurrentNode { get; private set; }

        public DumpRevision CurrentRevision { get; private set; }

        public List<DumpRevision> Revisions { get; } = new();

        public override void AddNode(DumpNode node)
        {
            CurrentRevision.AddNode(node);
        }

        public override void AddRevision(DumpRevision revision)
        {
            Revisions.Add(revision);
        }

        public override DumpNode CreateNode()
        {
            CurrentNode = new DumpNode();
            return CurrentNode;
        }

        public override DumpRevision CreateRevision()
        {
            CurrentRevision = new DumpRevision();
            return CurrentRevision;
        }

        public override void NodeContentUpdated(DumpNode node)
        {
        }
    }
}
