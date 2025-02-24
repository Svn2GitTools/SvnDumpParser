using SvnDumpParser.Models;
using SvnDumpParser.Parsers;

namespace SvnDumpParser
{
    public class StepParserActions : ParserActionsBase
    {
        // Define simple delegates for the parser actions without including CurrentNode.
        public Action<DumpNode>? OnNodeAdded;

        public Action<DumpNode>? OnNodeContentUpdated;

        public Action? OnParsingEnded;

        public Action<DumpRevision>? OnRevisionAdded;

        public DumpNode CurrentNode { get; private set; }

        public DumpRevision CurrentRevision { get; private set; }

        public override void AddNode(DumpNode node)
        {
            CurrentRevision.AddNode(node);
            // Invoke the delegate if assigned.
            OnNodeAdded?.Invoke(node);
        }

        public override void AddRevision(DumpRevision revision)
        {
            // Invoke the delegate if assigned.
            OnRevisionAdded?.Invoke(revision);
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

        public override void EndParsing()
        {
            base.EndParsing();
            // Invoke the delegate if assigned.
            OnParsingEnded?.Invoke();
        }

        public override void NodeContentUpdated(DumpNode node)
        {
            // Invoke the delegate if assigned.
            OnNodeContentUpdated?.Invoke(node);
        }
    }
}
