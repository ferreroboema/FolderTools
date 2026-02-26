using System;
using System.Collections.Generic;
using System.Linq;

namespace FolderTools.Models
{
    /// <summary>
    /// Result of a bulk replacement operation aggregating multiple search/replace pairs
    /// </summary>
    public class BulkReplacementResult
    {
        /// <summary>
        /// Individual results for each search/replace pair
        /// </summary>
        public List<PairReplacementResult> PairResults { get; private set; }

        /// <summary>
        /// Total number of pairs processed
        /// </summary>
        public int TotalPairs { get; private set; }

        /// <summary>
        /// Number of pairs that were processed successfully
        /// </summary>
        public int SuccessfulPairs { get; private set; }

        /// <summary>
        /// Number of pairs that had errors
        /// </summary>
        public int FailedPairs { get; private set; }

        /// <summary>
        /// Total number of files processed across all pairs
        /// </summary>
        public int TotalFilesProcessed { get; private set; }

        /// <summary>
        /// Total number of replacements made across all pairs
        /// </summary>
        public int TotalReplacements { get; private set; }

        /// <summary>
        /// Creates a new BulkReplacementResult
        /// </summary>
        public BulkReplacementResult()
        {
            PairResults = new List<PairReplacementResult>();
        }

        /// <summary>
        /// Adds a result for a single search/replace pair
        /// </summary>
        /// <param name="pair">The search/replace pair</param>
        /// <param name="result">The replacement result for this pair</param>
        /// <param name="error">Error message if the pair failed to process</param>
        public void AddPairResult(SearchReplacePair pair, ReplacementResult result, string error = null)
        {
            TotalPairs++;

            var pairResult = new PairReplacementResult
            {
                Pair = pair,
                Result = result,
                Error = error,
                Success = string.IsNullOrEmpty(error)
            };

            PairResults.Add(pairResult);

            if (pairResult.Success)
            {
                SuccessfulPairs++;
                if (result != null)
                {
                    TotalFilesProcessed += result.ProcessedFiles;
                    TotalReplacements += result.TotalReplacements;
                }
            }
            else
            {
                FailedPairs++;
            }
        }

        /// <summary>
        /// Gets all errors from failed pairs
        /// </summary>
        /// <returns>List of error messages</returns>
        public List<string> GetAllErrors()
        {
            return PairResults
                .Where(pr => !pr.Success)
                .Select(pr => $"Line {pr.Pair.LineNumber}: {pr.Error}")
                .ToList();
        }

        /// <summary>
        /// Gets a summary of the bulk operation
        /// </summary>
        /// <returns>Summary string</returns>
        public string GetSummary()
        {
            return $"Pairs: {TotalPairs} total, {SuccessfulPairs} successful, {FailedPairs} failed | " +
                   $"Files: {TotalFilesProcessed} | Replacements: {TotalReplacements}";
        }
    }

    /// <summary>
    /// Result for a single search/replace pair in a bulk operation
    /// </summary>
    public class PairReplacementResult
    {
        /// <summary>
        /// The search/replace pair that was processed
        /// </summary>
        public SearchReplacePair Pair { get; set; }

        /// <summary>
        /// The replacement result (null if pair failed before processing)
        /// </summary>
        public ReplacementResult Result { get; set; }

        /// <summary>
        /// Error message if the pair failed to process
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Whether this pair was processed successfully
        /// </summary>
        public bool Success { get; set; }
    }
}
