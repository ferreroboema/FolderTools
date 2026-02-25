using FluentAssertions;
using FolderTools.Models;
using Xunit;

namespace FolderTools.Tests.Models
{
    public class SearchOptionsTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaults()
        {
            var options = new SearchOptions();
            options.IsRegex.Should().BeFalse();
            options.CaseSensitive.Should().BeFalse();
            options.IsDryRun.Should().BeFalse();
            options.Encoding.Should().Be(FileEncoding.Auto);
            options.Verbose.Should().BeFalse();
            options.Quiet.Should().BeFalse();
            options.IncludeHidden.Should().BeFalse();
            options.MaxDepth.Should().BeNull();
            options.Pattern.Should().BeNull();
            options.Replacement.Should().BeNull();
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public void Properties_ShouldBeSettable(bool isRegex, bool caseSensitive, bool isDryRun)
        {
            var options = new SearchOptions
            {
                IsRegex = isRegex,
                CaseSensitive = caseSensitive,
                IsDryRun = isDryRun,
                Pattern = "test",
                Replacement = "value"
            };
            options.IsRegex.Should().Be(isRegex);
            options.CaseSensitive.Should().Be(caseSensitive);
            options.IsDryRun.Should().Be(isDryRun);
            options.Pattern.Should().Be("test");
            options.Replacement.Should().Be("value");
        }

        [Theory]
        [InlineData(FileEncoding.Auto)]
        [InlineData(FileEncoding.Utf8)]
        [InlineData(FileEncoding.Ascii)]
        [InlineData(FileEncoding.Unicode)]
        public void Encoding_ShouldBeSettable(FileEncoding encoding)
        {
            var options = new SearchOptions { Encoding = encoding };
            options.Encoding.Should().Be(encoding);
        }

        [Fact]
        public void MaxDepth_ShouldBeNullByDefault()
        {
            var options = new SearchOptions();
            options.MaxDepth.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void MaxDepth_ShouldBeSettable(int maxDepth)
        {
            var options = new SearchOptions { MaxDepth = maxDepth };
            options.MaxDepth.Should().Be(maxDepth);
        }
    }
}
