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

        /// <summary>
        /// Prints a header for bulk mode processing
        /// </summary>
        /// <param name="csvFilePath">Path to the CSV file</param>
        /// <param name="rootDirectory">Root directory being processed</param>
        /// <param name="pairCount">Number of search/replace pairs</param>
        /// <param name="isDryRun">Whether this is a dry-run</param>
        public void PrintBulkHeader(string csvFilePath, string rootDirectory, int pairCount, bool isDryRun)
        {
            Console.WriteLine();
            Console.WriteLine("=== FolderTools - Bulk Find and Replace ===");
            Console.WriteLine($"CSV file: {csvFilePath}");
            Console.WriteLine($"Directory: {rootDirectory}");
            Console.WriteLine($"Pairs to process: {pairCount}");
            Console.WriteLine($"Mode: {(isDryRun ? "Dry-run (no changes will be made)" : "Live (files will be modified)")}");
            Console.WriteLine();
        }

        /// <summary>
        /// Prints detailed results for bulk mode
        /// </summary>
        /// <param name="result">The bulk replacement result</param>
        /// <param name="rootDirectory">Root directory for relative path display</param>
        /// <param name="verbose">Whether to show verbose output</param>
        public void PrintBulkResults(BulkReplacementResult result, string rootDirectory, bool verbose)
        {
            Console.WriteLine();

            // Print results for each pair
            foreach (var pairResult in result.PairResults)
            {
                Console.WriteLine($"--- Pair {pairResult.Pair.LineNumber}: \"{EscapeForDisplay(pairResult.Pair.SearchPattern)}\" -> \"{EscapeForDisplay(pairResult.Pair.Replacement)}\" ---");

                if (pairResult.Success)
                {
                    var fileResult = pairResult.Result;
                    WriteSuccess($"  Files processed: {fileResult.ProcessedFiles}");
                    WriteSuccess($"  Replacements: {fileResult.TotalReplacements}");

                    if (verbose)
                    {
                        foreach (var file in fileResult.FileResults)
                        {
                            if (file.Success)
                            {
                                string relativePath = GetRelativePath(rootDirectory, file.FilePath);
                                WriteSuccess($"    {relativePath}: {file.ReplacementCount} replacement(s)");
                            }
                        }
                    }

                    if (fileResult.FilesWithErrors > 0)
                    {
                        foreach (var file in fileResult.FileResults)
                        {
                            if (!file.Success)
                            {
                                string relativePath = GetRelativePath(rootDirectory, file.FilePath);
                                WriteError($"    {relativePath}: {file.ErrorMessage}");
                            }
                        }
                    }
                }
                else
                {
                    WriteError($"  Failed: {pairResult.Error}");
                }

                Console.WriteLine();
            }

            // Print all errors at the end
            if (result.FailedPairs > 0)
            {
                Console.WriteLine("--- Failed Pairs Summary ---");
                var errors = result.GetAllErrors();
                foreach (var error in errors)
                {
                    WriteError($"  {error}");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Prints a summary for bulk mode operations
        /// </summary>
        /// <param name="result">The bulk replacement result</param>
        /// <param name="isDryRun">Whether this was a dry-run operation</param>
        public void PrintBulkSummary(BulkReplacementResult result, bool isDryRun)
        {
            string operation = isDryRun ? "Bulk dry-run completed" : "Bulk operation completed";

            Console.WriteLine("=== " + operation + " ===");
            Console.WriteLine($"Pairs processed: {result.TotalPairs} total, {result.SuccessfulPairs} successful, {result.FailedPairs} failed");
            Console.WriteLine($"Total files processed: {result.TotalFilesProcessed}");
            Console.WriteLine($"Total replacements: {result.TotalReplacements}");
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
