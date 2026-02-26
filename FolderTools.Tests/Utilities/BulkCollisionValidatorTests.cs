using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FolderTools.Models;
using FolderTools.Utilities;
using Xunit;

namespace FolderTools.Tests.Utilities
{
    public class BulkCollisionValidatorTests
    {
        private readonly BulkCollisionValidator _validator;

        public BulkCollisionValidatorTests()
        {
            _validator = new BulkCollisionValidator();
        }

        [Fact]
        public void DetectCollisions_WithNoCollisions_ShouldReturnEmptyResult()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("foo", "bar", 1),
                new SearchReplacePair("baz", "qux", 2),
                new SearchReplacePair("hello", "world", 3)
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeFalse();
            result.CollisionCount.Should().Be(0);
        }

        [Fact]
        public void DetectCollisions_WithSimpleCollision_ShouldDetectCorrectly()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("V123", "V146", 1),
                new SearchReplacePair("V146", "V178", 2)
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeTrue();
            result.CollisionCount.Should().Be(1);
            result.Collisions[0].CollisionChain.Count.Should().Be(2);
            result.Collisions[0].CollisionChain[0].SearchPattern.Should().Be("V123");
            result.Collisions[0].CollisionChain[1].SearchPattern.Should().Be("V146");
        }

        [Fact]
        public void DetectCollisions_WithChain_ShouldDetectFullChain()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("A", "B", 1),
                new SearchReplacePair("B", "C", 2),
                new SearchReplacePair("C", "D", 3)
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeTrue();
            result.Collisions[0].CollisionChain.Count.Should().Be(3);
        }

        [Fact]
        public void DetectCollisions_WithMultipleIndependentChains_ShouldDetectAll()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("X", "Y", 1),
                new SearchReplacePair("Y", "Z", 2),
                new SearchReplacePair("A", "B", 3),
                new SearchReplacePair("B", "C", 4)
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeTrue();
            result.CollisionCount.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void DetectCollisions_WithEmptyReplacement_ShouldSkip()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("V123", "", 1),  // Empty replacement (deletion)
                new SearchReplacePair("", "V146", 2)   // Empty pattern is skipped
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeFalse();
        }

        [Fact]
        public void DetectCollisions_WithSelfReference_ShouldSkip()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("V123", "V123", 1),  // Self-reference (no-op)
                new SearchReplacePair("V146", "V178", 2)
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeFalse();
        }

        [Fact]
        public void DetectCollisions_WithCircularReference_ShouldDetect()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("A", "B", 1),
                new SearchReplacePair("B", "A", 2)
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeTrue();
            result.Collisions[0].CollisionChain.Count.Should().Be(2);
        }

        [Fact]
        public void DetectCollisions_WithEmptyList_ShouldReturnEmptyResult()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>();

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeFalse();
        }

        [Fact]
        public void DetectCollisions_WithNullList_ShouldReturnEmptyResult()
        {
            // Arrange
            List<SearchReplacePair> pairs = null;

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeFalse();
        }

        [Fact]
        public void DetectCollisions_WithCollisionInMiddleOfList_ShouldDetect()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("foo", "bar", 1),
                new SearchReplacePair("V123", "V146", 2),
                new SearchReplacePair("V146", "V178", 3),
                new SearchReplacePair("hello", "world", 4)
            };

            // Act
            var result = _validator.DetectCollisions(pairs);

            // Assert
            result.HasCollisions.Should().BeTrue();
        }

        [Fact]
        public void GetChainVisualization_WithTwoPairs_ShouldFormatCorrectly()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("V123", "V146", 1),
                new SearchReplacePair("V146", "V178", 2)
            };
            var result = _validator.DetectCollisions(pairs);
            var collision = result.Collisions[0];

            // Act
            string visualization = collision.GetChainVisualization();

            // Assert
            visualization.Should().Be("\"V123\" -> \"V146\"");
        }

        [Fact]
        public void GetChainVisualization_WithThreePairs_ShouldFormatCorrectly()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("A", "B", 1),
                new SearchReplacePair("B", "C", 2),
                new SearchReplacePair("C", "D", 3)
            };
            var result = _validator.DetectCollisions(pairs);
            var collision = result.Collisions[0];

            // Act
            string visualization = collision.GetChainVisualization();

            // Assert
            visualization.Should().Be("\"A\" -> \"B\" -> \"C\"");
        }

        [Fact]
        public void GetSummaryMessage_WithNoCollisions_ShouldReturnNoCollisions()
        {
            // Arrange
            var result = new CollisionDetectionResult();

            // Act
            string message = result.GetSummaryMessage();

            // Assert
            message.Should().Be("No collisions detected.");
        }

        [Fact]
        public void GetSummaryMessage_WithCollisions_ShouldReturnCorrectCount()
        {
            // Arrange
            var pairs = new List<SearchReplacePair>
            {
                new SearchReplacePair("A", "B", 1),
                new SearchReplacePair("B", "C", 2)
            };
            var result = _validator.DetectCollisions(pairs);

            // Act
            string message = result.GetSummaryMessage();

            // Assert
            message.Should().Contain("1 collision");
        }
    }
}
