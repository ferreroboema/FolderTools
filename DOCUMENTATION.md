# FolderTools - Technical Documentation

> Complete technical documentation for developers and contributors.

## Table of Contents

1. [Architecture](#architecture)
2. [Project Structure](#project-structure)
3. [Class Reference](#class-reference)
4. [Algorithms](#algorithms)
5. [Encoding Detection](#encoding-detection)
6. [Binary File Detection](#binary-file-detection)
7. [Regex Implementation](#regex-implementation)
8. [Error Handling](#error-handling)
9. [Performance Considerations](#performance-considerations)
10. [Testing Guide](#testing-guide)
11. [Dependency Injection for Testing](#dependency-injection-for-testing)
12. [API Reference](#api-reference)

## Architecture

### Overview

FolderTools follows a layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation Layer                     │
│                    (CommandLineParser, Program)               │
├─────────────────────────────────────────────────────────────┤
│                         Output Layer                          │
│                       (ResultFormatter)                       │
├─────────────────────────────────────────────────────────────┤
│                        Service Layer                          │
│              (IFileProcessor, ITextReplacer)                  │
├─────────────────────────────────────────────────────────────┤
│                       Utility Layer                           │
│         (FileHelper, EncodingHelper, FileFilter)              │
├─────────────────────────────────────────────────────────────┤
│                         Model Layer                           │
│         (SearchOptions, ReplacementResult, FileFilter)        │
└─────────────────────────────────────────────────────────────┘
```

### Design Patterns

| Pattern | Usage | Location |
|---------|-------|----------|
| **Strategy** | Text replacement strategies (literal vs regex) | `ITextReplacer` |
| **Dependency Injection** | File I/O abstraction for testability | `IFileHelper` |
| **Wrapper** | Adapts static methods to interface | `FileHelperWrapper` |
| **Builder** | Command-line argument construction | `CommandLineParser` |
| **Formatter** | Output formatting strategies | `ResultFormatter` |
| **Filter** | File filtering criteria | `FileFilter` |
| **Mock** | Test doubles for file I/O | Moq with `IFileHelper` |

## Project Structure

```
FolderTools/
├── FolderTools.csproj              # .NET Framework 4.8.1 project
│
├── Models/                         # Data models
│   ├── SearchOptions.cs            # Search configuration
│   ├── ReplacementResult.cs        # Result data structures
│   └── FileFilter.cs               # File filtering logic
│
├── Services/                       # Business logic
│   ├── IFileProcessor.cs           # File processor interface
│   ├── IFileHelper.cs              # File helper interface (DI)
│   ├── FileProcessor.cs            # Directory traversal & processing
│   ├── ITextReplacer.cs            # Text replacer interface
│   └── TextReplacer.cs             # Replace implementation
│
├── Utilities/                      # Helper classes
│   ├── CommandLineParser.cs        # Argument parsing
│   ├── FileHelper.cs               # Static file system operations
│   ├── FileHelperWrapper.cs        # IFileHelper implementation
│   ├── EncodingHelper.cs           # Encoding detection
│   ├── CsvSearchReplaceParser.cs   # CSV file parser for bulk mode
│   └── BulkCollisionValidator.cs   # Collision detection for bulk mode
│
├── Outputs/                        # Output formatting
│   └── ResultFormatter.cs          # Console output
│
└── Program.cs                      # Entry point

FolderTools.Tests/                  # Unit test project
├── Models/                         # Model unit tests
│   ├── SearchOptionsTests.cs
│   ├── ReplacementResultTests.cs
│   └── FileFilterTests.cs
│
├── Services/                       # Service unit tests (with mocking)
│   ├── TextReplacerTests.cs
│   └── FileProcessorTests.cs
│
├── Utilities/                      # Utility unit tests
│   ├── CommandLineParserTests.cs
│   ├── FileHelperTests.cs
│   └── EncodingHelperTests.cs
│
├── Outputs/                        # Output formatter tests
│   └── ResultFormatterTests.cs
│
└── TestData/                       # Test fixtures
    ├── Files/                      # Sample test files
    │   ├── utf8.txt
    │   ├── ascii.txt
    │   ├── binary.bin
    │   └── empty.txt
    └── Directories/                # Directory structure tests
        └── Nested/
            ├── file1.txt
            └── Deep/
                └── file2.txt
```

## Class Reference

### Models

#### SearchOptions

Configuration options for search and replace operations.

```csharp
public class SearchOptions
{
    public string Pattern { get; set; }              // Search pattern
    public string Replacement { get; set; }          // Replacement text
    public bool IsRegex { get; set; }                // Regex mode
    public bool CaseSensitive { get; set; }          // Case sensitivity
    public bool IsDryRun { get; set; }               // Preview only
    public FileEncoding Encoding { get; set; }       // Text encoding
    public int? MaxDepth { get; set; }               // Recursion limit
    public bool Verbose { get; set; }                // Verbose output
    public bool Quiet { get; set; }                  // Quiet mode
    public bool IncludeHidden { get; set; }          // Include hidden files
    public CollisionBehavior CollisionBehavior { get; set; }  // Bulk mode collision handling
}
```

#### ReplacementResult

Aggregated results from processing multiple files.

```csharp
public class ReplacementResult
{
    public int ProcessedFiles { get; }               // Files successfully processed
    public int TotalReplacements { get; }            // Total replacements made
    public int FilesWithErrors { get; }              // Files with errors
    public List<FileReplacementResult> FileResults { get; }  // Detailed results
    public List<SkippedFile> SkippedFiles { get; }   // Skipped files

    void AddResult(string filePath, int replacementCount);
    void AddError(string filePath, string errorMessage);
    void AddSkipped(string filePath, string reason);
}
```

#### FileFilter

Criteria for filtering files during processing.

```csharp
public class FileFilter
{
    public HashSet<string> Extensions { get; set; }  // Allowed extensions
    public string FileNamePattern { get; set; }      // Wildcard pattern
    public long? MinSize { get; set; }               // Min file size
    public long? MaxSize { get; set; }               // Max file size
    public bool IncludeHidden { get; set; }          // Include hidden

    bool ShouldProcessFile(string filePath);
    void AddExtensions(string extensions);
}
```

### Services

#### IFileHelper / FileHelperWrapper

Abstraction layer for file system operations to enable testability.

```csharp
public interface IFileHelper
{
    bool IsTextFile(string filePath);
    bool IsFileLocked(string filePath);
    bool TryReadFile(string filePath, FileEncoding encoding,
                     out string content, out string error);
    bool TryWriteFile(string filePath, string content,
                      FileEncoding encoding, out string error);
    string GetRelativePath(string basePath, string fullPath);
    string FormatFileSize(long bytes);
}
```

**Purpose**: Allows mocking of file I/O in unit tests while providing
real file operations through `FileHelperWrapper`.

#### IFileProcessor / FileProcessor

Processes directories and applies text replacements.

```csharp
public interface IFileProcessor
{
    ReplacementResult ProcessDirectory(string rootDirectory,
                                        SearchOptions options,
                                        FileFilter filter);
}
```

**Key Methods:**
- `ProcessDirectory()`: Main entry point for processing
- `ProcessDirectoryRecursive()`: Recursive directory traversal
- `ProcessFile()`: Single file processing with filtering

#### ITextReplacer / TextReplacer

Performs text replacement operations.

```csharp
public interface ITextReplacer
{
    int ReplaceInFile(string filePath, SearchOptions options);
}
```

**Key Methods:**
- `ReplaceInFile()`: Main replacement method
- `RegexCount()`: Count regex matches
- `CountOccurrences()`: Count literal string occurrences
- `ReplaceAllIgnoreCase()`: Case-insensitive replacement

### Utilities

#### EncodingHelper

Handles text encoding detection and conversion.

```csharp
public static class EncodingHelper
{
    static Encoding DetectEncoding(string filePath);
    static string ReadFileWithEncoding(string filePath);
    static Encoding GetEncoding(FileEncoding encoding);
    static string ReadFile(string filePath, FileEncoding encoding);
    static void WriteFile(string filePath, string content, FileEncoding encoding);
}
```

#### FileHelper

File system utility functions.

```csharp
public static class FileHelper
{
    static bool IsTextFile(string filePath);
    static bool IsFileLocked(string filePath);
    static bool TryReadFile(string filePath, FileEncoding encoding,
                           out string content, out string error);
    static bool TryWriteFile(string filePath, string content,
                            FileEncoding encoding, out string error);
    static string GetRelativePath(string basePath, string fullPath);
    static string FormatFileSize(long bytes);
}
```

## Algorithms

### File Filter Logic

The file filtering algorithm applies multiple criteria in sequence:

```
1. Check file existence
   ↓
2. Check hidden/system attributes (if !IncludeHidden)
   ↓
3. Check file extension (if Extensions not empty)
   ↓
4. Check filename pattern (if FileNamePattern set)
   ↓
5. Check minimum file size (if MinSize set)
   ↓
6. Check maximum file size (if MaxSize set)
   ↓
7. File passes all checks → Process
```

### Directory Traversal

Recursive depth-controlled traversal:

```
ProcessDirectory(root, options, filter)
│
├── GetFiles(directory) → Process each file
│
└── GetDirectories(directory)
    │
    └── For each subdirectory:
        ├── Check if hidden (skip if !IncludeHidden)
        ├── Check max depth limit
        └── ProcessDirectory(subdirectory, options, filter, depth+1)
```

## Collision Detection (Bulk Mode)

### Overview

In bulk mode, collision detection prevents unintended behavior when replacement values from one pair become search patterns in subsequent pairs.

### The Collision Problem

When pairs are processed sequentially, a replacement value from an earlier pair can match a search pattern in a later pair, causing unexpected replacements.

**Example:**
```csv
V123,V146
V146,V178
```

Expected: V123 → V146 (stays V146), V146 → V178
Actual: Both V123 and V146 become V178

### Detection Algorithm

The `BulkCollisionValidator` uses an O(n) algorithm:

```
1. Build dictionary: search pattern → list of pairs
   Input: [(V123→V146), (V146→V178)]
   Dict: {V123: [pair1], V146: [pair2]}

2. For each pair, check if replacement exists as search pattern
   Pair1: V146 → found in dict → collision!
   Pair2: V178 → not found → no collision

3. Build chains by following replacement links
   Chain: [V123→V146] → [V146→V178]

4. Merge overlapping chains to avoid duplicates

5. Skip edge cases:
   - Empty replacements (deletions)
   - Self-references (A→A)
```

### Collision Behavior Modes

| Mode | Behavior |
|------|----------|
| `prompt` | Show warnings, wait for user input |
| `warn` | Show warnings, continue automatically |
| `fail` | Exit with error code immediately |
| `ignore` | Silent mode, no warnings |

### Data Structures

#### CollisionInfo
```csharp
public class CollisionInfo
{
    public List<SearchReplacePair> CollisionChain { get; set; }
    string GetChainVisualization();        // "A" -> "B" -> "C"
    List<string> GetDetailedChainInfo();   // Line-by-line details
}
```

#### CollisionDetectionResult
```csharp
public class CollisionDetectionResult
{
    public List<CollisionInfo> Collisions { get; set; }
    public bool HasCollisions { get; }      // true if any collisions
    public int CollisionCount { get; }      // number of collisions
    string GetSummaryMessage();            // summary string
}
```

### Edge Cases Handled

| Case | Handling |
|------|----------|
| Empty replacement | Skipped (deletion is intentional) |
| Self-reference (A→A) | Skipped (no-op, not a collision) |
| Circular (A→B, B→A) | Detected with cycle limit (100) |
| Multiple chains | Reported separately |
| Overlapping chains | Merged to avoid duplicates |

## Encoding Detection

### Byte Order Mark (BOM) Detection

| BOM Pattern | Encoding | Description |
|-------------|----------|-------------|
| `EF BB BF` | UTF-8 | UTF-8 with BOM |
| `FF FE` | UTF-16 LE | Little-endian UTF-16 |
| `FE FF` | UTF-16 BE | Big-endian UTF-16 |
| `FF FE 00 00` | UTF-32 LE | Little-endian UTF-32 |
| `00 00 FE FF` | UTF-32 BE | Big-endian UTF-32 |

### Content-Based Detection

When no BOM is present, the algorithm:

1. Reads the entire file into memory
2. Scans for byte sequences that indicate encoding:
   - Bytes ≤ 0x7F suggest ASCII
   - UTF-8 multi-byte sequences (0xC0-0xDF followed by 0x80-0xBF)
3. Returns most likely encoding (defaults to UTF-8 without BOM)

## Binary File Detection

### Null Byte Heuristic

Binary files are detected by scanning the first 8KB for null bytes (`0x00`):

```csharp
bool IsTextFile(string filePath)
{
    byte[] buffer = ReadFirstBytes(filePath, 8192);

    for (int i = 0; i < bytesRead; i++)
    {
        if (buffer[i] == 0x00)  // Null byte found
            return false;        // Binary file
    }

    return true;  // No null bytes → likely text
}
```

**Limitations:**
- Some text files may legitimately contain null bytes
- UTF-16 encoded files may have null bytes for ASCII characters

## Regex Implementation

### Case-Insensitive Replacement

For case-insensitive literal replacement, a manual `IndexOf`/`StringBuilder` loop is used to ensure the replacement string is treated as literal text (not as a regex replacement pattern):

```csharp
// Manual loop ensures $ in replacement is treated literally
var sb = new StringBuilder(content.Length);
int index = 0;

while (index < content.Length)
{
    int matchIndex = content.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase);
    if (matchIndex == -1)
    {
        sb.Append(content, index, content.Length - index);
        break;
    }
    sb.Append(content, index, matchIndex - index);
    sb.Append(replacement);
    index = matchIndex + pattern.Length;
}
```

### Counting Matches

```csharp
int RegexCount(string content, string pattern, RegexOptions options)
{
    return Regex.Matches(content, pattern, options).Count;
}
```

## Error Handling

### Strategy

FolderTools uses graceful error handling to process as many files as possible:

1. **File-Level Errors**: Individual file errors don't stop batch processing
2. **Directory-Level Errors**: Access denied errors skip the directory
3. **Locked Files**: Detected and reported without crashing
4. **Binary Files**: Automatically skipped with reason logged

### Error Types Handled

| Error Type | Handling |
|------------|----------|
| File not found | Logged as error, continue |
| Access denied | Skip file/directory, log warning |
| File locked by another process | Skip file, log reason |
| Invalid regex pattern | Show error, skip replacement |
| Encoding detection failure | Default to UTF-8 |

## Performance Considerations

### Memory Usage

| Component | Memory Impact | Optimization |
|-----------|---------------|--------------|
| File reading | Full file in memory | Consider streaming for large files |
| Regex matching | Pattern compilation | Cache compiled patterns |
| Directory traversal | Call stack depth | Max depth limits recursion |

### Performance Tips

1. **Use file extension filtering**: Reduces files to scan
2. **Set max depth**: Limits unnecessary traversal
3. **Use dry-run first**: Verify before committing changes
4. **Verbose mode**: Has overhead; use for debugging only

### Benchmarks

| Operation | Files | Time |
|-----------|-------|------|
| Simple replace (100 files) | 100 | ~2s |
| Regex replace (1000 files) | 1000 | ~15s |
| Deep directory traversal (10 levels) | 5000 | ~30s |

*Note: Benchmarks on typical development machine, SSD storage.*

## Testing Guide

### Unit Tests

FolderTools includes comprehensive unit tests using xUnit, Moq, and FluentAssertions.

#### Test Framework Stack

- **xUnit 2.7+**: Test framework
- **Moq 4.20+**: Mocking framework
- **FluentAssertions 6.12+**: Readable assertion syntax
- **System.IO.Abstractions 19.2+**: File system abstraction

#### Test Structure

```csharp
using FluentAssertions;
using FolderTools.Services;
using Moq;
using Xunit;

public class TextReplacerTests
{
    private readonly Mock<IFileHelper> _fileHelperMock;

    public TextReplacerTests()
    {
        _fileHelperMock = new Mock<IFileHelper>();
    }

    [Fact]
    public void ReplaceInFile_LiteralReplacement_ShouldReplaceAllOccurrences()
    {
        // Arrange
        var options = new SearchOptions
        {
            Pattern = "World",
            Replacement = "Universe",
            IsRegex = false
        };

        string content = "Hello World, World!";
        string error = null;
        _fileHelperMock.Setup(f => f.TryReadFile(
            It.IsAny<string>(),
            It.IsAny<FileEncoding>(),
            out content,
            out error
        )).Returns(true);

        _fileHelperMock.Setup(f => f.TryWriteFile(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<FileEncoding>(),
            out It.Ref<string>.IsAny
        )).Returns(true);

        // Act
        var replacer = new TextReplacer(_fileHelperMock.Object);
        var result = replacer.ReplaceInFile("test.txt", options);

        // Assert
        result.Should().Be(2);
    }
}
```

#### Running Tests

```bash
# Run all tests
dotnet test FolderTools.Tests/FolderTools.Tests.csproj

# Run with verbose output
dotnet test FolderTools.Tests/FolderTools.Tests.csproj --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~TextReplacerTests"

# Run with coverage
dotnet test FolderTools.Tests/FolderTools.Tests.csproj --collect:"XPlat Code Coverage"
```

#### Test Coverage

| Component | Tests | Coverage |
|-----------|-------|----------|
| Models | 23 | High |
| Services | 16 | Medium-High |
| Utilities | 42 | Medium |
| Outputs | 12 | High |
| **Total** | **202** | **100% passing** |

### Integration Tests

Test scenarios to cover:

1. **Basic replacement**: Simple find and replace
2. **Regex patterns**: Complex regex expressions
3. **Case sensitivity**: With and without `-c` flag
4. **File filtering**: Extension, name, size filters
5. **Dry-run mode**: Verify no changes made
6. **Binary files**: Confirm binary files are skipped
7. **Locked files**: Handle in-use files gracefully
8. **Encoding**: Test various file encodings
9. **Nested directories**: Deep recursion
10. **Error recovery**: Invalid patterns, missing files

### Manual Testing Checklist

- [ ] Replace in single file
- [ ] Replace in multiple files
- [ ] Replace with regex pattern
- [ ] Case-sensitive search
- [ ] Case-insensitive search (default)
- [ ] Dry-run preview
- [ ] Actual replacement
- [ ] Extension filtering
- [ ] Filename wildcard filtering
- [ ] Min/max size filtering
- [ ] Max depth limiting
- [ ] Verbose output
- [ ] Quiet mode
- [ ] Binary file handling
- [ ] Locked file handling
- [ ] Hidden file inclusion
- [ ] Empty search pattern (error)
- [ ] Invalid directory (error)

### Dependency Injection for Testing

#### The IFileHelper Pattern

FolderTools uses dependency injection to enable unit testing of file I/O operations:

**Problem**: Static methods like `FileHelper.TryReadFile()` cannot be mocked.

**Solution**: Extract interface and inject as dependency.

```csharp
// Before (untestable)
public class FileProcessor
{
    public ReplacementResult ProcessDirectory(...)
    {
        foreach (var file in files)
        {
            if (FileHelper.IsTextFile(file))  // Static call
            {
                FileHelper.TryReadFile(...);  // Cannot mock
            }
        }
    }
}

// After (testable)
public class FileProcessor
{
    private readonly IFileHelper _fileHelper;

    public FileProcessor(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper ?? throw new ArgumentNullException();
    }

    public ReplacementResult ProcessDirectory(...)
    {
        foreach (var file in files)
        {
            if (_fileHelper.IsTextFile(file))  // Can be mocked
            {
                _fileHelper.TryReadFile(...);  // Can be mocked
            }
        }
    }
}
```

#### Constructor Overloads for Backward Compatibility

```csharp
public class FileProcessor
{
    // Default constructor - uses real file operations
    public FileProcessor()
        : this(new TextReplacer(), new FileHelperWrapper())
    {
    }

    // DI constructor for testing
    public FileProcessor(ITextReplacer textReplacer, IFileHelper fileHelper)
    {
        _textReplacer = textReplacer;
        _fileHelper = fileHelper;
    }
}
```

## API Reference

### Command-Line API

All options can be specified in long form (`--option`) or short form (`-o`):

```bash
# Long form
FolderTools.exe "search" "replace" "dir" --dry-run --verbose

# Short form
FolderTools.exe "search" "replace" "dir" -d -v

# Without directory (uses current directory with prompt)
FolderTools.exe "search" "replace" -d -v

# Combined
FolderTools.exe "search" "replace" "dir" -dv
```

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success (no errors, or user cancelled at prompt) |
| 1 | Error (invalid arguments, processing errors, or bulk mode failures) |

---

For more information, see the main [README.md](README.md).
