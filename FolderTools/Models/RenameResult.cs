using System.Collections.Generic;

namespace FolderTools.Models
{
    /// <summary>
    /// Result of a file rename operation
    /// </summary>
    public class RenameResult
    {
        /// <summary>
        /// Number of files successfully processed
        /// </summary>
        public int ProcessedFiles { get; private set; }

        /// <summary>
        /// Total number of files renamed
        /// </summary>
        public int TotalRenamed { get; private set; }

        /// <summary>
        /// Number of files that had errors during processing
        /// </summary>
        public int FilesWithErrors { get; private set; }

        /// <summary>
        /// Detailed results for each file
        /// </summary>
        public List<FileRenameResult> FileResults { get; private set; }

        /// <summary>
        /// Files that were skipped (no match, locked, etc.)
        /// </summary>
        public List<SkippedFile> SkippedFiles { get; private set; }

        public RenameResult()
        {
            FileResults = new List<FileRenameResult>();
            SkippedFiles = new List<SkippedFile>();
        }

        /// <summary>
        /// Adds a successful rename result
        /// </summary>
        /// <param name="originalPath">Original file path</param>
        /// <param name="newPath">New file path after rename</param>
        public void AddResult(string originalPath, string newPath)
        {
            ProcessedFiles++;
            TotalRenamed++;

            FileResults.Add(new FileRenameResult
            {
                OriginalPath = originalPath,
                NewPath = newPath,
                Success = true
            });
        }

        /// <summary>
        /// Adds a file rename error
        /// </summary>
        /// <param name="filePath">Path to the file that had an error</param>
        /// <param name="errorMessage">Error message</param>
        public void AddError(string filePath, string errorMessage)
        {
            FilesWithErrors++;

            FileResults.Add(new FileRenameResult
            {
                OriginalPath = filePath,
                NewPath = null,
                Success = false,
                ErrorMessage = errorMessage
            });
        }

        /// <summary>
        /// Adds a skipped file
        /// </summary>
        /// <param name="filePath">Path to the skipped file</param>
        /// <param name="reason">Reason for skipping</param>
        public void AddSkipped(string filePath, string reason)
        {
            SkippedFiles.Add(new SkippedFile
            {
                FilePath = filePath,
                Reason = reason
            });
        }
    }

    /// <summary>
    /// Result for a single file rename operation
    /// </summary>
    public class FileRenameResult
    {
        /// <summary>
        /// Original file path before rename
        /// </summary>
        public string OriginalPath { get; set; }

        /// <summary>
        /// New file path after rename (null if failed)
        /// </summary>
        public string NewPath { get; set; }

        /// <summary>
        /// Whether the rename was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the rename failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
