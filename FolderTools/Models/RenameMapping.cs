using System;

namespace FolderTools.Models
{
    /// <summary>
    /// Represents a single old_name->new_name mapping for CSV rename mode
    /// </summary>
    public class RenameMapping
    {
        /// <summary>
        /// The original filename to match
        /// </summary>
        public string OldName { get; set; }

        /// <summary>
        /// The new filename to rename to
        /// </summary>
        public string NewName { get; set; }

        /// <summary>
        /// Line number in the CSV file (1-based) for error reporting
        /// </summary>
        public int LineNumber { get; set; }

        public RenameMapping() { }

        /// <summary>
        /// Creates a new RenameMapping with the specified values
        /// </summary>
        /// <param name="oldName">Original filename</param>
        /// <param name="newName">New filename</param>
        /// <param name="lineNumber">Line number in CSV file (1-based)</param>
        public RenameMapping(string oldName, string newName, int lineNumber)
        {
            OldName = oldName;
            NewName = newName;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Validates whether this rename mapping is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(OldName)
                && !string.IsNullOrWhiteSpace(NewName)
                && !string.Equals(OldName, NewName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a string representation of this mapping
        /// </summary>
        public override string ToString()
        {
            return $"Line {LineNumber}: \"{OldName}\" -> \"{NewName}\"";
        }
    }
}
