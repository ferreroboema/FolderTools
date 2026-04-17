using System;

namespace FolderTools.Models
{
    /// <summary>
    /// Sort order for prefix/suffix numbering rename mode
    /// </summary>
    public enum RenameSortOrder
    {
        /// <summary>
        /// Alphabetical by filename
        /// </summary>
        Name,

        /// <summary>
        /// By last write time (oldest first)
        /// </summary>
        Date,

        /// <summary>
        /// By file size (smallest first)
        /// </summary>
        Size
    }

    /// <summary>
    /// Configuration options for file rename operations
    /// </summary>
    public class RenameOptions
    {
        // Sub-mode 1: Find/Replace in filenames
        /// <summary>
        /// Text or regex pattern to search for in filenames
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Replacement text for matched filename patterns
        /// </summary>
        public string Replacement { get; set; }

        /// <summary>
        /// Whether the pattern is a regular expression
        /// </summary>
        public bool IsRegex { get; set; }

        /// <summary>
        /// Whether the search should be case-sensitive
        /// </summary>
        public bool CaseSensitive { get; set; }

        // Sub-mode 2: CSV mapping
        /// <summary>
        /// Path to CSV file with old_name,new_name pairs
        /// </summary>
        public string RenameFilePath { get; set; }

        // Sub-mode 3: Prefix/Suffix + Numbering
        /// <summary>
        /// Prefix to add before the filename (before extension)
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Suffix to add before the file extension
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Starting number for sequential numbering (default: 1)
        /// </summary>
        public int StartNumber { get; set; }

        /// <summary>
        /// Zero-padding width for numbers (0 = no padding)
        /// </summary>
        public int Padding { get; set; }

        /// <summary>
        /// Sort order for files before applying numbering
        /// </summary>
        public RenameSortOrder SortOrder { get; set; }

        // Common options
        /// <summary>
        /// Preview changes without actually renaming files
        /// </summary>
        public bool IsDryRun { get; set; }

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

        /// <summary>
        /// Maximum directory recursion depth (null = unlimited)
        /// </summary>
        public int? MaxDepth { get; set; }

        /// <summary>
        /// Whether find/replace sub-mode is active
        /// </summary>
        public bool IsFindReplaceMode => !string.IsNullOrEmpty(Pattern);

        /// <summary>
        /// Whether CSV mapping sub-mode is active
        /// </summary>
        public bool IsCsvMode => !string.IsNullOrEmpty(RenameFilePath);

        /// <summary>
        /// Whether prefix/suffix numbering sub-mode is active
        /// </summary>
        public bool IsPrefixSuffixMode => !string.IsNullOrEmpty(Prefix) || !string.IsNullOrEmpty(Suffix);

        public RenameOptions()
        {
            IsRegex = false;
            CaseSensitive = false;
            StartNumber = 1;
            Padding = 0;
            SortOrder = RenameSortOrder.Name;
            IsDryRun = false;
            Verbose = false;
            Quiet = false;
            IncludeHidden = false;
        }
    }
}
