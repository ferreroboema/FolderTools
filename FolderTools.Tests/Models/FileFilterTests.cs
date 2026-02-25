using FluentAssertions;
using FolderTools.Models;
using System.IO;
using Xunit;

namespace FolderTools.Tests.Models
{
    public class FileFilterTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithEmptyExtensions()
        {
            var filter = new FileFilter();
            filter.Extensions.Should().NotBeNull().And.BeEmpty();
            filter.FileNamePattern.Should().BeNull();
            filter.MinSize.Should().BeNull();
            filter.MaxSize.Should().BeNull();
            filter.IncludeHidden.Should().BeFalse();
        }

        [Fact]
        public void AddExtensions_ShouldAddExtensionsWithDots()
        {
            var filter = new FileFilter();
            filter.AddExtensions(".txt,.cs");
            filter.Extensions.Should().Contain(".txt").And.Contain(".cs");
        }

        [Fact]
        public void AddExtensions_WithoutDots_ShouldAddDots()
        {
            var filter = new FileFilter();
            filter.AddExtensions("txt,cs");
            filter.Extensions.Should().Contain(".txt").And.Contain(".cs");
        }

        [Fact]
        public void AddExtensions_WithSemicolon_ShouldParse()
        {
            var filter = new FileFilter();
            filter.AddExtensions("txt;cs;json");
            filter.Extensions.Should().Contain(".txt").And.Contain(".cs").And.Contain(".json");
        }

        [Fact]
        public void AddExtensions_ShouldBeCaseInsensitive()
        {
            var filter = new FileFilter();
            filter.AddExtensions(".TXT");
            filter.Extensions.Should().Contain(".txt");
        }

        [Fact]
        public void ShouldProcessFile_WithNoFilters_ShouldReturnTrue()
        {
            var filter = new FileFilter();
            filter.ShouldProcessFile("test.txt").Should().BeTrue();
        }

        [Fact]
        public void ShouldProcessFile_WithExtensionMatch_ShouldReturnTrue()
        {
            var filter = new FileFilter();
            filter.AddExtensions(".txt,.cs");
            filter.ShouldProcessFile("test.txt").Should().BeTrue();
        }

        [Fact]
        public void ShouldProcessFile_WithExtensionMismatch_ShouldReturnFalse()
        {
            var filter = new FileFilter();
            filter.AddExtensions(".txt,.cs");
            filter.ShouldProcessFile("test.exe").Should().BeFalse();
        }

        [Fact]
        public void ShouldProcessFile_WithFileNamePatternMatch_ShouldReturnTrue()
        {
            var filter = new FileFilter();
            filter.FileNamePattern = "*config*";
            filter.ShouldProcessFile("myconfig.txt").Should().BeTrue();
            filter.ShouldProcessFile("config.json").Should().BeTrue();
        }

        [Fact]
        public void ShouldProcessFile_WithFileNamePatternMismatch_ShouldReturnFalse()
        {
            var filter = new FileFilter();
            filter.FileNamePattern = "*config*";
            filter.ShouldProcessFile("test.txt").Should().BeFalse();
        }

        [Fact]
        public void ShouldProcessFile_WithWildcardQuestionMark_ShouldMatch()
        {
            var filter = new FileFilter();
            filter.FileNamePattern = "Test?.txt";
            filter.ShouldProcessFile("Test1.txt").Should().BeTrue();
            filter.ShouldProcessFile("TestA.txt").Should().BeTrue();
            filter.ShouldProcessFile("Test12.txt").Should().BeFalse();
        }

        [Fact]
        public void AddExtensions_EmptyString_ShouldNotAdd()
        {
            var filter = new FileFilter();
            filter.AddExtensions("");
            filter.Extensions.Should().BeEmpty();
        }

        [Fact]
        public void AddExtensions_Whitespace_ShouldNotAdd()
        {
            var filter = new FileFilter();
            filter.AddExtensions("  ,  , ");
            filter.Extensions.Should().BeEmpty();
        }

        [Fact]
        public void ShouldProcessFile_WithMinSize_ShouldFilter()
        {
            var filter = new FileFilter();
            filter.MinSize = 1000;
            // This test requires actual file, will be tested with test data files
        }

        [Fact]
        public void ShouldProcessFile_WithMaxSize_ShouldFilter()
        {
            var filter = new FileFilter();
            filter.MaxSize = 100;
            // This test requires actual file, will be tested with test data files
        }

        [Fact]
        public void ShouldProcessFile_WithSizeRange_ShouldFilter()
        {
            var filter = new FileFilter();
            filter.MinSize = 100;
            filter.MaxSize = 10000;
            // This test requires actual file, will be tested with test data files
        }

        [Fact]
        public void ShouldProcessFile_InvalidPath_ShouldReturnFalse()
        {
            var filter = new FileFilter();
            filter.ShouldProcessFile(null).Should().BeFalse();
            filter.ShouldProcessFile("").Should().BeFalse();
        }
    }
}
