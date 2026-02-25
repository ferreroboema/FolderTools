using System;
using System.Collections.Generic;

namespace FolderTools.Models
{
    /// <summary>
    /// Result of a find and replace operation
    /// </summary>
    public class ReplacementResult
    {
        /// <summary>
        /// Number of files successfully processed
        /// </summary>
        public int ProcessedFiles { get; private set; }

        /// <summary>
        /// Total number of replacements made across all files
        /// </summary>
        public int TotalReplacements { get; private set; }

        /// <summary>
        /// Number of files that had errors during processing
        /// </summary>
        public int FilesWithErrors { get; private set; }

        /// <summary>
        /// Detailed results for each file
        /// </summary>
        public List<FileReplacementResult> FileResults { get; private set; }

        /// <summary>
        /// Files that were skipped (binary, locked, etc.)
        /// </summary>
        public List<SkippedFile> SkippedFiles { get; private set; }

        public ReplacementResult()
        {
            FileResults = new List<FileReplacementResult>();
            SkippedFiles = new List<SkippedFile>();
        }

        /// <summary>
        /// Adds a successful file processing result
        /// </summary>
        /// <param name="filePath">Path to the file that was processed</param>
        /// <param name="replacementCount">Number of replacements made in the file</param>
        public void AddResult(string filePath, int replacementCount)
        {
            ProcessedFiles++;
            TotalReplacements += replacementCount;

            FileResults.Add(new FileReplacementResult
            {
                FilePath = filePath,
                ReplacementCount = replacementCount,
                Success = true
            });
        }

        /// <summary>
        /// Adds a file processing error
        /// </summary>
        /// <param name="filePath">Path to the file that had an error</param>
        /// <param name="errorMessage">Error message describing what went wrong</param>
        public void AddError(string filePath, string errorMessage)
        {
            FilesWithErrors++;

            FileResults.Add(new FileReplacementResult
            {
                FilePath = filePath,
                ReplacementCount = 0,
                Success = false,
                ErrorMessage = errorMessage
            });
        }

        /// <summary>
        /// Adds a skipped file
        /// </summary>
        /// <param name="filePath">Path to the file that was skipped</param>
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
    /// Result for a single file replacement operation
    /// </summary>
    public class FileReplacementResult
    {
        public string FilePath { get; set; }
        public int ReplacementCount { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Information about a skipped file
    /// </summary>
    public class SkippedFile
    {
        public string FilePath { get; set; }
        public string Reason { get; set; }
    }
}
