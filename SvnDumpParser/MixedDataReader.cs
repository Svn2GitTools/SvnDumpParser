using SvnDumpParser.Interfaces;
using System.Text;

namespace SvnDumpParser;

public class MixedDataReader : IMixedDataReader
{
    private readonly BinaryReader _br;

    private readonly FileStream _fs;

    private bool _disposed;

    private const string ValueKey = "V "; // Prefix for value length lines

    /// <summary>
    /// Checks whether the stream has reached the end.
    /// </summary>
    public bool EndOfStream => _fs.Position >= _fs.Length;

    public MixedDataReader(string filePath)
    {
        _fs = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: false);
        _br = new BinaryReader(_fs);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// Reads a single property key-value pair from the stream.
    /// </summary>
    public (string Key, string Value) ReadProperty(string line, string propertyKey)
    {
        // Parse the key length
        int keyLength = ParseIntValue(line, propertyKey);

        // Read the key
        string key = ReadText(keyLength);

        // Consume the newline after the key
        ReadLine();

        // Read the next line for the value prefix
        string? valueLine = ReadLine();
        if (valueLine != null && valueLine.StartsWith(ValueKey))
        {
            // Parse the value length
            int valueLength = ParseIntValue(valueLine, ValueKey);

            // Read the value
            string value = ReadText(valueLength);

            // Consume the newline after the value
            ReadLine();

            return (key, value);
        }

        throw new InvalidDataException($"Expected value key ('{ValueKey}'), but found '{valueLine}'");
    }
    /// <summary>
    /// Parses an integer from a line with a given prefix.
    /// </summary>
    private int ParseIntValue(string line, string prefix)
    {
        if (!line.StartsWith(prefix))
        {
            throw new InvalidDataException($"Expected prefix '{prefix}', but found '{line}'");
        }

        return int.Parse(line.Substring(prefix.Length).Trim());
    }

    /// <summary>
    /// Reads a fixed number of bytes and returns them as raw binary data.
    /// </summary>
    public byte[] ReadBinary(int size)
    {
        return _br.ReadBytes(size);
    }

    /// <summary>
    /// Reads a UTF-8 string until a newline ('\n') is encountered.
    /// </summary>
    public string ReadLine()
    {
        using (MemoryStream ms = new MemoryStream())
        {
            int b;
            while ((b = _br.ReadByte()) != -1 && b != '\n') // Stop at EOF or newline
            {
                ms.WriteByte((byte)b);
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    /// <summary>
    /// Reads the next non-empty line, skipping empty ones. Returns null if EOF is reached.
    /// </summary>
    public string? ReadNextNonEmptyLine()
    {
        while (!EndOfStream)
        {
            string line = ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                return line;
            }
        }

        return null;
    }

    /// <summary>
    /// Reads a fixed number of bytes and converts them into a UTF-8 string.
    /// </summary>
    public string ReadText(int size)
    {
        byte[] buffer = _br.ReadBytes(size);
        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// Skips an expected empty line. Throws an exception if the line is not empty.
    /// </summary>
    public void SkipEmptyLine()
    {
        string line = ReadLine();
        if (!string.IsNullOrEmpty(line))
        {
            throw new InvalidDataException(
                $"Invalid SVN dump format: Expected empty line, but found: {line}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _br?.Dispose(); // This disposes both BinaryReader and FileStream
            }

            _disposed = true;
        }
    }

    ~MixedDataReader()
    {
        Dispose(false);
    }
}
