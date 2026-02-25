using FolderTools.Models;

namespace FolderTools.Services
{
    /// <summary>
    /// Interface for file processing operations
    /// </summary>
    public interface IFileProcessor
    {
        /// <summary>
        /// Processes all files in a directory matching the given filter criteria
        /// </summary>
        /// <param name="rootDirectory">Root directory to start processing from</param>
        /// <param name="options">Search and replace options</param>
        /// <param name="filter">File filtering criteria</param>
        /// <returns>Aggregated results from all processed files</returns>
        ReplacementResult ProcessDirectory(string rootDirectory, SearchOptions options, FileFilter filter);
    }
}
