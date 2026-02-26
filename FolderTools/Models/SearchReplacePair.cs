using System;

namespace FolderTools.Models
{
    /// <summary>
    /// Represents a single search/replace pair for bulk operations
    /// </summary>
    public class SearchReplacePair
    {
        /// <summary>
        /// The text or regex pattern to search for
        /// </summary>
        public string SearchPattern { get; set; }

        /// <summary>
        /// The text to replace matches with
        /// </summary>
        public string Replacement { get; set; }

        /// <summary>
        /// Line number in the CSV file (1-based) for error reporting
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Creates a new SearchReplacePair
        /// </summary>
        public SearchReplacePair()
        {
        }

        /// <summary>
        /// Creates a new SearchReplacePair with the specified values
        /// </summary>
        /// <param name="searchPattern">The text or regex pattern to search for</param>
        /// <param name="replacement">The text to replace matches with</param>
        /// <param name="lineNumber">Line number in the CSV file (1-based)</param>
        public SearchReplacePair(string searchPattern, string replacement, int lineNumber)
        {
            SearchPattern = searchPattern;
            Replacement = replacement;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Validates whether this search/replace pair is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SearchPattern);
        }

        /// <summary>
        /// Returns a string representation of this pair
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            string replacementDisplay = string.IsNullOrEmpty(Replacement) ? "(empty)" : Replacement;
            return $"Line {LineNumber}: \"{SearchPattern}\" -> \"{replacementDisplay}\"";
        }
    }
}
