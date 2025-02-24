# SvnDumpParser: A .NET Library for Parsing Subversion Dumps

[![NuGet Package](https://img.shields.io/nuget/v/SvnDumpParser.svg?style=flat-square)](https://www.nuget.org/packages/SvnDumpParser)

**SvnDumpParser** is a .NET 9 library for parsing Subversion (SVN) dump files. It provides a simple and efficient way to read and extract information from SVN dump streams programmatically.

## Features

*   **Parses SVN Dump Files:**  Core functionality to read and interpret the structure of SVN dump files.
*   **Detailed Revision Information:** Extracts detailed information about each revision, including revision number, author, date, log message, and changed nodes.
*   **Node and Property Parsing:**  Parses information about nodes (files and directories) within revisions, including paths, actions (add, delete, modify), and properties.
*   **Step-by-Step Parsing:** Supports step-by-step parsing, which is especially useful for handling large dump files efficiently and processing data incrementally.
*   **Extensible Parser Actions:**  Utilizes the `IParserActions` interface, allowing you to customize the parsing process and perform actions as revisions and nodes are parsed (e.g., store data in a database, convert to another format, etc.).
*   **Error Handling:** Provides robust error handling to gracefully manage issues within the dump file format.
*   **Verbose Output:** Includes a `Verbose` option for detailed logging during parsing, helpful for debugging.

## Getting Started

### Installation

You can install `SvnDumpParser` via NuGet:

```bash
dotnet add package SvnDumpParser
```

### Basic Usage

Here's a simple example of how to use `SvnDumpParser` to parse an SVN dump file and print out the revision numbers:

```csharp
using SvnDumpParser;
using SvnDumpParser.Interfaces;
using SvnDumpParser.Models;

// Path to your SVN dump file
string dumpFilePath = "path/to/your/svn_dump.dump";

// Create a parser service instance
ISvnDumpParser svnDumpParser = new SvnDumpParserService();

// Create step-based parser actions
var parserActions = new StepParserActions();

parserActions.OnRevisionAdded = revision =>
{
    Console.WriteLine($"Revision: {revision.RevisionNumber}");
};

// Configure parser options (optional)
var parserOptions = new ParserOptions
{
    Verbose = true // Enable verbose output if needed
};

try
{
    // Parse the dump file
    svnDumpParser.Parse(dumpFilePath, parserActions, parserOptions);

    Console.WriteLine("Parsing completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred during parsing: {ex.Message}");
}
```

**Explanation:**

1.  **Install the NuGet package:**  Add `SvnDumpParser` to your project.
2.  **Create `SvnDumpParserService`:** Instantiate the parser service to handle the parsing logic.
3.  **Implement `StepParserActions`:** Create an instance of `StepParserActions` to define actions to be performed during parsing. In this example, we are subscribing to the `OnRevisionAdded` event to print revision numbers.
4.  **Configure `ParserOptions` (Optional):**  You can create a `ParserOptions` object to set parsing configurations, such as enabling verbose output.
5.  **Call `Parse`:** Call the `svnDumpParser.Parse()` method, providing the dump file path, parser actions, and parser options.
6.  **Handle Events:** The `parserActions` will trigger events like `OnRevisionAdded` as the dump file is processed, allowing you to react to parsed data.

## Parser Options

The `ParserOptions` class allows you to configure the parsing behavior. Currently, the following options are available:

*   **`Verbose` (bool):** Enables verbose output during parsing. This can be helpful for debugging and understanding the parsing process. Default is `false`.

```csharp
var parserOptions = new ParserOptions
{
    Verbose = true
};
```

## Step-by-Step Parsing Actions

Here's a description of each action in `StepParserActions`:

**Delegates (Actions)**

These delegates allow you to hook into specific points during the parsing process and execute custom logic. They are designed for "step-by-step" processing of the SVN dump.

*   **`OnRevisionAdded: Action<DumpRevision>?`**
    *   **Triggered When:**  Invoked immediately after a new revision header is successfully parsed from the dump file. This happens at the start of processing each revision block in the dump.
    *   **Parameter:**  `DumpRevision revision` -  Provides a `DumpRevision` object. This object contains information about the newly parsed revision, such as:
        *   `RevisionNumber`: The revision number.
        *   `Author`: The author of the revision.
        *   `Date`: The date and time of the revision.
        *   `LogMessage`: The commit message for the revision.
        *   Initially, the `Nodes` list within `DumpRevision` will be empty. Nodes are added later as they are parsed within this revision.
    *   **Use Case:** This is the primary entry point for processing each revision in a step-by-step manner. You might use this action to:
        *   Start processing changes for a specific revision.
        *   Log the revision number being processed.
        *   Prepare resources needed for processing nodes within this revision.
        *   Convert the `DumpRevision` object to a Git commit representation (as seen in your `SvnDump2Git` example).

*   **`OnNodeAdded: Action<DumpNode>?`**
    *   **Triggered When:** Invoked immediately after a new node (representing a file or directory change within a revision) is parsed. This happens for each file or directory that is added, modified, or deleted in a revision.
    *   **Parameter:** `DumpNode node` - Provides a `DumpNode` object. This object contains information about a specific node change within the current revision, such as:
        *   `Path`: The path of the file or directory.
        *   `Action`: The type of change (e.g., "add", "delete", "modify").
        *   `NodeType`:  Indicates if it's a file or directory.
        *   `TextContent`: (If applicable) The text content of the file.
        *   `BinaryContent`: (If applicable) The binary content of the file.
        *   `Properties`: A collection of properties associated with the node.
    *   **Use Case:** This action allows you to process individual file or directory changes as they are parsed. You might use this to:
        *   Store node information in a database.
        *   Apply changes to a working directory.
        *   Convert node changes to Git index operations.
        *   Filter or analyze specific file types or paths.

*   **`OnNodeContentUpdated: Action<DumpNode>?`**
    *   **Triggered When:**  Invoked specifically when the *content* of a node (file content or directory properties) is parsed.  This is called *after* `OnNodeAdded` if the node has content or properties to be processed.  It's a separate action because node metadata (path, action) might be parsed before the actual content.
    *   **Parameter:** `DumpNode node` -  Provides the same `DumpNode` object that was provided in `OnNodeAdded`. By this point, the `DumpNode` object will have its `TextContent`, `BinaryContent`, or `Properties` populated (if applicable for the node type and action).
    *   **Use Case:** This action is useful when you need to process the actual *data* associated with a node change. You might use it to:
        *   Write file content to disk.
        *   Process binary file data.
        *   Extract and store node properties.
        *   Perform content-based analysis of file changes.

*   **`OnParsingEnded: Action?`**
    *   **Triggered When:** Invoked after the entire dump file has been parsed successfully, and the end of the dump stream is reached.
    *   **Parameter:**  None ( `Action?` with no generic type parameter)
    *   **Use Case:** This action allows you to perform cleanup or finalization tasks after parsing is complete. You might use it to:
        *   Close database connections.
        *   Finalize data processing.
        *   Perform a checkout operation in a Git conversion process (as in your `SvnDump2Git` example).
        *   Log completion status.

**Properties**

*   **`CurrentNode: DumpNode` (Read-only)**
    *   **Purpose:**  Holds the `DumpNode` object that is currently being processed by the parser. This property is set internally by the `StepParserActions` class and is primarily for internal use within the action methods.  While it's publicly accessible, in the context of the delegates (`OnNodeAdded`, `OnNodeContentUpdated`), you'll receive the relevant `DumpNode` object directly as a parameter, making direct access to `CurrentNode` usually unnecessary from *outside* the `StepParserActions` class itself.

*   **`CurrentRevision: DumpRevision` (Read-only)**
    *   **Purpose:** Holds the `DumpRevision` object that is currently being processed. Similar to `CurrentNode`, this is managed internally by `StepParserActions`.  In the `OnRevisionAdded` delegate, you get the `DumpRevision` as a parameter.  Direct external access to `CurrentRevision` is typically not needed when using the delegates.

**Overridden Methods (Internal Use)**

These methods are overridden from the `ParserActionsBase` class and are called internally by the `SvnDumpParserService` during the parsing process. They are responsible for creating objects and triggering the delegates. You generally don't call these methods directly when *using* the `SvnDumpParser` library.

*   **`AddNode(DumpNode node)`:** Called by the parser when a complete node (file/directory change) is parsed. It adds the `node` to the `CurrentRevision.Nodes` collection and then invokes the `OnNodeAdded` delegate, passing the `node` as a parameter.

*   **`AddRevision(DumpRevision revision)`:** Called by the parser when a complete revision header is parsed. It invokes the `OnRevisionAdded` delegate, passing the `revision` as a parameter.

*   **`CreateNode(): DumpNode`:** Called by the parser when it needs to create a new `DumpNode` object. It creates a new instance and sets it as the `CurrentNode` property.

*   **`CreateRevision(): DumpRevision`:** Called by the parser when it starts parsing a new revision. It creates a new `DumpRevision` object and sets it as the `CurrentRevision` property.

*   **`EndParsing()`:** Called by the parser when the entire dump file has been parsed. It invokes the `OnParsingEnded` delegate.

*   **`NodeContentUpdated(DumpNode node)`:** Called by the parser after the content (or properties) of a node have been parsed. It invokes the `OnNodeContentUpdated` delegate, passing the `node` as a parameter.

**In Summary:**

The `StepParserActions` class provides a powerful event-driven mechanism for processing SVN dump files step-by-step. By subscribing to the `OnRevisionAdded`, `OnNodeAdded`, `OnNodeContentUpdated`, and `OnParsingEnded` delegates, you can insert your own custom logic at various stages of the parsing process to extract, transform, or utilize the data from the SVN dump file.  The `CurrentNode` and `CurrentRevision` properties are mainly for internal management within the `StepParserActions` and are less likely to be directly used by consumers of the library.

## License

`SvnDumpParser` is released under the [MIT License](LICENSE).

