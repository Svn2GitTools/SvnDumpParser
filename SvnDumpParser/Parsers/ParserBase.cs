using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;

namespace SvnDumpParser.Parsers
{
    public abstract class ParserBase
    {
        protected readonly ParserOptions _options;

        public ParserBase(ParserOptions options)
        {
            _options = options;
        }

        public abstract string? Parse(
            IMixedDataReader reader,
            IParserActions parserActions,
            string? currentLine = null);

        /// <summary>
        /// Reads the next non empty line.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? ReadNextNonEmptyLine(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    return line;
                }
            }

            return null; // Return null if the end of the stream is reached
        }

        protected int ParseIntValue(string line, string prefix)
        {
            string valueString = line.Substring(prefix.Length).Trim();
            if (int.TryParse(valueString, out int value))
            {
                return value;
            }

            throw new InvalidDataException(
                $"Invalid SVN dump format: Could not parse integer value from line: {line}");
        }

        //protected string ReadExact(StreamReader reader, int length)
        //{
        //    char[] buffer = new char[length];
        //    int bytesRead = reader.Read(buffer, 0, length);
        //    if (bytesRead != length)
        //    {
        //        // Handle the case where the stream ends prematurely
        //        return new string(buffer, 0, bytesRead); // Or throw an exception.
        //    }

        //    return new string(buffer);
        //}

        protected string ReadExact(TextReader reader, int length)
        {
            char[] buffer = new char[length];
            int bytesRead = reader.Read(buffer, 0, length);
            if (bytesRead != length)
            {
                // Handle the case where the stream ends prematurely
                throw new InvalidDataException(
                    $"Invalid SVN dump format: Could not read expected number of bytes. Expected: {length}, Actual: {bytesRead}");
            }

            return new string(buffer);
        }

        protected string ReadLimitedString(StreamReader reader, int length, int? maxLength)
        {
            if (maxLength.HasValue && length > maxLength.Value)
            {
                throw new InvalidDataException(
                    $"Invalid SVN dump format: Property length exceeds maximum allowed length. Length: {length}, MaxLength: {maxLength.Value}");
            }

            char[] buffer = new char[length];
            int bytesRead = reader.Read(buffer, 0, length);

            if (bytesRead != length)
            {
                throw new InvalidDataException(
                    $"Invalid SVN dump format: Could not read expected number of bytes. Expected: {length}, Actual: {bytesRead}");
            }

            return new string(buffer);
        }

        //protected void SkipEmptyLine(TextReader reader)
        //{
        //    string line = reader.ReadLine();
        //    if (!string.IsNullOrEmpty(line))
        //    {
        //        throw new InvalidDataException(
        //            "Invalid SVN dump format: Expected empty line, but found: " + line);
        //    }
        //}

        protected void Log(string message)
        {
            if (_options.Verbose)
            {
                Console.WriteLine(message);
            }
        }

        protected void ParseProperties(
            IMixedDataReader reader,
            int contentLength,
            Dictionary<string, string> properties)
        {
            if (contentLength > 0)
            {
                PropertyParserHelper propertyParser = new(properties, _options);
                //string propertiesBlock = ReadExact(reader, contentLength);
                string propertiesBlock = reader.ReadText(contentLength);
                using (StringReader propertiesReader = new(propertiesBlock))
                {
                    propertyParser.Parse(propertiesReader);

                    foreach (var property in properties)
                    {
                        //properties.Add(property.Key, property.Value);

                        Log($"   Key: {property.Key}, Value: {property.Value}");
                    }
                }
            }

            //skip '\n' after properties end
            //reader.SkipEmptyLine();
            //StreamReader? streamReader = reader as StreamReader;
            //int? readByte = streamReader?.BaseStream.ReadByte();

        }
    }
}
