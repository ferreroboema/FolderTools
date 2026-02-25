# Changelog

All notable changes to FolderTools will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- Linux/macOS support
- Configuration file support
- Interactive confirmation mode
- Backup creation before modifications
- Undo functionality
- Multi-threaded processing

## [1.0.0] - 2025-02-25

### Added

#### Core Features
- **Find and Replace Engine**: Complete text replacement functionality
  - Literal string replacement
  - Regular expression pattern matching
  - Case-sensitive and case-insensitive search modes
- **Recursive Directory Processing**: Process nested directories with configurable depth
- **File Filtering System**: Multiple filtering options
  - File extension filtering (multiple extensions supported)
  - Filename wildcard pattern matching (`*` and `?`)
  - Minimum and maximum file size filtering
  - Hidden and system file inclusion/exclusion

#### Safety Features
- **Dry-Run Mode**: Preview changes without modifying files (`--dry-run`)
- **Binary File Detection**: Automatically detect and skip binary files
- **File Lock Detection**: Detect and skip files locked by other processes
- **Encoding Detection**: Auto-detect file encoding (UTF-8, ASCII, Unicode)

#### Command-Line Interface
- **Comprehensive Argument Parser**: Full-featured CLI with help system
- **Verbose Mode**: Detailed per-file processing information (`--verbose`)
- **Quiet Mode**: Minimal output for scripting (`--quiet`)
- **Color-Coded Output**: Visual feedback for success (green), errors (red), warnings (yellow)

#### Output & Reporting
- **Summary Statistics**: Files processed, total replacements, errors, skipped files
- **Detailed Results**: Per-file replacement counts and error messages
- **Relative Path Display**: Shows paths relative to root directory
- **Human-Readable File Sizes**: Converts bytes to KB, MB, GB as needed

#### Documentation
- **README.md**: Comprehensive project documentation with GitHub-style formatting
- **DOCUMENTATION.md**: Technical documentation for developers
- **CHANGELOG.md**: Version history and changes (this file)
- **Built-in Help**: `--help` command displays usage information

#### Build & Distribution
- **.NET 8.0 Project**: SDK-style project for `dotnet CLI` compatibility
- **.NET Framework 4.8 Project**: Original project for Visual Studio compatibility
- **Solution File**: Complete Visual Studio solution structure
- **Self-Contained Executable**: No external dependencies required

### Technical Details

#### Project Structure
```
FolderTools/
├── Models/          # Data structures (SearchOptions, ReplacementResult, FileFilter)
├── Services/        # Business logic (FileProcessor, TextReplacer)
├── Utilities/       # Helper classes (FileHelper, EncodingHelper, CommandLineParser)
├── Outputs/         # Output formatting (ResultFormatter)
└── Program.cs       # Entry point
```

#### Dependencies
- .NET 8.0 (or .NET Framework 4.8)
- No external NuGet packages
- Pure .NET Standard Library

#### Supported Platforms
- Windows 10/11 (x86, x64)
- Windows Server 2019+

### Known Limitations

1. **Windows Only**: Currently supports Windows platforms only
2. **.NET Requirement**: Requires .NET 8.0 runtime or .NET Framework 4.8
3. **Line Endings**: Does not preserve original line ending style (CRLF vs LF)
4. **Large Files**: Files >100MB may cause memory issues
5. **PowerShell Arguments**: Some argument combinations need special handling in PowerShell

### Security Considerations

- No network operations
- No elevation required (unless accessing protected directories)
- Read/write permissions respected
- No data collection or telemetry

### Performance

- **Small files** (<1KB): ~100 files/second
- **Medium files** (1-100KB): ~50 files/second
- **Large files** (>100KB): ~5 files/second

*Benchmarks on typical development machine, SSD storage.*

## [0.1.0] - 2025-02-24

### Added
- Initial project concept
- Basic file processing research
- Architecture design

---

## Versioning Scheme

- **Major** (X.0.0): Breaking changes, major features
- **Minor** (0.X.0): New features, backward compatible
- **Patch** (0.0.X): Bug fixes, minor improvements

## Release Calendar

| Version | Target Date | Status |
|---------|-------------|--------|
| 1.0.0 | February 2025 | ✅ Released |
| 1.1.0 | Q2 2025 | � Planned |
| 1.2.0 | Q3 2025 | � Planned |
| 2.0.0 | Q4 2025 | � Planned |

---

## Migration Guide

### From 0.x to 1.0.0

No breaking changes. First stable release.

### Future Migration Notes

When upgrading versions, check this section for:
- Deprecated options
- Changed default behaviors
- New required arguments
