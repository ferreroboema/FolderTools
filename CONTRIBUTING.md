# Contributing to FolderTools

First off, thank you for considering contributing to FolderTools! It's people like you that make FolderTools such a great tool.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Commit Messages](#commit-messages)
- [Pull Request Process](#pull-request-process)
- [Testing Guidelines](#testing-guidelines)

## Code of Conduct

### Our Pledge

We as members, contributors, and leaders pledge to make participation in our
community a harassment-free experience for everyone.

### Our Standards

Examples of behavior that contributes to a positive environment:
- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates.

When filing a bug report, please include:
- **Title**: Clear, descriptive summary
- **Description**: Detailed explanation of the issue
- **Reproduction Steps**: Steps to reproduce the behavior
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Environment**: OS version, .NET version
- **Logs**: Relevant error messages or console output

### Suggesting Enhancements

Enhancement suggestions are welcome! Please provide:
- **Use Case**: Describe the use case for the feature
- **Proposed Solution**: How you envision the feature working
- **Alternatives**: Any alternative solutions considered
- **Impact**: How this would benefit users

## Development Setup

### Prerequisites

- .NET 8.0 SDK
- Windows 10/11 or Windows Server 2019+
- Git
- A code editor (Visual Studio, VS Code, or similar)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork:
   ```bash
   git clone https://github.com/your-username/FolderTools.git
   cd FolderTools
   ```

3. Add upstream remote:
   ```bash
   git remote add upstream https://github.com/original-owner/FolderTools.git
   ```

### Build the Project

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the tool
dotnet run --project FolderTools/FolderTools.netcore.csproj -- --help
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Coding Standards

### C# Conventions

Follow the official [C# coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

- **PascalCase** for types, methods, properties, public fields
- **camelCase** for method parameters, local variables
- **_camelCase** for private fields
- **IPascalCase** for interfaces

### Example

```csharp
public class FileProcessor : IFileProcessor
{
    private readonly ITextReplacer _textReplacer;
    private const int MaxBufferSize = 8192;

    public ReplacementResult ProcessDirectory(string rootDirectory,
                                               SearchOptions options,
                                               FileFilter filter)
    {
        // Implementation
    }
}
```

### XML Documentation

All public members should have XML documentation comments:

```csharp
/// <summary>
/// Processes all files in a directory matching the given filter criteria
/// </summary>
/// <param name="rootDirectory">Root directory to start processing from</param>
/// <param name="options">Search and replace options</param>
/// <param name="filter">File filtering criteria</param>
/// <returns>Aggregated results from all processed files</returns>
public ReplacementResult ProcessDirectory(string rootDirectory,
                                          SearchOptions options,
                                          FileFilter filter)
{
    // Implementation
}
```

### Code Organization

- Keep methods focused (single responsibility)
- Methods should generally be under 50 lines
- Classes should be under 500 lines
- Use regions sparingly
- Keep nesting level ≤ 4

### Naming Guidelines

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `FileProcessor` |
| Interfaces | I + PascalCase | `IFileProcessor` |
| Methods | PascalCase | `ProcessDirectory` |
| Properties | PascalCase | `ProcessedFiles` |
| Public fields | PascalCase | `MaxSize` |
| Private fields | _camelCase | `_textReplacer` |
| Parameters | camelCase | `rootDirectory` |
| Local variables | camelCase | `filePath` |
| Constants | PascalCase | `MaxBufferSize` |

## Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

### Format

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Types

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation changes |
| `style` | Code style changes (formatting) |
| `refactor` | Code refactoring |
| `test` | Adding or updating tests |
| `chore` | Maintenance tasks |

### Examples

```bash
feat(text-replacer): add support for multiline regex matching

fix(encoding): properly detect UTF-8 without BOM

docs(readme): update installation instructions for Linux

refactor(file-filter): simplify extension matching logic

test(text-replacer): add tests for case-sensitive replacement
```

## Pull Request Process

### Before Submitting

1. **Update Documentation**: Update README.md and DOCUMENTATION.md if needed
2. **Add Tests**: Write tests for new features or bug fixes
3. **Update Changelog**: Add entry to CHANGELOG.md under "Unreleased"
4. **Code Review**: Self-review your changes

### PR Title

Use conventional commit format:

```
feat(encoding): add UTF-16 LE support
fix(cli): properly handle quoted arguments
```

### PR Description

Include:
- **Summary**: Brief description of changes
- **Motivation**: Why these changes were made
- **Changes**: List of specific changes
- **Testing**: How you tested the changes
- **Screenshots**: If applicable (UI changes)

### Review Process

1. Automated checks must pass
2. At least one maintainer approval required
3. Resolve all review comments
4. Squash commits if requested
5. Maintainer will merge when ready

## Testing Guidelines

### Unit Tests

- Write unit tests for new features
- Aim for >80% code coverage
- Test edge cases and error conditions
- Use descriptive test names

### Example Test Structure

```csharp
[TestClass]
public class FileFilterTests
{
    [TestMethod]
    public void ShouldProcessFile_WithMatchingExtension_ReturnsTrue()
    {
        // Arrange
        var filter = new FileFilter();
        filter.AddExtensions(".txt,.cs");
        string testFile = "test.txt";

        // Act
        bool result = filter.ShouldProcessFile(testFile);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldProcessFile_WithNonMatchingExtension_ReturnsFalse()
    {
        // Arrange
        var filter = new FileFilter();
        filter.AddExtensions(".txt");
        string testFile = "test.cs";

        // Act
        bool result = filter.ShouldProcessFile(testFile);

        // Assert
        Assert.IsFalse(result);
    }
}
```

### Integration Tests

Test end-to-end scenarios:
- Command-line argument parsing
- File system operations
- Text replacement with various options
- Error handling and recovery

### Manual Testing

Before submitting a PR:
1. Test on Windows 10/11
2. Test with various file types (text, binary, mixed)
3. Test with nested directories
4. Test edge cases (empty files, locked files, etc.)

## Questions?

Feel free to open an issue with the `question` label, or start a discussion in GitHub Discussions.

---

Thank you for your contributions! 🎉
