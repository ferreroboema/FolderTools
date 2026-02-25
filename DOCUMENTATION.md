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
| **Builder** | Command-line argument construction | `CommandLineParser` |
| **Formatter** | Output formatting strategies | `ResultFormatter` |
| **Filter** | File filtering criteria | `FileFilter` |

## Project Structure

```
FolderTools/
├── FolderTools.csproj              # .NET Framework 4.8 project
├── FolderTools.netcore.csproj      # .NET 8.0 project (SDK-style)
│
├── Models/                         # Data models
│   ├── SearchOptions.cs            # Search configuration
│   ├── ReplacementResult.cs        # Result data structures
│   └── FileFilter.cs               # File filtering logic
│
├── Services/                       # Business logic
│   ├── IFileProcessor.cs           # File processor interface
│   ├── FileProcessor.cs            # Directory traversal & processing
│   ├── ITextReplacer.cs            # Text replacer interface
│   └── TextReplacer.cs             # Replace implementation
│
├── Utilities/                      # Helper classes
│   ├── CommandLineParser.cs        # Argument parsing
│   ├── FileHelper.cs               # File system operations
│   └── EncodingHelper.cs           # Encoding detection
│
├── Outputs/                        # Output formatting
│   └── ResultFormatter.cs          # Console output
│
└── Program.cs                      # Entry point
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

For case-insensitive literal replacement, a regex is constructed:

```csharp
// Escape special regex characters in pattern
string escapedPattern = Regex.Escape(pattern);

// Use regex with IgnoreCase option
string result = Regex.Replace(content, escapedPattern,
                              replacement,
                              RegexOptions.IgnoreCase);
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

### Unit Tests (Planned)

```csharp
// Example test structure
[TestClass]
public class TextReplacerTests
{
    [TestMethod]
    public void ReplaceInFile_LiteralPattern_Success()
    {
        // Arrange
        var replacer = new TextReplacer();
        var options = new SearchOptions
        {
            Pattern = "old",
            Replacement = "new",
            IsRegex = false
        };

        // Act
        int count = replacer.ReplaceInFile(testFilePath, options);

        // Assert
        Assert.AreEqual(expectedCount, count);
    }
}
```

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

## API Reference

### Command-Line API

All options can be specified in long form (`--option`) or short form (`-o`):

```bash
# Long form
FolderTools.exe "search" "replace" "dir" --dry-run --verbose

# Short form
FolderTools.exe "search" "replace" "dir" -d -v

# Combined
FolderTools.exe "search" "replace" "dir" -dv
```

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success (no errors) |
| 1 | One or more files had errors |
| 2 | Invalid command-line arguments |

---

For more information, see the main [README.md](README.md).
