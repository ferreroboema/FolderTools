using System;
using System.IO;
using FolderTools.Models;
using FolderTools.Utilities;

namespace FolderTools.Services
{
    /// <summary>
    /// Implementation of file processing operations
    /// </summary>
    public class FileProcessor : IFileProcessor
    {
        private readonly ITextReplacer _textReplacer;

        public FileProcessor() : this(new TextReplacer())
        {
        }

        public FileProcessor(ITextReplacer textReplacer)
        {
            _textReplacer = textReplacer ?? throw new ArgumentNullException(nameof(textReplacer));
        }

        /// <summary>
        /// Processes all files in a directory matching the given filter criteria
        /// </summary>
        /// <param name="rootDirectory">Root directory to start processing from</param>
        /// <param name="options">Search and replace options</param>
        /// <param name="filter">File filtering criteria</param>
        /// <returns>Aggregated results from all processed files</returns>
        public ReplacementResult ProcessDirectory(string rootDirectory, SearchOptions options, FileFilter filter)
        {
            var result = new ReplacementResult();

            if (!Directory.Exists(rootDirectory))
            {
                result.AddError(rootDirectory, "Directory not found");
                return result;
            }

            try
            {
                // Process the directory recursively
                ProcessDirectoryRecursive(rootDirectory, options, filter, result, 0);
            }
            catch (Exception ex)
            {
                result.AddError(rootDirectory, $"Processing error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Recursively processes a directory and its subdirectories
        /// </summary>
        private void ProcessDirectoryRecursive(string currentDirectory, SearchOptions options, FileFilter filter,
            ReplacementResult result, int currentDepth)
        {
            // Check max depth
            if (options.MaxDepth.HasValue && currentDepth > options.MaxDepth.Value)
            {
                return;
            }

            try
            {
                // Get all files in the current directory
                var files = Directory.GetFiles(currentDirectory);

                foreach (string filePath in files)
                {
                    ProcessFile(filePath, options, filter, result);
                }

                // Recursively process subdirectories
                var subdirectories = Directory.GetDirectories(currentDirectory);

                foreach (string subdirectory in subdirectories)
                {
                    // Check if we should process this directory (hidden check)
                    if (!options.IncludeHidden)
                    {
                        var dirInfo = new DirectoryInfo(subdirectory);
                        if ((dirInfo.Attributes & FileAttributes.Hidden) != 0 ||
                            (dirInfo.Attributes & FileAttributes.System) != 0)
                        {
                            continue;
                        }
                    }

                    ProcessDirectoryRecursive(subdirectory, options, filter, result, currentDepth + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we don't have access to
                if (options.Verbose)
                {
                    Console.WriteLine($"  Skipping directory (access denied): {currentDirectory}");
                }
            }
            catch (Exception ex)
            {
                if (options.Verbose)
                {
                    Console.WriteLine($"  Error processing directory: {currentDirectory} - {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Processes a single file
        /// </summary>
        private void ProcessFile(string filePath, SearchOptions options, FileFilter filter, ReplacementResult result)
        {
            try
            {
                // Check if file should be processed based on filter
                if (!filter.ShouldProcessFile(filePath))
                {
                    if (options.Verbose)
                    {
                        Console.WriteLine($"  Filtered out: {filePath}");
                    }
                    return;
                }

                // Check if file is text file
                if (!FileHelper.IsTextFile(filePath))
                {
                    result.AddSkipped(filePath, "Binary file");
                    if (options.Verbose)
                    {
                        Console.WriteLine($"  Skipped (binary): {filePath}");
                    }
                    return;
                }

                // Check if file is locked
                if (FileHelper.IsFileLocked(filePath))
                {
                    result.AddSkipped(filePath, "File locked");
                    if (options.Verbose)
                    {
                        Console.WriteLine($"  Skipped (locked): {filePath}");
                    }
                    return;
                }

                // Perform the replacement
                int replacementCount = _textReplacer.ReplaceInFile(filePath, options);

                if (replacementCount >= 0)
                {
                    result.AddResult(filePath, replacementCount);

                    if (options.Verbose)
                    {
                        string action = options.IsDryRun ? "Would replace" : "Replaced";
                        Console.WriteLine($"{action}: {replacementCount} occurrence(s) in {filePath}");
                    }
                }
                else
                {
                    result.AddError(filePath, "Processing failed");
                }
            }
            catch (Exception ex)
            {
                result.AddError(filePath, ex.Message);
                if (options.Verbose)
                {
                    Console.WriteLine($"  Error: {filePath} - {ex.Message}");
                }
            }
        }
    }
}
