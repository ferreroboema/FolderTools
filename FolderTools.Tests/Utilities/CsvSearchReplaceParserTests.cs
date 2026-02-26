using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using FolderTools.Models;
using FolderTools.Utilities;
using Xunit;

namespace FolderTools.Tests.Utilities
{
    public class CsvSearchReplaceParserTests
    {
        private readonly string _testDirectory;

        public CsvSearchReplaceParserTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        private void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        private string CreateTestCsvFile(string content)
        {
            string filePath = Path.Combine(_testDirectory, "test.csv");
            File.WriteAllText(filePath, content);
            return filePath;
        }

        [Fact]
        public void ParseFile_WithSimplePairs_ShouldParseCorrectly()
        {
            // Arrange
            string content = "old,new\nfoo,bar\nTODO,";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().HaveCount(3);
            pairs[0].SearchPattern.Should().Be("old");
            pairs[0].Replacement.Should().Be("new");
            pairs[0].LineNumber.Should().Be(1);

            pairs[1].SearchPattern.Should().Be("foo");
            pairs[1].Replacement.Should().Be("bar");
            pairs[1].LineNumber.Should().Be(2);

            pairs[2].SearchPattern.Should().Be("TODO");
            pairs[2].Replacement.Should().Be("");
            pairs[2].LineNumber.Should().Be(3);
        }

        [Fact]
        public void ParseFile_WithComments_ShouldSkipCommentLines()
        {
            // Arrange
            string content = "# This is a comment\nold,new\n# Another comment\nfoo,bar";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().HaveCount(2);
            pairs[0].SearchPattern.Should().Be("old");
            pairs[0].LineNumber.Should().Be(2);

            pairs[1].SearchPattern.Should().Be("foo");
            pairs[1].LineNumber.Should().Be(4);
        }

        [Fact]
        public void ParseFile_WithEmptyLines_ShouldSkipEmptyLines()
        {
            // Arrange
            string content = "old,new\n\n\nfoo,bar\n";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().HaveCount(2);
            pairs[0].LineNumber.Should().Be(1);
            pairs[1].LineNumber.Should().Be(4);
        }

        [Fact]
        public void ParseFile_WithQuotedStringsContainingCommas_ShouldParseCorrectly()
        {
            // Arrange
            string content = "\"old, pattern\",\"new, replacement\"\nfoo,bar";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().HaveCount(2);
            pairs[0].SearchPattern.Should().Be("old, pattern");
            pairs[0].Replacement.Should().Be("new, replacement");

            pairs[1].SearchPattern.Should().Be("foo");
            pairs[1].Replacement.Should().Be("bar");
        }

        [Fact]
        public void ParseFile_WithEscapedQuotes_ShouldParseCorrectly()
        {
            // Arrange
            string content = "\"old\"\"pattern\",\"new\"\"replacement\"";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().HaveCount(1);
            pairs[0].SearchPattern.Should().Be("old\"pattern");
            pairs[0].Replacement.Should().Be("new\"replacement");
        }

        [Fact]
        public void ParseFile_WithEmptyReplacement_ShouldParseCorrectly()
        {
            // Arrange
            string content = "TODO,\nold,new";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().HaveCount(2);
            pairs[0].SearchPattern.Should().Be("TODO");
            pairs[0].Replacement.Should().Be("");

            pairs[1].SearchPattern.Should().Be("old");
            pairs[1].Replacement.Should().Be("new");
        }

        [Fact]
        public void ParseFile_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            string filePath = Path.Combine(_testDirectory, "nonexistent.csv");
            var parser = new CsvSearchReplaceParser();

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => parser.ParseFile(filePath));
        }

        [Fact]
        public void ParseFile_WithOnlyCommentsAndEmptyLines_ShouldReturnEmptyList()
        {
            // Arrange
            string content = "# Comment 1\n\n# Comment 2\n   \n";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().BeEmpty();
        }

        [Fact]
        public void ValidatePairs_WithAllValidPairs_ShouldReturnTrue()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("old", "new", 1),
                new SearchReplacePair("foo", "bar", 2)
            };
            var parser = new CsvSearchReplaceParser();

            // Act
            var result = parser.ValidatePairs(pairs);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidatePairs_WithInvalidPairs_ShouldReturnFalse()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("old", "new", 1),
                new SearchReplacePair("", "bar", 2), // Invalid
                new SearchReplacePair(null, "baz", 3) // Invalid
            };
            var parser = new CsvSearchReplaceParser();

            // Act
            var result = parser.ValidatePairs(pairs);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidatePairs_WithEmptyList_ShouldReturnFalse()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>();
            var parser = new CsvSearchReplaceParser();

            // Act
            var result = parser.ValidatePairs(pairs);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidatePairs_WithNullList_ShouldReturnFalse()
        {
            // Arrange
            var parser = new CsvSearchReplaceParser();

            // Act
            var result = parser.ValidatePairs(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetValidationErrors_WithInvalidPairs_ShouldReturnErrorMessages()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("old", "new", 1),
                new SearchReplacePair("", "bar", 5), // Invalid
                new SearchReplacePair(null, "baz", 10) // Invalid
            };
            var parser = new CsvSearchReplaceParser();

            // Act
            var errors = parser.GetValidationErrors(pairs);

            // Assert
            errors.Should().HaveCount(2);
            errors.Should().Contain("Line 5: Search pattern is empty or invalid");
            errors.Should().Contain("Line 10: Search pattern is empty or invalid");
        }

        [Fact]
        public void GetValidationErrors_WithEmptyList_ShouldReturnNoPairsError()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>();
            var parser = new CsvSearchReplaceParser();

            // Act
            var errors = parser.GetValidationErrors(pairs);

            // Assert
            errors.Should().HaveCount(1);
            errors[0].Should().Be("No valid search/replace pairs found in CSV file");
        }

        [Fact]
        public void ParseFile_WithComplexCsv_ShouldHandleAllFeatures()
        {
            // Arrange
            string content = @"# Configuration file for replacements
# This line is a comment

# Simple replacements
old,new
foo,bar

# Replacement with commas in pattern
""pattern, with, commas"",""replacement, also, with, commas""

# Empty replacement (deletes matched text)
TODO,

# More simple replacements
deprecated,supported
legacy,modern
";
            string filePath = CreateTestCsvFile(content);
            var parser = new CsvSearchReplaceParser();

            // Act
            var pairs = parser.ParseFile(filePath);

            // Assert
            pairs.Should().HaveCount(6);
            pairs[0].SearchPattern.Should().Be("old");
            pairs[0].LineNumber.Should().Be(5);

            pairs[1].SearchPattern.Should().Be("foo");
            pairs[1].LineNumber.Should().Be(6);

            pairs[2].SearchPattern.Should().Be("pattern, with, commas");
            pairs[2].Replacement.Should().Be("replacement, also, with, commas");
            pairs[2].LineNumber.Should().Be(9);

            pairs[3].SearchPattern.Should().Be("TODO");
            pairs[3].Replacement.Should().Be("");
            pairs[3].LineNumber.Should().Be(12);

            pairs[4].SearchPattern.Should().Be("deprecated");
            pairs[4].LineNumber.Should().Be(15);

            pairs[5].SearchPattern.Should().Be("legacy");
            pairs[5].LineNumber.Should().Be(16);

            pairs[5].SearchPattern.Should().Be("legacy");
            pairs[5].LineNumber.Should().Be(16);
        }
    }
}
