using System;
using System.Collections.Generic;
using FluentAssertions;
using FolderTools.Models;
using FolderTools.Services;
using Moq;
using Xunit;

namespace FolderTools.Tests.Services
{
    public class BulkFileProcessorTests
    {
        [Fact]
        public void Constructor_Default_ShouldCreateInstance()
        {
            // Act
            var processor = new BulkFileProcessor();

            // Assert
            processor.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithFileProcessor_ShouldSetFileProcessor()
        {
            // Arrange
            var mockFileProcessor = new Mock<IFileProcessor>();

            // Act
            var processor = new BulkFileProcessor(mockFileProcessor.Object);

            // Assert
            processor.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullFileProcessor_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BulkFileProcessor((IFileProcessor)null));
        }

        [Fact]
        public void ProcessBulk_WithNullPairs_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new BulkFileProcessor();
            var options = new SearchOptions();
            var filter = new FileFilter();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => processor.ProcessBulk(null, "C:\\Test", options, filter));
        }

        [Fact]
        public void ProcessBulk_WithEmptyPairs_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new BulkFileProcessor();
            var options = new SearchOptions();
            var filter = new FileFilter();
            var pairs = new List<SearchReplacePair>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => processor.ProcessBulk(pairs, "C:\\Test", options, filter));
        }

        [Fact]
        public void ProcessBulk_WithNullDirectory_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new BulkFileProcessor();
            var options = new SearchOptions();
            var filter = new FileFilter();
            var pairs = new List<SearchReplacePair> { new SearchReplacePair("old", "new", 1) };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => processor.ProcessBulk(pairs, null, options, filter));
        }

        [Fact]
        public void ProcessBulk_WithEmptyDirectory_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new BulkFileProcessor();
            var options = new SearchOptions();
            var filter = new FileFilter();
            var pairs = new List<SearchReplacePair> { new SearchReplacePair("old", "new", 1) };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => processor.ProcessBulk(pairs, "", options, filter));
        }

        [Fact]
        public void ProcessBulk_WithNullOptions_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new BulkFileProcessor();
            var filter = new FileFilter();
            var pairs = new List<SearchReplacePair> { new SearchReplacePair("old", "new", 1) };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => processor.ProcessBulk(pairs, "C:\\Test", null, filter));
        }

        [Fact]
        public void ProcessBulk_WithNullFilter_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new BulkFileProcessor();
            var options = new SearchOptions();
            var pairs = new List<SearchReplacePair> { new SearchReplacePair("old", "new", 1) };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => processor.ProcessBulk(pairs, "C:\\Test", options, null));
        }

        [Fact]
        public void ProcessBulk_WithValidPairs_ShouldProcessAllPairs()
        {
            // Arrange
            var mockFileProcessor = new Mock<IFileProcessor>();
            var processor = new BulkFileProcessor(mockFileProcessor.Object);

            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("old1", "new1", 1),
                new SearchReplacePair("old2", "new2", 2)
            };

            var options = new SearchOptions { IsDryRun = true, CaseSensitive = true };
            var filter = new FileFilter();

            var result1 = new ReplacementResult();
            result1.AddResult("file1.txt", 5);
            var result2 = new ReplacementResult();
            result2.AddResult("file2.txt", 3);

            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.Is<SearchOptions>(o => o.Pattern == "old1" && o.Replacement == "new1"), filter))
                            .Returns(result1);
            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.Is<SearchOptions>(o => o.Pattern == "old2" && o.Replacement == "new2"), filter))
                            .Returns(result2);

            // Act
            var bulkResult = processor.ProcessBulk(pairs, "C:\\Test", options, filter);

            // Assert
            bulkResult.TotalPairs.Should().Be(2);
            bulkResult.SuccessfulPairs.Should().Be(2);
            bulkResult.FailedPairs.Should().Be(0);
            bulkResult.TotalFilesProcessed.Should().Be(2);
            bulkResult.TotalReplacements.Should().Be(8);
        }

        [Fact]
        public void ProcessBulk_WithInvalidPair_ShouldContinueProcessing()
        {
            // Arrange
            var mockFileProcessor = new Mock<IFileProcessor>();
            var processor = new BulkFileProcessor(mockFileProcessor.Object);

            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("old1", "new1", 1),
                new SearchReplacePair("", "new2", 2), // Invalid
                new SearchReplacePair("old3", "new3", 3)
            };

            var options = new SearchOptions();
            var filter = new FileFilter();

            var result1 = new ReplacementResult();
            result1.AddResult("file1.txt", 5);
            var result3 = new ReplacementResult();
            result3.AddResult("file3.txt", 3);

            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.Is<SearchOptions>(o => o.Pattern == "old1"), filter))
                            .Returns(result1);
            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.Is<SearchOptions>(o => o.Pattern == "old3"), filter))
                            .Returns(result3);

            // Act
            var bulkResult = processor.ProcessBulk(pairs, "C:\\Test", options, filter);

            // Assert
            bulkResult.TotalPairs.Should().Be(3);
            bulkResult.SuccessfulPairs.Should().Be(2);
            bulkResult.FailedPairs.Should().Be(1);
            bulkResult.TotalFilesProcessed.Should().Be(2);
            bulkResult.TotalReplacements.Should().Be(8);

            bulkResult.PairResults[1].Success.Should().BeFalse();
            bulkResult.PairResults[1].Error.Should().Be("Invalid search pattern (empty or null)");
        }

        [Fact]
        public void ProcessBulk_WithExceptionInProcessing_ShouldContinueProcessing()
        {
            // Arrange
            var mockFileProcessor = new Mock<IFileProcessor>();
            var processor = new BulkFileProcessor(mockFileProcessor.Object);

            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("old1", "new1", 1),
                new SearchReplacePair("old2", "new2", 2)
            };

            var options = new SearchOptions();
            var filter = new FileFilter();

            var result1 = new ReplacementResult();
            result1.AddResult("file1.txt", 5);

            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.Is<SearchOptions>(o => o.Pattern == "old1"), filter))
                            .Returns(result1);
            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.Is<SearchOptions>(o => o.Pattern == "old2"), filter))
                            .Throws(new Exception("Processing error"));

            // Act
            var bulkResult = processor.ProcessBulk(pairs, "C:\\Test", options, filter);

            // Assert
            bulkResult.TotalPairs.Should().Be(2);
            bulkResult.SuccessfulPairs.Should().Be(1);
            bulkResult.FailedPairs.Should().Be(1);
            bulkResult.TotalFilesProcessed.Should().Be(1);
            bulkResult.TotalReplacements.Should().Be(5);

            bulkResult.PairResults[1].Success.Should().BeFalse();
            bulkResult.PairResults[1].Error.Should().Be("Processing error");
        }

        [Fact]
        public void ProcessBulk_ShouldCopyBaseOptionsToEachPair()
        {
            // Arrange
            var mockFileProcessor = new Mock<IFileProcessor>();
            var processor = new BulkFileProcessor(mockFileProcessor.Object);

            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("old", "new", 1)
            };

            var options = new SearchOptions
            {
                IsDryRun = true,
                CaseSensitive = true,
                IsRegex = true,
                Encoding = FileEncoding.Utf8,
                MaxDepth = 5,
                Verbose = true,
                Quiet = false,
                IncludeHidden = true
            };
            var filter = new FileFilter();

            var result = new ReplacementResult();
            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.IsAny<SearchOptions>(), filter))
                            .Returns(result);

            // Act
            processor.ProcessBulk(pairs, "C:\\Test", options, filter);

            // Assert
            mockFileProcessor.Verify(p => p.ProcessDirectory("C:\\Test",
                It.Is<SearchOptions>(o =>
                    o.IsDryRun == true &&
                    o.CaseSensitive == true &&
                    o.IsRegex == true &&
                    o.Encoding == FileEncoding.Utf8 &&
                    o.MaxDepth == 5 &&
                    o.Verbose == true &&
                    o.Quiet == false &&
                    o.IncludeHidden == true &&
                    o.Pattern == "old" &&
                    o.Replacement == "new"
                ), filter), Times.Once);
        }

        [Fact]
        public void ProcessBulk_ShouldSetPairSpecificPatternAndReplacement()
        {
            // Arrange
            var mockFileProcessor = new Mock<IFileProcessor>();
            var processor = new BulkFileProcessor(mockFileProcessor.Object);

            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("pattern1", "replacement1", 1),
                new SearchReplacePair("pattern2", "replacement2", 2),
                new SearchReplacePair("pattern3", "", 3)
            };

            var options = new SearchOptions();
            var filter = new FileFilter();

            var result = new ReplacementResult();
            mockFileProcessor.Setup(p => p.ProcessDirectory("C:\\Test", It.IsAny<SearchOptions>(), filter))
                            .Returns(result);

            // Act
            processor.ProcessBulk(pairs, "C:\\Test", options, filter);

            // Assert
            mockFileProcessor.Verify(p => p.ProcessDirectory("C:\\Test",
                It.Is<SearchOptions>(o => o.Pattern == "pattern1" && o.Replacement == "replacement1"), filter), Times.Once);
            mockFileProcessor.Verify(p => p.ProcessDirectory("C:\\Test",
                It.Is<SearchOptions>(o => o.Pattern == "pattern2" && o.Replacement == "replacement2"), filter), Times.Once);
            mockFileProcessor.Verify(p => p.ProcessDirectory("C:\\Test",
                It.Is<SearchOptions>(o => o.Pattern == "pattern3" && o.Replacement == ""), filter), Times.Once);
        }
    }
}
