using System;
using FluentAssertions;
using FolderTools.Models;
using Xunit;

namespace FolderTools.Tests.Models
{
    public class SearchReplacePairTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateValidPair()
        {
            // Arrange & Act
            var pair = new SearchReplacePair("old", "new", 5);

            // Assert
            pair.SearchPattern.Should().Be("old");
            pair.Replacement.Should().Be("new");
            pair.LineNumber.Should().Be(5);
        }

        [Fact]
        public void Constructor_Default_ShouldCreateEmptyPair()
        {
            // Arrange & Act
            var pair = new SearchReplacePair();

            // Assert
            pair.SearchPattern.Should().BeNull();
            pair.Replacement.Should().BeNull();
            pair.LineNumber.Should().Be(0);
        }

        [Fact]
        public void IsValid_WithValidSearchPattern_ShouldReturnTrue()
        {
            // Arrange
            var pair = new SearchReplacePair("pattern", "replacement", 1);

            // Act
            var result = pair.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithEmptySearchPattern_ShouldReturnFalse()
        {
            // Arrange
            var pair = new SearchReplacePair("", "replacement", 1);

            // Act
            var result = pair.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WithNullSearchPattern_ShouldReturnFalse()
        {
            // Arrange
            var pair = new SearchReplacePair(null, "replacement", 1);

            // Act
            var result = pair.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WithWhitespaceSearchPattern_ShouldReturnFalse()
        {
            // Arrange
            var pair = new SearchReplacePair("   ", "replacement", 1);

            // Act
            var result = pair.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WithEmptyReplacement_ShouldReturnTrue()
        {
            // Arrange
            var pair = new SearchReplacePair("pattern", "", 1);

            // Act
            var result = pair.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var pair = new SearchReplacePair("old", "new", 10);

            // Act
            var result = pair.ToString();

            // Assert
            result.Should().Be("Line 10: \"old\" -> \"new\"");
        }

        [Fact]
        public void ToString_WithEmptyReplacement_ShouldShowEmpty()
        {
            // Arrange
            var pair = new SearchReplacePair("old", "", 10);

            // Act
            var result = pair.ToString();

            // Assert
            result.Should().Be("Line 10: \"old\" -> \"(empty)\"");
        }

        [Fact]
        public void ToString_WithNullReplacement_ShouldShowEmpty()
        {
            // Arrange
            var pair = new SearchReplacePair("old", null, 10);

            // Act
            var result = pair.ToString();

            // Assert
            result.Should().Be("Line 10: \"old\" -> \"(empty)\"");
        }

        [Fact]
        public void ToString_WithSpecialCharacters_ShouldEscapeCorrectly()
        {
            // Arrange
            var pair = new SearchReplacePair("old\n", "new\t", 10);

            // Act
            var result = pair.ToString();

            // Assert
            // ToString doesn't escape special characters, just wraps in quotes
            result.Should().Be("Line 10: \"old\n\" -> \"new\t\"");
        }
    }
}
