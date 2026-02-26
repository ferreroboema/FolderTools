using System.Collections.Generic;
using System.Text;

namespace FolderTools.Models
{
    /// <summary>
    /// Represents a single collision between search/replace pairs
    /// where a replacement value from one pair becomes a search pattern in a subsequent pair
    /// </summary>
    public class CollisionInfo
    {
        /// <summary>
        /// The chain of pairs involved in the collision, in order
        /// </summary>
        public List<SearchReplacePair> CollisionChain { get; set; }

        /// <summary>
        /// Creates a new CollisionInfo
        /// </summary>
        public CollisionInfo()
        {
            CollisionChain = new List<SearchReplacePair>();
        }

        /// <summary>
        /// Creates a new CollisionInfo with an initial chain
        /// </summary>
        /// <param name="chain">The chain of pairs involved in the collision</param>
        public CollisionInfo(List<SearchReplacePair> chain)
        {
            CollisionChain = chain ?? new List<SearchReplacePair>();
        }

        /// <summary>
        /// Gets a string representation of the collision chain for display
        /// </summary>
        /// <returns>A visual representation of the collision chain</returns>
        public string GetChainVisualization()
        {
            if (CollisionChain.Count == 0)
                return "(empty chain)";

            var sb = new StringBuilder();
            for (int i = 0; i < CollisionChain.Count; i++)
            {
                // Add the search pattern
                sb.Append($"\"{EscapeString(CollisionChain[i].SearchPattern)}\"");

                // Add arrow and replacement if not the last item
                if (i < CollisionChain.Count - 1)
                {
                    sb.Append(" -> ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets detailed information about each pair in the chain
        /// </summary>
        /// <returns>List of formatted strings describing each pair</returns>
        public List<string> GetDetailedChainInfo()
        {
            var details = new List<string>();

            for (int i = 0; i < CollisionChain.Count; i++)
            {
                var pair = CollisionChain[i];
                string replacementDisplay = string.IsNullOrEmpty(pair.Replacement) ? "(empty)" : $"\"{EscapeString(pair.Replacement)}\"";

                if (i < CollisionChain.Count - 1)
                {
                    var nextPair = CollisionChain[i + 1];
                    details.Add($"Line {pair.LineNumber}: \"{EscapeString(pair.SearchPattern)}\" -> {replacementDisplay}");
                    details.Add($"  This value will be replaced again by line {nextPair.LineNumber}");
                }
                else
                {
                    details.Add($"Line {pair.LineNumber}: \"{EscapeString(pair.SearchPattern)}\" -> {replacementDisplay}");
                }
            }

            return details;
        }

        private string EscapeString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            // Escape quotes for display
            return s.Replace("\"", "\\\"");
        }
    }

    /// <summary>
    /// Aggregates all collisions detected with formatted warning messages
    /// </summary>
    public class CollisionDetectionResult
    {
        /// <summary>
        /// List of all detected collisions
        /// </summary>
        public List<CollisionInfo> Collisions { get; set; }

        /// <summary>
        /// Whether any collisions were detected
        /// </summary>
        public bool HasCollisions => Collisions.Count > 0;

        /// <summary>
        /// The total number of collisions detected
        /// </summary>
        public int CollisionCount => Collisions.Count;

        /// <summary>
        /// Creates a new CollisionDetectionResult
        /// </summary>
        public CollisionDetectionResult()
        {
            Collisions = new List<CollisionInfo>();
        }

        /// <summary>
        /// Adds a collision to the result
        /// </summary>
        /// <param name="collision">The collision to add</param>
        public void AddCollision(CollisionInfo collision)
        {
            Collisions.Add(collision);
        }

        /// <summary>
        /// Gets a summary message for all collisions
        /// </summary>
        /// <returns>A summary message describing the collisions</returns>
        public string GetSummaryMessage()
        {
            if (!HasCollisions)
                return "No collisions detected.";

            int totalPairs = 0;
            foreach (var collision in Collisions)
            {
                totalPairs += collision.CollisionChain.Count;
            }

            return $"{CollisionCount} collision(s) detected involving {totalPairs} pair(s).";
        }
    }
}
