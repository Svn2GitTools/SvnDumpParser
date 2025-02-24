namespace SvnDumpParser.Interfaces;

public interface IMixedDataReader : IDisposable
{
    /// <summary>
    /// Indicates whether the end of the stream has been reached.
    /// </summary>
    bool EndOfStream { get; }

    /// <summary>
    /// Reads a fixed number of bytes and returns them as raw binary data.
    /// </summary>
    byte[] ReadBinary(int size);

    /// <summary>
    /// Reads a UTF-8 string until a newline ('\n') is encountered.
    /// </summary>
    string ReadLine();

    /// <summary>
    /// Reads the next non-empty line, skipping empty ones. Returns null if EOF is reached.
    /// </summary>
    string? ReadNextNonEmptyLine();

    /// <summary>
    /// Reads a fixed number of bytes and converts them into a UTF-8 string.
    /// </summary>
    string ReadText(int size);

    /// <summary>
    /// Skips an expected empty line. Throws an exception if the line is not empty.
    /// </summary>
    void SkipEmptyLine();
}
