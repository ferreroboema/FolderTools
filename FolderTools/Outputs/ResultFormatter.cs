using System;
using System.IO;
using FolderTools.Models;
using FolderTools.Utilities;

namespace FolderTools.Outputs
{
    /// <summary>
    /// Formats and displays operation results to the console
    /// </summary>
    public class ResultFormatter
    {
        private readonly bool _useColors;

        public ResultFormatter(bool useColors = true)
        {
            _useColors = useColors && !Console.IsErrorRedirected && !Console.IsOutputRedirected;
        }

        /// <summary>
        /// Prints a summary of the replacement operation results
        /// </summary>
        /// <param name="result">The result to summarize</param>
        /// <param name="isDryRun">Whether this was a dry-run operation</param>
        public void PrintSummary(ReplacementResult result, bool isDryRun)
        {
            string operation = isDryRun ? "Dry-run completed" : "Operation completed";

            Console.WriteLine();
            Console.WriteLine("=== " + operation + " ===");
            Console.WriteLine($"Files processed: {result.ProcessedFiles}");
            Console.WriteLine($"Total replacements: {result.TotalReplacements}");
            Console.WriteLine($"Files with errors: {result.FilesWithErrors}");
            Console.WriteLine($"Files skipped: {result.SkippedFiles.Count}");
        }

        /// <summary>
        /// Prints detailed results for each file
        /// </summary>
        /// <param name="result">The result to display</param>
        /// <param name="rootDirectory">Root directory for relative path display</param>
        /// <param name="verbose">Whether to show verbose output</param>
        public void PrintFileResults(ReplacementResult result, string rootDirectory, bool verbose)
        {
            if (!verbose && result.FilesWithErrors == 0 && result.SkippedFiles.Count == 0)
            {
                return;
            }

            Console.WriteLine();

            // Print successful file results (only in verbose mode)
            if (verbose && result.FileResults.Count > 0)
            {
                Console.WriteLine("--- Detailed Results ---");
                foreach (var fileResult in result.FileResults)
                {
                    if (fileResult.Success)
                    {
                        string relativePath = GetRelativePath(rootDirectory, fileResult.FilePath);
                        WriteSuccess($"  {relativePath}: {fileResult.ReplacementCount} replacement(s)");
                    }
                }
            }

            // Print errors
            if (result.FilesWithErrors > 0)
            {
                Console.WriteLine();
                Console.WriteLine("--- Errors ---");
                foreach (var fileResult in result.FileResults)
                {
                    if (!fileResult.Success)
                    {
                        string relativePath = GetRelativePath(rootDirectory, fileResult.FilePath);
                        WriteError($"  {relativePath}: {fileResult.ErrorMessage}");
                    }
                }
            }

            // Print skipped files (only in verbose mode)
            if (verbose && result.SkippedFiles.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("--- Skipped Files ---");
                foreach (var skipped in result.SkippedFiles)
                {
                    string relativePath = GetRelativePath(rootDirectory, skipped.FilePath);
                    WriteWarning($"  {relativePath}: {skipped.Reason}");
                }
            }
        }

        /// <summary>
        /// Prints a header before processing begins
        /// </summary>
        /// <param name="rootDirectory">Root directory being processed</param>
        /// <param name="pattern">Search pattern</param>
        /// <param name="replacement">Replacement text</param>
        /// <param name="isDryRun">Whether this is a dry-run</param>
        public void PrintHeader(string rootDirectory, string pattern, string replacement, bool isDryRun)
        {
            Console.WriteLine();
            Console.WriteLine("=== FolderTools - Find and Replace ===");
            Console.WriteLine($"Directory: {rootDirectory}");
            Console.WriteLine($"Search pattern: {EscapeForDisplay(pattern)}");
            Console.WriteLine($"Replacement: {EscapeForDisplay(replacement)}");
            Console.WriteLine($"Mode: {(isDryRun ? "Dry-run (no changes will be made)" : "Live (files will be modified)")}");
            Console.WriteLine();
        }

        /// <summary>
        /// Prints an error message
        /// </summary>
        public void PrintError(string message)
        {
            WriteError($"ERROR: {message}");
        }

        /// <summary>
        /// Prints a warning message
        /// </summary>
        public void PrintWarning(string message)
        {
            WriteWarning($"WARNING: {message}");
        }

        /// <summary>
        /// Prints an info message
        /// </summary>
        public void PrintInfo(string message)
        {
            Console.WriteLine($"INFO: {message}");
        }

        private string GetRelativePath(string basePath, string fullPath)
        {
            try
            {
                return FileHelper.GetRelativePath(basePath, fullPath);
            }
            catch
            {
                return fullPath;
            }
        }

        private string EscapeForDisplay(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "(empty)";
            }

            // Show whitespace characters for better visibility
            return text
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private void WriteSuccess(string message)
        {
            if (_useColors)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        private void WriteError(string message)
        {
            if (_useColors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        private void WriteWarning(string message)
        {
            if (_useColors)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
