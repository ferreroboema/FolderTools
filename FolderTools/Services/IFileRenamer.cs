using FolderTools.Models;

namespace FolderTools.Services
{
    /// <summary>
    /// Interface for file rename operations
    /// </summary>
    public interface IFileRenamer
    {
        /// <summary>
        /// Renames files in a directory according to rename options and file filter
        /// </summary>
        /// <param name="rootDirectory">Root directory to start renaming from</param>
        /// <param name="renameOptions">Rename configuration options</param>
        /// <param name="filter">File filtering criteria</param>
        /// <returns>Result of the rename operation</returns>
        RenameResult RenameFiles(string rootDirectory, RenameOptions renameOptions, FileFilter filter);
    }
}
