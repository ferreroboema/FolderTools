using System;
using System.Collections.Generic;
using FolderTools.Models;

namespace FolderTools.Services
{
    /// <summary>
    /// Service for orchestrating bulk file processing operations
    /// </summary>
    public class BulkFileProcessor
    {
        private readonly IFileProcessor _fileProcessor;

        /// <summary>
        /// Creates a new BulkFileProcessor with default dependencies
        /// </summary>
        public BulkFileProcessor() : this(new FileProcessor())
        {
        }

        /// <summary>
        /// Creates a new BulkFileProcessor with the specified file processor
        /// </summary>
        /// <param name="fileProcessor">The file processor to use for each pair</param>
        public BulkFileProcessor(IFileProcessor fileProcessor)
        {
            _fileProcessor = fileProcessor ?? throw new ArgumentNullException(nameof(fileProcessor));
        }

        /// <summary>
        /// Processes multiple search/replace pairs in bulk
        /// </summary>
        /// <param name="pairs">List of search/replace pairs to process</param>
        /// <param name="rootDirectory">Root directory to process</param>
        /// <param name="baseOptions">Base search options to use for all pairs</param>
        /// <param name="filter">File filter to use for all pairs</param>
        /// <returns>Aggregated bulk replacement result</returns>
        public BulkReplacementResult ProcessBulk(
            List<SearchReplacePair> pairs,
            string rootDirectory,
            SearchOptions baseOptions,
            FileFilter filter)
        {
            if (pairs == null || pairs.Count == 0)
            {
                throw new ArgumentException("Pairs list cannot be null or empty", nameof(pairs));
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

            var bulkResult = new BulkReplacementResult();

            foreach (var pair in pairs)
            {
                ReplacementResult result = null;
                string error = null;

                try
                {
                    // Validate the pair
                    if (!pair.IsValid())
                    {
                        error = "Invalid search pattern (empty or null)";
                        bulkResult.AddPairResult(pair, null, error);
                        continue;
                    }

                    // Create options for this pair based on the base options
                    var options = CreateOptionsForPair(baseOptions, pair);

                    // Process the directory with this pair's options
                    result = _fileProcessor.ProcessDirectory(rootDirectory, options, filter);

                    // Add the result
                    bulkResult.AddPairResult(pair, result, null);
                }
                catch (Exception ex)
                {
                    // Continue on error - log and move to next pair
                    error = ex.Message;
                    bulkResult.AddPairResult(pair, result, error);
                }
            }

            return bulkResult;
        }

        /// <summary>
        /// Creates SearchOptions for a specific pair based on base options
        /// </summary>
        /// <param name="baseOptions">Base search options</param>
        /// <param name="pair">Search/replace pair</param>
        /// <returns>Search options for this pair</returns>
        private SearchOptions CreateOptionsForPair(SearchOptions baseOptions, SearchReplacePair pair)
        {
            return new SearchOptions
            {
                Pattern = pair.SearchPattern,
                Replacement = pair.Replacement,
                IsRegex = baseOptions.IsRegex,
                CaseSensitive = baseOptions.CaseSensitive,
                IsDryRun = baseOptions.IsDryRun,
                Encoding = baseOptions.Encoding,
                MaxDepth = baseOptions.MaxDepth,
                Verbose = baseOptions.Verbose,
                Quiet = baseOptions.Quiet,
                IncludeHidden = baseOptions.IncludeHidden
            };
        }
    }
}
