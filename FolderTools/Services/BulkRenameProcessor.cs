using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FolderTools.Models;
using FolderTools.Utilities;

namespace FolderTools.Services
{
    /// <summary>
    /// Service for orchestrating bulk file rename operations from CSV mappings
    /// </summary>
    public class BulkRenameProcessor
    {
        private readonly IFileRenamer _fileRenamer;

        /// <summary>
        /// Creates a new BulkRenameProcessor with default dependencies
        /// </summary>
        public BulkRenameProcessor() : this(new FileRenamer()) { }

        /// <summary>
        /// Creates a new BulkRenameProcessor with the specified file renamer
        /// </summary>
        public BulkRenameProcessor(IFileRenamer fileRenamer)
        {
            _fileRenamer = fileRenamer ?? throw new ArgumentNullException(nameof(fileRenamer));
        }

        /// <summary>
        /// Processes rename mappings from a CSV file
        /// Each mapping is applied as a find/replace pass over filenames in the directory
        /// </summary>
        /// <param name="mappings">List of rename mappings to process</param>
        /// <param name="rootDirectory">Root directory to process</param>
        /// <param name="baseOptions">Base rename options</param>
        /// <param name="filter">File filter to use</param>
        /// <returns>Aggregated rename result</returns>
        public RenameResult ProcessBulkRename(
            List<RenameMapping> mappings,
            string rootDirectory,
            RenameOptions baseOptions,
            FileFilter filter)
        {
            if (mappings == null || mappings.Count == 0)
            {
                throw new ArgumentException("Mappings list cannot be null or empty", nameof(mappings));
            }

            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentException("Root directory cannot be null or empty", nameof(rootDirectory));
            }

            if (baseOptions == null)
            {
                throw new ArgumentNullException(nameof(baseOptions));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var bulkResult = new RenameResult();

            foreach (var mapping in mappings)
            {
                if (!mapping.IsValid())
                {
                    bulkResult.AddError(mapping.OldName ?? "(null)",
                        $"Line {mapping.LineNumber}: Invalid mapping - {GetInvalidReason(mapping)}");
                    continue;
                }

                try
                {
                    // Create options for this mapping as a find/replace on filenames
                    var options = CreateOptionsForMapping(baseOptions, mapping);

                    var result = _fileRenamer.RenameFiles(rootDirectory, options, filter);

                    // Merge results
                    foreach (var fileResult in result.FileResults)
                    {
                        if (fileResult.Success)
                        {
                            bulkResult.AddResult(fileResult.OriginalPath, fileResult.NewPath);
                        }
                        else
                        {
                            bulkResult.AddError(fileResult.OriginalPath,
                                $"Line {mapping.LineNumber}: {fileResult.ErrorMessage}");
                        }
                    }

                    foreach (var skipped in result.SkippedFiles)
                    {
                        bulkResult.AddSkipped(skipped.FilePath, skipped.Reason);
                    }
                }
                catch (Exception ex)
                {
                    bulkResult.AddError(mapping.OldName,
                        $"Line {mapping.LineNumber}: {ex.Message}");
                }
            }

            return bulkResult;
        }

        /// <summary>
        /// Creates RenameOptions for a specific mapping based on base options
        /// </summary>
        private RenameOptions CreateOptionsForMapping(RenameOptions baseOptions, RenameMapping mapping)
        {
            return new RenameOptions
            {
                // Use the mapping's old/new names as find/replace pattern on filenames
                Pattern = mapping.OldName,
                Replacement = mapping.NewName,
                IsRegex = false, // CSV mappings are literal filename matches
                CaseSensitive = false,
                IsDryRun = baseOptions.IsDryRun,
                Verbose = baseOptions.Verbose,
                Quiet = baseOptions.Quiet,
                IncludeHidden = baseOptions.IncludeHidden,
                MaxDepth = baseOptions.MaxDepth
            };
        }

        /// <summary>
        /// Gets the reason a mapping is invalid
        /// </summary>
        private string GetInvalidReason(RenameMapping mapping)
        {
            if (string.IsNullOrWhiteSpace(mapping.OldName))
                return "Old filename is empty";
            if (string.IsNullOrWhiteSpace(mapping.NewName))
                return "New filename is empty";
            if (string.Equals(mapping.OldName, mapping.NewName, StringComparison.OrdinalIgnoreCase))
                return "Old and new filenames are the same";
            return "Unknown validation error";
        }
    }
}
