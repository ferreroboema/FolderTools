using FolderTools.Models;

namespace FolderTools.Services
{
    /// <summary>
    /// Interface for text replacement operations
    /// </summary>
    public interface ITextReplacer
    {
        /// <summary>
        /// Performs find and replace operations on a file
        /// </summary>
        /// <param name="filePath">Path to the file to process</param>
        /// <param name="options">Search options including pattern and replacement</param>
        /// <returns>Number of replacements made, or -1 if an error occurred</returns>
        int ReplaceInFile(string filePath, SearchOptions options);
    }
}
