using FluentAssertions;
using FolderTools.Models;
using FolderTools.Services;
using Moq;
using Xunit;

namespace FolderTools.Tests.Services
{
    public class TextReplacerTests
    {
        private readonly Mock<IFileHelper> _fileHelperMock;

        public TextReplacerTests()
        {
            _fileHelperMock = new Mock<IFileHelper>();
        }

        [Fact]
        public void Constructor_Default_ShouldCreateInstance()
        {
            var replacer = new TextReplacer();
            replacer.Should().NotBeNull();
        }

        [Fact]
        public void ReplaceInFile_LiteralReplacement_ShouldReplaceAllOccurrences()
        {
            var options = new SearchOptions
            {
                Pattern = "World",
                Replacement = "Universe",
                IsRegex = false,
                CaseSensitive = false
            };

            string content = "Hello World, World!";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            _fileHelperMock.Setup(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out It.Ref<string>.IsAny))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(2);
        }

        [Theory]
        [InlineData("Hello", "Hi", false, 1)]
        [InlineData("hello", "Hi", false, 1)]
        [InlineData("hello", "Hi", true, 0)]
        public void ReplaceInFile_CaseSensitivity_ShouldWorkCorrectly(
            string pattern, string replacement, bool caseSensitive, int expectedCount)
        {
            var options = new SearchOptions
            {
                Pattern = pattern,
                Replacement = replacement,
                CaseSensitive = caseSensitive
            };

            string content = "Hello World";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            _fileHelperMock.Setup(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out It.Ref<string>.IsAny))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(expectedCount);
        }

        [Fact]
        public void ReplaceInFile_DryRun_ShouldNotModifyFile()
        {
            var options = new SearchOptions
            {
                Pattern = "World",
                Replacement = "Universe",
                IsDryRun = true
            };

            string content = "Hello World";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(1);
            _fileHelperMock.Verify(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out It.Ref<string>.IsAny), Times.Never);
        }

        [Fact]
        public void ReplaceInFile_ReadFails_ShouldReturnNegativeOne()
        {
            var options = new SearchOptions
            {
                Pattern = "World",
                Replacement = "Universe"
            };

            string content = null;
            string error = "Access denied";
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(false);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(-1);
        }

        [Fact]
        public void ReplaceInFile_WriteFails_ShouldReturnNegativeOne()
        {
            var options = new SearchOptions
            {
                Pattern = "World",
                Replacement = "Universe"
            };

            string readContent = "Hello World";
            string readError = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out readContent, out readError))
                .Returns(true);

            string writeError = "Access denied";
            _fileHelperMock.Setup(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out writeError))
                .Returns(false);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(-1);
        }

        [Fact]
        public void ReplaceInFile_NoMatches_ShouldReturnZero()
        {
            var options = new SearchOptions
            {
                Pattern = "NotFound",
                Replacement = "Replacement"
            };

            string content = "Hello World";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            _fileHelperMock.Setup(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out It.Ref<string>.IsAny))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(0);
            _fileHelperMock.Verify(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out It.Ref<string>.IsAny), Times.Never);
        }

        [Fact]
        public void ReplaceInFile_WithRegex_ShouldWorkCorrectly()
        {
            var options = new SearchOptions
            {
                Pattern = @"\d+",
                Replacement = "N",
                IsRegex = true
            };

            string content = "Test123 and 456";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            _fileHelperMock.Setup(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out It.Ref<string>.IsAny))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(2);
        }

        [Fact]
        public void ReplaceInFile_WithEmptyPattern_ShouldReturnNegativeOne()
        {
            var options = new SearchOptions
            {
                Pattern = "",
                Replacement = "Replacement"
            };

            string content = "Hello World";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(-1);
        }

        [Fact]
        public void ReplaceInFile_WithNullContent_ShouldHandleGracefully()
        {
            var options = new SearchOptions
            {
                Pattern = "World",
                Replacement = "Universe"
            };

            string content = null;
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(0);
        }

        [Fact]
        public void ReplaceInFile_LiteralReplacementWithDollarSign_ShouldTreatReplacementAsLiteral()
        {
            // Regression test: $ in replacement must NOT be treated as regex backreference
            var options = new SearchOptions
            {
                Pattern = "price: $10",
                Replacement = "price: $20",
                CaseSensitive = false
            };

            string content = "The price: $10 is old";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            string writeError = null;
            _fileHelperMock.Setup(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out writeError))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(1);
            _fileHelperMock.Verify(f => f.TryWriteFile(
                It.IsAny<string>(),
                It.Is<string>(c => c == "The price: $20 is old"),
                It.IsAny<FileEncoding>(),
                out It.Ref<string>.IsAny), Times.Once);
        }

        [Fact]
        public void ReplaceInFile_CaseInsensitiveMultipleMatches_ShouldReplaceAllWithLiteral()
        {
            var options = new SearchOptions
            {
                Pattern = "hello",
                Replacement = "$1world",
                CaseSensitive = false
            };

            string content = "Hello HELLO hello";
            string error = null;
            _fileHelperMock.Setup(f => f.TryReadFile(It.IsAny<string>(), It.IsAny<FileEncoding>(), out content, out error))
                .Returns(true);

            string writeError = null;
            _fileHelperMock.Setup(f => f.TryWriteFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncoding>(), out writeError))
                .Returns(true);

            var replacer = new TextReplacer(_fileHelperMock.Object);
            var result = replacer.ReplaceInFile("test.txt", options);
            result.Should().Be(3);
            _fileHelperMock.Verify(f => f.TryWriteFile(
                It.IsAny<string>(),
                It.Is<string>(c => c == "$1world $1world $1world"),
                It.IsAny<FileEncoding>(),
                out It.Ref<string>.IsAny), Times.Once);
        }
    }
}
