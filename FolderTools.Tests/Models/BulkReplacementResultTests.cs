using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FolderTools.Models;
using Xunit;

namespace FolderTools.Tests.Models
{
    public class BulkReplacementResultTests
    {
        [Fact]
        public void Constructor_ShouldInitializeEmptyCollections()
        {
            // Arrange & Act
            var result = new BulkReplacementResult();

            // Assert
            result.PairResults.Should().NotBeNull();
            result.PairResults.Should().BeEmpty();
            result.TotalPairs.Should().Be(0);
            result.SuccessfulPairs.Should().Be(0);
            result.FailedPairs.Should().Be(0);
            result.TotalFilesProcessed.Should().Be(0);
            result.TotalReplacements.Should().Be(0);
        }

        [Fact]
        public void AddPairResult_WithSuccessfulPair_ShouldIncrementSuccessfulPairs()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();
            var pair = new SearchReplacePair("old", "new", 1);
            var replacementResult = new ReplacementResult();
            replacementResult.AddResult("file1.txt", 5);
            replacementResult.AddResult("file2.txt", 3);

            // Act
            bulkResult.AddPairResult(pair, replacementResult, null);

            // Assert
            bulkResult.TotalPairs.Should().Be(1);
            bulkResult.SuccessfulPairs.Should().Be(1);
            bulkResult.FailedPairs.Should().Be(0);
            bulkResult.TotalFilesProcessed.Should().Be(2);
            bulkResult.TotalReplacements.Should().Be(8);
        }

        [Fact]
        public void AddPairResult_WithFailedPair_ShouldIncrementFailedPairs()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();
            var pair = new SearchReplacePair("old", "new", 1);

            // Act
            bulkResult.AddPairResult(pair, null, "Error occurred");

            // Assert
            bulkResult.TotalPairs.Should().Be(1);
            bulkResult.SuccessfulPairs.Should().Be(0);
            bulkResult.FailedPairs.Should().Be(1);
            bulkResult.TotalFilesProcessed.Should().Be(0);
            bulkResult.TotalReplacements.Should().Be(0);
        }

        [Fact]
        public void AddPairResult_MultiplePairs_ShouldAggregateCorrectly()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();

            var pair1 = new SearchReplacePair("old1", "new1", 1);
            var result1 = new ReplacementResult();
            result1.AddResult("file1.txt", 5);

            var pair2 = new SearchReplacePair("old2", "new2", 2);
            var result2 = new ReplacementResult();
            result2.AddResult("file2.txt", 3);

            var pair3 = new SearchReplacePair("old3", "new3", 3);

            // Act
            bulkResult.AddPairResult(pair1, result1, null);
            bulkResult.AddPairResult(pair2, result2, null);
            bulkResult.AddPairResult(pair3, null, "Error");

            // Assert
            bulkResult.TotalPairs.Should().Be(3);
            bulkResult.SuccessfulPairs.Should().Be(2);
            bulkResult.FailedPairs.Should().Be(1);
            bulkResult.TotalFilesProcessed.Should().Be(2);
            bulkResult.TotalReplacements.Should().Be(8);
        }

        [Fact]
        public void GetAllErrors_WithFailedPairs_ShouldReturnErrorMessages()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();

            var pair1 = new SearchReplacePair("old1", "new1", 1);
            var pair2 = new SearchReplacePair("old2", "new2", 5);
            var pair3 = new SearchReplacePair("old3", "new3", 10);

            bulkResult.AddPairResult(pair1, null, "Error 1");
            bulkResult.AddPairResult(pair2, new ReplacementResult(), null); // Success
            bulkResult.AddPairResult(pair3, null, "Error 3");

            // Act
            var errors = bulkResult.GetAllErrors();

            // Assert
            errors.Should().HaveCount(2);
            errors.Should().Contain("Line 1: Error 1");
            errors.Should().Contain("Line 10: Error 3");
        }

        [Fact]
        public void GetAllErrors_WithNoFailedPairs_ShouldReturnEmptyList()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();

            var pair1 = new SearchReplacePair("old1", "new1", 1);
            var result1 = new ReplacementResult();

            bulkResult.AddPairResult(pair1, result1, null);

            // Act
            var errors = bulkResult.GetAllErrors();

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void GetSummary_ShouldReturnFormattedSummary()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();

            var pair1 = new SearchReplacePair("old1", "new1", 1);
            var result1 = new ReplacementResult();
            result1.AddResult("file1.txt", 5);

            var pair2 = new SearchReplacePair("old2", "new2", 2);
            var result2 = new ReplacementResult();
            result2.AddResult("file2.txt", 3);

            bulkResult.AddPairResult(pair1, result1, null);
            bulkResult.AddPairResult(pair2, result2, null);

            // Act
            var summary = bulkResult.GetSummary();

            // Assert
            summary.Should().Be("Pairs: 2 total, 2 successful, 0 failed | Files: 2 | Replacements: 8");
        }

        [Fact]
        public void GetSummary_WithFailures_ShouldIncludeFailedCount()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();

            var pair1 = new SearchReplacePair("old1", "new1", 1);
            var result1 = new ReplacementResult();
            result1.AddResult("file1.txt", 5);

            var pair2 = new SearchReplacePair("old2", "new2", 2);

            bulkResult.AddPairResult(pair1, result1, null);
            bulkResult.AddPairResult(pair2, null, "Error");

            // Act
            var summary = bulkResult.GetSummary();

            // Assert
            summary.Should().Be("Pairs: 2 total, 1 successful, 1 failed | Files: 1 | Replacements: 5");
        }

        [Fact]
        public void PairResults_ShouldContainCorrectData()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();

            var pair = new SearchReplacePair("old", "new", 1);
            var replacementResult = new ReplacementResult();
            replacementResult.AddResult("file1.txt", 5);

            // Act
            bulkResult.AddPairResult(pair, replacementResult, null);

            // Assert
            bulkResult.PairResults.Should().HaveCount(1);
            var pairResult = bulkResult.PairResults.First();
            pairResult.Pair.Should().Be(pair);
            pairResult.Result.Should().Be(replacementResult);
            pairResult.Error.Should().BeNull();
            pairResult.Success.Should().BeTrue();
        }

        [Fact]
        public void PairResults_WithFailedPair_ShouldContainError()
        {
            // Arrange
            var bulkResult = new BulkReplacementResult();

            var pair = new SearchReplacePair("old", "new", 1);

            // Act
            bulkResult.AddPairResult(pair, null, "Test error");

            // Assert
            bulkResult.PairResults.Should().HaveCount(1);
            var pairResult = bulkResult.PairResults.First();
            pairResult.Pair.Should().Be(pair);
            pairResult.Result.Should().BeNull();
            pairResult.Error.Should().Be("Test error");
            pairResult.Success.Should().BeFalse();
        }
    }
}
