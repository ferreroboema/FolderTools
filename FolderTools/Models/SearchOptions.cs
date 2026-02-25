using System;

namespace FolderTools.Models
{
    /// <summary>
    /// Configuration options for search and replace operations
    /// </summary>
    public class SearchOptions
    {
        /// <summary>
        /// The text or regex pattern to search for
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// The text to replace matches with
        /// </summary>
        public string Replacement { get; set; }

        /// <summary>
        /// Whether the pattern is a regular expression (false = literal string)
        /// </summary>
        public bool IsRegex { get; set; }

        /// <summary>
        /// Whether the search should be case-sensitive
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Preview changes without actually modifying files
        /// </summary>
        public bool IsDryRun { get; set; }

        /// <summary>
        /// Text encoding to use when reading/writing files
        /// </summary>
        public FileEncoding Encoding { get; set; }

        /// <summary>
        /// Maximum directory recursion depth (null = unlimited)
        /// </summary>
        public int? MaxDepth { get; set; }

        /// <summary>
        /// Verbose output mode
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Quiet mode (minimal output)
        /// </summary>
        public bool Quiet { get; set; }

        /// <summary>
        /// Include hidden and system files
        /// </summary>
        public bool IncludeHidden { get; set; }

        public SearchOptions()
        {
            IsRegex = false;
            CaseSensitive = false;
            IsDryRun = false;
            Encoding = FileEncoding.Auto;
            Verbose = false;
            Quiet = false;
            IncludeHidden = false;
        }
    }

    /// <summary>
    /// Supported file encoding types
    /// </summary>
    public enum FileEncoding
    {
        Auto,
        Utf8,
        Ascii,
        Unicode
    }
}
