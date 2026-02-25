using FluentAssertions;
using FolderTools.Models;
using Xunit;

namespace FolderTools.Tests.Models
{
    public class ReplacementResultTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithZeros()
        {
            var result = new ReplacementResult();
            result.ProcessedFiles.Should().Be(0);
            result.TotalReplacements.Should().Be(0);
            result.FilesWithErrors.Should().Be(0);
            result.FileResults.Should().NotBeNull().And.BeEmpty();
            result.SkippedFiles.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void AddResult_ShouldIncrementCounters()
        {
            var result = new ReplacementResult();
            result.AddResult("test.txt", 5);
            result.AddResult("test2.txt", 3);
            result.ProcessedFiles.Should().Be(2);
            result.TotalReplacements.Should().Be(8);
            result.FileResults.Should().HaveCount(2);
        }

        [Fact]
        public void AddResult_ShouldAddToFileResults()
        {
            var result = new ReplacementResult();
            result.AddResult("test.txt", 5);

            result.FileResults.Should().HaveCount(1);
            result.FileResults[0].FilePath.Should().Be("test.txt");
            result.FileResults[0].ReplacementCount.Should().Be(5);
            result.FileResults[0].Success.Should().BeTrue();
            result.FileResults[0].ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void AddError_ShouldIncrementErrorCount()
        {
            var result = new ReplacementResult();
            result.AddError("test.txt", "Access denied");
            result.AddError("test2.txt", "File not found");
            result.FilesWithErrors.Should().Be(2);
        }

        [Fact]
        public void AddError_ShouldAddToFileResults()
        {
            var result = new ReplacementResult();
            result.AddError("test.txt", "Access denied");

            result.FileResults.Should().HaveCount(1);
            result.FileResults[0].FilePath.Should().Be("test.txt");
            result.FileResults[0].ReplacementCount.Should().Be(0);
            result.FileResults[0].Success.Should().BeFalse();
            result.FileResults[0].ErrorMessage.Should().Be("Access denied");
        }

        [Fact]
        public void AddSkipped_ShouldAddToSkippedFiles()
        {
            var result = new ReplacementResult();
            result.AddSkipped("binary.bin", "Binary file");
            result.AddSkipped("locked.txt", "File locked");

            result.SkippedFiles.Should().HaveCount(2);
            result.SkippedFiles[0].FilePath.Should().Be("binary.bin");
            result.SkippedFiles[0].Reason.Should().Be("Binary file");
            result.SkippedFiles[1].FilePath.Should().Be("locked.txt");
            result.SkippedFiles[1].Reason.Should().Be("File locked");
        }

        [Fact]
        public void AddSkipped_ShouldNotIncrementProcessedFiles()
        {
            var result = new ReplacementResult();
            result.AddSkipped("binary.bin", "Binary file");
            result.ProcessedFiles.Should().Be(0);
            result.FilesWithErrors.Should().Be(0);
        }

        [Fact]
        public void MultipleOperations_ShouldTrackCorrectly()
        {
            var result = new ReplacementResult();
            result.AddResult("success.txt", 5);
            result.AddError("error.txt", "Access denied");
            result.AddSkipped("skip.bin", "Binary file");
            result.AddResult("success2.txt", 2);

            result.ProcessedFiles.Should().Be(2);
            result.TotalReplacements.Should().Be(7);
            result.FilesWithErrors.Should().Be(1);
            result.SkippedFiles.Should().HaveCount(1);
            result.FileResults.Should().HaveCount(3); // 2 success + 1 error
        }

        [Fact]
        public void AddResult_WithZeroReplacements_ShouldIncrementProcessedFiles()
        {
            var result = new ReplacementResult();
            result.AddResult("nomatch.txt", 0);
            result.ProcessedFiles.Should().Be(1);
            result.TotalReplacements.Should().Be(0);
        }
    }
}
