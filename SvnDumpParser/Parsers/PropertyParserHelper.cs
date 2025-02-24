using SvnDumpParser.Models;

namespace SvnDumpParser.Parsers;

public class PropertyParserHelper
{
    private const string PropertyKey = "K ";

    private const string PropsEndKey = "PROPS-END";

    private const string ValueKey = "V ";

    private readonly ParserOptions _options;

    private readonly Dictionary<string, string> _properties;

    public PropertyParserHelper(Dictionary<string, string> properties, ParserOptions options)
    {
        _properties = properties;
        _options = options;
    }

    public string? Parse(StringReader reader, string? currentLine = null)
    {
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // Check for end of properties
            if (line == PropsEndKey)
            {
                break;
            }

            if (line.StartsWith(PropertyKey))
            {
                var (key, value) = ReadProperty(reader, line, PropertyKey);
                _properties[key] = value;

                // Track bytes read
                //bytesRead += key.Length + value.Length + 4; // +4 for newlines
            }
            else
            {
                throw new InvalidDataException(
                    $"Expected property key ('{PropertyKey}') or '{PropsEndKey}', but found '{line}'");
            }

            // Validate content length if specified
            //if (maxLength.HasValue && bytesRead > maxLength.Value)
            //    throw new InvalidDataException(
            //        $"Property content length exceeded. Expected: {maxLength.Value}, Actual: {bytesRead}");
        }

        return null;
    }

    /// <summary>
    /// Reads a single property key-value pair.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="line">The current line containing the property key length.</param>
    /// <param name="propertyKey">The prefix indicating a property key (e.g., "K ").</param>
    /// <returns>A tuple containing the property key and value.</returns>
    public (string Key, string Value) ReadProperty(
        TextReader reader,
        string line,
        string propertyKey)
    {
        // Parse the key length
        int keyLength = ParseIntValue(line, propertyKey);

        // Read the key
        string key = ReadExact(reader, keyLength);
        //bytesRead += key.Length;

        // Consume the newline after the key
        reader.ReadLine();
        //bytesRead++;

        // Read the next line for the value prefix
        line = reader.ReadLine();
        if (line != null && line.StartsWith(ValueKey))
        {
            // Parse the value length
            int valueLength = ParseIntValue(line, ValueKey);

            // Read the value
            string value = ReadExact(reader, valueLength);
            //bytesRead += value.Length;

            // Consume the newline after the value
            reader.ReadLine();
            //bytesRead++;

            return (key, value);
        }

        throw new InvalidDataException($"Expected value key ('{ValueKey}'), but found '{line}'");
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
    /// Reads an exact number of characters from a reader.
    /// </summary>
    private string ReadExact(TextReader reader, int length)
    {
        char[] buffer = new char[length];
        int read = reader.Read(buffer, 0, length);

        if (read != length)
        {
            throw new InvalidDataException($"Expected to read {length} characters but got {read}");
        }

        return new string(buffer);
    }
}
