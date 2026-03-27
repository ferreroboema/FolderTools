# FolderTools - CLI Find and Replace Tool

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/ferreroboema/FolderTools)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8.1-purple)](https://dotnet.microsoft.com/download/dotnet-framework)
[![Tests](https://img.shields.io/badge/tests-202%20passed-success)](https://github.com/ferreroboema/FolderTools)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)](https://microsoft.com/windows)

> A powerful command-line tool for performing find and replace operations across multiple files and directories in Windows.

## Vision

**FolderTools** aims to be the go-to CLI utility for developers and power users who need to perform bulk text replacements across files efficiently. Unlike basic text editor search/replace, FolderTools provides:

- **Batch Processing**: Process hundreds of files in seconds
- **Safety First**: Dry-run mode to preview changes before committing
- **Flexibility**: Support for both literal strings and regular expressions
- **Smart Filtering**: Filter by extension, filename pattern, file size, and more
- **Developer Friendly**: Binary file detection, encoding handling, and verbose logging

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Examples](#examples)
- [Testing](#testing)
- [Project Status](#project-status)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core Capabilities

| Feature | Status | Description |
|---------|--------|-------------|
| **Find & Replace** | ✅ Complete | Literal string replacement with case sensitivity toggle |
| **Regex Support** | ✅ Complete | Full regular expression pattern matching |
| **Bulk/Batch Mode** | ✅ Complete | Process multiple search/replace pairs from CSV file |
| **Recursive Search** | ✅ Complete | Process nested directories with configurable depth |
| **File Filtering** | ✅ Complete | Filter by extension, name pattern, file size |
| **Dry-Run Mode** | ✅ Complete | Preview changes without modifying files |
| **Binary Detection** | ✅ Complete | Automatically skip binary files |
| **Encoding Detection** | ✅ Complete | Auto-detect UTF-8, ASCII, Unicode encodings |
| **Color Output** | ✅ Complete | Color-coded console output for success/errors |
| **Error Handling** | ✅ Complete | Graceful handling of locked files and permissions |
| **Collision Detection** | ✅ Complete | Detects potential issues in bulk mode with configurable behavior |

### Advanced Features

- **Bulk/Batch Mode**: Process multiple search/replace operations from a CSV file in a single run
- **Hidden File Support**: Option to include/exclude hidden and system files
- **Size Filtering**: Filter files by minimum/maximum size
- **Wildcard Patterns**: Filename matching with `*` and `?` wildcards
- **Verbose Logging**: Detailed output for debugging and confirmation
- **Quiet Mode**: Minimal output for scripting
- **Progress Tracking**: Summary statistics with files processed and replacements made
- **Continue on Error**: Process all pairs even if some fail (bulk mode)

## Installation

### Prerequisites

- .NET Framework 4.8.1 Runtime (pre-installed on Windows 10+)
- Windows 10 or later

### Quick Install (Recommended)

The easiest way to install FolderTools is using the PowerShell installer:

```powershell
# One-line installation
irm https://raw.githubusercontent.com/ferreroboema/FolderTools/main/install.ps1 | iex
```

This will:
- Download the latest FolderTools.exe
- Install to `%LOCALAPPDATA%\FolderTools`
- Add to your user PATH automatically
- Create uninstall script for easy removal

After installation, **restart your terminal** and run:

```bash
FolderTools --help
```

Or use the shorthand alias:
```bash
ft --help
```

### Manual Installation

#### Download Release

1. Grab the latest executable from the [Releases](https://github.com/ferreroboema/FolderTools/releases) page
2. Place `FolderTools.exe` in a directory of your choice
3. Add that directory to your PATH:

**Using PowerShell (run as Administrator):**
```powershell
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\Your\Path\Here", "Machine")
```

**Using GUI:**
1. Search for "Environment Variables" in Windows
2. Click "Environment Variables"
3. Edit "Path" under User variables
4. Add the directory containing `FolderTools.exe`

### Build from Source

If you want to build from source:

```bash
git clone https://github.com/ferreroboema/FolderTools.git
cd FolderTools
dotnet build FolderTools.sln -c Release
```

Then run the local installer:
```powershell
.\install.ps1 -Local
```

### Usage Without .exe Extension

Once FolderTools is in your PATH, you can run commands without the `.exe` extension:

```bash
# Full command name
FolderTools "search" "replace" "C:\MyFolder" -e ".txt"

# Shorthand alias
ft "search" "replace" "C:\MyFolder" -e ".txt"
```

### Uninstall

To remove FolderTools from your system:

```powershell
# If you used the installer, run:
& "$env:LOCALAPPDATA\FolderTools\uninstall.ps1"

# Or download and run the uninstall script:
irm https://raw.githubusercontent.com/ferreroboema/FolderTools/main/uninstall.ps1 | iex
```

## Usage

> **Note:** Once FolderTools is installed and added to PATH, you can use commands without the `.exe` extension. The examples below show the cleaner syntax.

### Basic Syntax

#### Standard Mode (Single Operation)
```bash
FolderTools <search-pattern> <replace-pattern> [<directory>] [options]
```

#### Bulk Mode (Multiple Operations)
```bash
FolderTools --bulk-file <csv-file> [<directory>] [options]
# or
FolderTools -b <csv-file> [<directory>] [options]
```

### Required Arguments

#### Standard Mode
| Argument | Description |
|----------|-------------|
| `<search-pattern>` | Text or regex pattern to search for |
| `<replace-pattern>` | Text to replace matches with (use `""` for empty) |
| `[<directory>]` | Starting directory (optional, defaults to current directory with confirmation) |

#### Bulk Mode
| Argument | Description |
|----------|-------------|
| `--bulk-file <csv-file>` | CSV file containing search/replace pairs |
| `[<directory>]` | Starting directory (optional, defaults to current directory with confirmation) |

### Options

| Option | Short | Description |
|--------|-------|-------------|
| `--bulk-file <csv>` | `-b` | Bulk mode: CSV file with search/replace pairs |
| `--extensions <exts>` | `-e` | File extensions to process (e.g., `.txt,.cs,.json`) |
| `--filename <pattern>` | `-f` | Filename wildcard pattern (e.g., `*config*`) |
| `--min-size <bytes>` | | Minimum file size |
| `--max-size <bytes>` | | Maximum file size |
| `--case-sensitive` | `-c` | Case sensitive matching (default: false) |
| `--regex` | `-r` | Use regex pattern matching |
| `--dry-run` | `-d` | Preview changes without modifying files |
| `--encoding <type>` | | Text encoding: `auto`, `utf8`, `ascii`, `unicode` |
| `--include-hidden` | | Include hidden/system files |
| `--max-depth <number>` | | Maximum recursion depth (0 = current dir only) |
| `--collision-behavior <mode>` | | How to handle bulk collisions: `prompt`, `warn`, `fail`, `ignore` |
| `--verbose` | `-v` | Verbose output with per-file details |
| `--quiet` | `-q` | Quiet mode (minimal output) |
| `--version` | `-V` | Show version information |
| `--help` | `-h` | Show help message |

## Examples

### Basic Literal Replacement

Replace "old" with "new" in all text files:

```bash
FolderTools "old" "new" "C:\MyProject" -e ".txt"
```

### Preview Changes (Dry-Run)

See what would change without modifying files:

```bash
FolderTools "TODO" "FIXME" "C:\MyProject" --dry-run -v
```

### Regex Pattern Matching

Replace all digit sequences with "NUM":

```bash
FolderTools "\d+" "NUM" "C:\MyProject" -r -e ".txt"
```

### Case Sensitive Search

Replace "Foo" but not "foo":

```bash
FolderTools "Foo" "Bar" "C:\MyProject" -c
```

### Filter by Filename Pattern

Process only files matching "*config*":

```bash
FolderTools "localhost" "production.server.com" "C:\MyProject" -f "*config*"
```

### Limit Directory Depth

Only search current directory (no recursion):

```bash
FolderTools "temp" "tmp" "C:\MyProject" --max-depth 0
```

### Multiple File Extensions

Process both `.cs` and `.vb` files:

```bash
FolderTools "MyNamespace" "NewNamespace" "C:\MyProject" -e ".cs,.vb"
```

### Verbose Output for Debugging

See detailed information about each file:

```bash
FolderTools "oldValue" "newValue" "C:\MyProject" -v --dry-run
```

## Bulk Mode (Batch Operations)

Bulk mode allows you to perform multiple search/replace operations in a single run by specifying pairs in a CSV file.

### CSV File Format

Create a CSV file with search/replace pairs (one per line):

```csv
# Comment lines start with #
old,new
foo,bar
"pattern, with, commas",replacement
TODO,  (empty replacement deletes matched text)
deprecated,supported
```

**CSV Format Rules:**
- Each line: `search,replacement`
- Use quotes for values containing commas: `"pattern, with, commas",replacement`
- Lines starting with `#` are comments
- Empty lines are ignored
- Empty replacement deletes the matched text

### Bulk Mode Examples

#### Basic Bulk Replacement

Create `replacements.csv`:
```csv
hello,hi
world,earth
foo,bar
```

Run bulk mode:
```bash
FolderTools --bulk-file replacements.csv "C:\MyFolder" -e ".txt"
```

#### Bulk Mode with Dry-Run

Preview all changes before applying:
```bash
FolderTools -b replacements.csv "C:\MyFolder" --dry-run -v
```

#### Complex CSV with Commas and Comments

Create `complex.csv`:
```csv
# Configuration file replacements
localhost,production.example.com
"127.0.0.1:8080","api.service.com:443"
DEBUG,false
TODO,  (empty replacement deletes TODO comments)
```

Run with options:
```bash
FolderTools -b complex.csv "C:\MyProject" -e ".cs,.js,.json" -c -v
```

#### Bulk Mode with All Filters

```bash
FolderTools --bulk-file pairs.csv "C:\Project" \
  -e ".cs,.vb,.js" \
  -f "*config*" \
  --max-depth 3 \
  --include-hidden \
  --dry-run
```

### Bulk Mode Output

```
=== FolderTools - Bulk Find and Replace ===
CSV file: replacements.csv
Directory: C:\MyFolder
Pairs to process: 3
Mode: Live (files will be modified)

--- Pair 1: "hello" -> "hi" ---
  Files processed: 5
  Replacements: 12

--- Pair 2: "world" -> "earth" ---
  Files processed: 5
  Replacements: 8

--- Pair 3: "foo" -> "bar" ---
  Files processed: 5
  Replacements: 3

=== Bulk operation completed ===
Pairs processed: 3 total, 3 successful, 0 failed
Total files processed: 15
Total replacements: 23
```

### Collision Detection in Bulk Mode

Bulk mode includes automatic collision detection to prevent unintended behavior when replacement values from one pair become search patterns in subsequent pairs.

#### What Are Collisions?

A collision occurs when a replacement value from one pair matches a search pattern in a later pair. Because pairs are processed sequentially, this can cause unexpected results.

**Example of a collision:**
```csv
V123,V146
V146,V178
```

Expected by user: V123 → V146 (and stays V146), V146 → V178
Actual behavior: Both V123 and V146 become V178 because:
1. First pair: V123 → V146
2. Second pair: V146 → V178 (affects both original V146 AND the newly replaced V146)

#### Collision Behavior Modes

Use the `--collision-behavior` flag to control how collisions are handled:

| Mode | Description |
|------|-------------|
| `prompt` | **(default)** Show warnings and ask user before continuing |
| `warn` | Show warnings but continue automatically |
| `fail` | Exit immediately with error code if collisions detected |
| `ignore` | Silent mode - no warnings, continue processing |

#### Collision Detection Examples

**Default prompt behavior:**
```bash
FolderTools --bulk-file pairs.csv "C:\MyFolder"
```

Output when collisions detected:
```
WARNING: Collisions detected: 1 collision(s) detected involving 2 pair(s).

=== POTENTIAL COLLISIONS DETECTED ===

Chain collision detected (2 pairs):
  "V123" -> "V146"
  Line 1: "V123" -> "V146"
    This value will be replaced again by line 2
  Line 2: "V146" -> "V178"

Expected behavior:
  Pairs are processed sequentially in the order they appear in the CSV file.
  If a replacement value becomes a search pattern in a later pair,
  it will be replaced again by that later pair.

Do you want to continue? (Y/n):
```

**Warn mode (non-interactive):**
```bash
FolderTools --bulk-file pairs.csv "C:\MyFolder" --collision-behavior warn
```

**Fail mode (for CI/CD):**
```bash
FolderTools --bulk-file pairs.csv "C:\MyFolder" --collision-behavior fail
```

**Ignore mode (silent):**
```bash
FolderTools --bulk-file pairs.csv "C:\MyFolder" --collision-behavior ignore
```

## Testing

### Test Architecture

FolderTools uses a dependency injection pattern to enable comprehensive unit testing of file I/O operations:

- **IFileHelper Interface**: Abstraction over static `FileHelper` methods
- **FileHelperWrapper**: Concrete implementation wrapping static methods
- **Moq Framework**: Mocking of IFileHelper for isolated unit tests
- **System.IO.Abstractions**: File system abstraction for integration tests

### Test Coverage by Component

| Component | Tests | Status | Notes |
|-----------|-------|--------|-------|
| SearchOptions | 5 | ✅ Pass | Model property initialization |
| ReplacementResult | 8 | ✅ Pass | Result aggregation |
| FileFilter | 10 | ✅ Pass | Pattern matching with filename-only support |
| SearchReplacePair | 10 | ✅ Pass | Bulk mode pair model |
| BulkReplacementResult | 10 | ✅ Pass | Bulk mode result aggregation |
| TextReplacer | 11 | ✅ Pass | Text replacement with edge cases |
| FileProcessor | 5 | ✅ Pass | File system integration working |
| BulkFileProcessor | 10 | ✅ Pass | Bulk processing with continue-on-error |
| CommandLineParser | 32 | ✅ Pass | Standard + bulk mode argument parsing |
| CsvSearchReplaceParser | 15 | ✅ Pass | CSV parsing with quotes/comments |
| FileHelper | 12 | ✅ Pass | File operations and utilities |
| EncodingHelper | 10 | ✅ Pass | Encoding detection |
| ResultFormatter | 15 | ✅ Pass | Standard + bulk mode output |
| BulkCollisionValidator | 14 | ✅ Pass | Collision detection algorithm |
| **Total** | **202** | **✅ 100%** | **All tests passing** |

### Writing New Tests

When adding new features, follow this testing pattern:

```csharp
using FluentAssertions;
using FolderTools.Services;
using Moq;
using Xunit;

public class MyFeatureTests
{
    private readonly Mock<IFileHelper> _fileHelperMock;

    public MyFeatureTests()
    {
        _fileHelperMock = new Mock<IFileHelper>();
    }

    [Fact]
    public void MyFeature_ShouldWorkAsExpected()
    {
        // Arrange
        _fileHelperMock.Setup(f => f.TryReadFile(...)).Returns(true);

        // Act
        var result = sut.DoSomething();

        // Assert
        result.Should().Be(expected);
    }
}
```

### Test Data

Test fixtures are located in `FolderTools.Tests/TestData/`:
- `Files/utf8.txt` - UTF-8 encoded text file
- `Files/ascii.txt` - ASCII encoded text file
- `Files/binary.bin` - Binary file with null bytes
- `Files/empty.txt` - Empty file for edge cases
- `Directories/Nested/` - Nested directory structure

## Project Status

### Version: 1.0.0

**Current Release**: Stable | **Last Updated**: February 2025

### Implementation Progress

| Component | Status | Progress |
|-----------|--------|----------|
| Core Engine | ✅ Complete | 100% |
| File Filtering | ✅ Complete | 100% |
| Regex Support | ✅ Complete | 100% |
| Bulk/Batch Mode | ✅ Complete | 100% |
| Encoding Detection | ✅ Complete | 100% |
| Binary Detection | ✅ Complete | 100% |
| CLI Parser | ✅ Complete | 100% |
| Error Handling | ✅ Complete | 100% |
| Unit Tests | ✅ Complete | 100% (202/202 passing) |
| Documentation | ✅ Complete | 100% |
| CI/CD Pipeline | 🔄 Planned | 0% |

### Known Limitations

1. **Windows Only**: Currently supports Windows platforms only
2. **.NET Framework 4.8.1**: Requires .NET Framework 4.8.1 runtime
3. **x86 Architecture**: Built for x86 (32-bit) architecture
4. **Line Ending**: Does not preserve original line ending style (CRLF vs LF)
5. **File Size**: Large files (>100MB) may cause memory issues

### Upcoming Features (Roadmap)

- [ ] Backup creation before modifications
- [ ] Undo functionality
- [ ] Unicode normalization options
- [ ] Performance improvements for large file sets
- [ ] Multi-threading for parallel processing
- [ ] Configuration file support (JSON/YAML) as alternative to CSV

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/ferreroboema/FolderTools.git
cd FolderTools

# Build the solution
dotnet build FolderTools.sln

# Run unit tests
dotnet test FolderTools.Tests/FolderTools.Tests.csproj

# Run tests with coverage (requires coverlet)
dotnet test FolderTools.Tests/FolderTools.Tests.csproj --collect:"XPlat Code Coverage"

# Run the tool
dotnet run --project FolderTools/FolderTools.csproj -- "search" "replace" "."
```

### Running Tests

The project includes comprehensive unit tests using **xUnit**, **Moq**, and **FluentAssertions**:

- **Total Tests**: 202
- **Passing**: 202 (100%)
- **Test Framework**: xUnit 2.7+
- **Mocking**: Moq 4.20+
- **Assertions**: FluentAssertions 6.12+
- **File System Abstraction**: System.IO.Abstractions 19.2+

To run all tests:
```bash
dotnet test FolderTools.Tests/FolderTools.Tests.csproj
```

To run a specific test class:
```bash
dotnet test --filter "FullyQualifiedName~TextReplacerTests"
```

To run with verbose output:
```bash
dotnet test FolderTools.Tests/FolderTools.Tests.csproj --verbosity normal
```

### Code Style

- Follow C# coding conventions
- Use XML documentation comments for public APIs
- Keep methods focused and under 50 lines
- Write unit tests for new features
- Use dependency injection for testability (see `IFileHelper` pattern)

### Project Structure

```
FolderTools/
├── FolderTools/                    # Main application
│   ├── Models/                     # Data models
│   │   ├── SearchOptions.cs
│   │   ├── ReplacementResult.cs
│   │   ├── FileFilter.cs
│   │   ├── SearchReplacePair.cs    # Bulk mode pair model
│   │   ├── BulkReplacementResult.cs # Bulk mode result model
│   │   └── CollisionInfo.cs        # Collision detection model
│   ├── Services/                   # Business logic
│   │   ├── IFileProcessor.cs       # File processor interface
│   │   ├── IFileHelper.cs          # File helper interface (for DI)
│   │   ├── FileProcessor.cs        # Main processing logic
│   │   ├── BulkFileProcessor.cs    # Bulk mode processing logic
│   │   ├── ITextReplacer.cs        # Text replacer interface
│   │   └── TextReplacer.cs         # Text replacement logic
│   ├── Utilities/                  # Helper classes
│   │   ├── FileHelper.cs           # Static file utilities
│   │   ├── FileHelperWrapper.cs    # Wrapper for IFileHelper
│   │   ├── EncodingHelper.cs       # Encoding detection
│   │   ├── CommandLineParser.cs    # CLI argument parser
│   │   ├── CsvSearchReplaceParser.cs # CSV file parser for bulk mode
│   │   └── BulkCollisionValidator.cs # Collision detection for bulk mode
│   ├── Outputs/                    # Output formatting
│   │   └── ResultFormatter.cs      # Console output formatter
│   └── Program.cs                  # Entry point
│
└── FolderTools.Tests/              # Unit test project
    ├── Models/                     # Model tests
    │   ├── SearchOptionsTests.cs
    │   ├── ReplacementResultTests.cs
    │   ├── FileFilterTests.cs
    │   ├── SearchReplacePairTests.cs
    │   └── BulkReplacementResultTests.cs
    ├── Services/                   # Service tests (with mocking)
    │   ├── FileProcessorTests.cs
    │   ├── BulkFileProcessorTests.cs
    │   └── TextReplacerTests.cs
    ├── Utilities/                  # Utility tests
    │   ├── CommandLineParserTests.cs
    │   ├── CsvSearchReplaceParserTests.cs
    │   ├── FileHelperTests.cs
    │   ├── EncodingHelperTests.cs
    │   └── BulkCollisionValidatorTests.cs
    ├── Outputs/                    # Output tests
    │   └── ResultFormatterTests.cs
    └── TestData/                   # Test fixtures and files
```

## Documentation

- [API Documentation](DOCUMENTATION.md) - Detailed technical documentation
- [Changelog](CHANGELOG.md) - Version history and changes

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with .NET Framework 4.8.1
- Testing with xUnit, Moq, and FluentAssertions
- Inspired by classic Unix utilities (`sed`, `find`, `grep`)
- Created for developers who need powerful text manipulation tools

## Support

- **Issues**: [GitHub Issues](https://github.com/ferreroboema/FolderTools/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ferreroboema/FolderTools/discussions)
- **Email**: support@foldertools.dev

---

<div align="center">

**Made with ❤️ by the FolderTools team**

[⬆ Back to Top](#foldertools---cli-find-and-replace-tool)

</div>
