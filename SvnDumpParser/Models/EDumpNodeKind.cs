namespace SvnDumpParser.Models
{
    public enum EDumpNodeKind
    {
        None,

        File, // "file"

        Directory, // "dir"

        Unknown, // not in dump file

        SymbolicLink, // not in dump file
    }
}
