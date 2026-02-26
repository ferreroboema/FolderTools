using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FolderTools.Models;

namespace FolderTools.Utilities
{
    /// <summary>
    /// Parses CSV files containing search/replace pairs for bulk operations
    /// </summary>
    public class CsvSearchReplaceParser
    {
        /// <summary>
        /// Parses a CSV file and returns the list of search/replace pairs
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>List of search/replace pairs</returns>
        /// <exception cref="FileNotFoundException">If the file doesn't exist</exception>
        /// <exception cref="InvalidDataException">If the CSV format is invalid</exception>
        public List<SearchReplacePair> ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }

            var pairs = new List<SearchReplacePair>();
            var lines = File.ReadAllLines(filePath);
            int lineNumber = 0;

            foreach (string rawLine in lines)
            {
                lineNumber++;

                // Trim whitespace from the line
                string line = rawLine.Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Skip comment lines (starting with #)
                if (line.StartsWith("#"))
                {
                    continue;
                }

                // Parse the line as CSV
                SearchReplacePair pair = ParseLine(line, lineNumber);
                if (pair != null && pair.IsValid())
                {
                    pairs.Add(pair);
                }
            }

            return pairs;
        }

        /// <summary>
        /// Parses a single CSV line into a search/replace pair
        /// Supports quoted strings with commas inside them
        /// </summary>
        /// <param name="line">The CSV line to parse</param>
        /// <param name="lineNumber">The line number (for error reporting)</param>
        /// <returns>A search/replace pair, or null if parsing fails</returns>
        private SearchReplacePair ParseLine(string line, int lineNumber)
        {
            try
            {
                var parts = ParseCsvLine(line);

                if (parts.Count < 1)
                {
                    return null;
                }

                // We expect at least a search pattern (replacement can be empty)
                string searchPattern = parts.Count > 0 ? parts[0] : string.Empty;
                string replacement = parts.Count > 1 ? parts[1] : string.Empty;

                return new SearchReplacePair(searchPattern, replacement, lineNumber);
            }
            catch (Exception)
            {
                // Return null for invalid lines - they will be filtered out
                return null;
            }
        }

        /// <summary>
        /// Parses a CSV line, handling quoted strings with commas
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>List of parsed values</returns>
        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Check if it's an escaped quote ("")
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        // Toggle quote mode
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // Comma outside quotes - this is a separator
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            // Add the last value
            values.Add(currentValue.ToString());

            // If we're still in quotes at the end, the line is malformed
            if (inQuotes)
            {
                throw new InvalidDataException("Unclosed quotes in CSV line");
            }

            return values;
        }

        /// <summary>
        /// Validates that all pairs in the list have valid search patterns
        /// </summary>
        /// <param name="pairs">List of search/replace pairs</param>
        /// <returns>True if all pairs are valid, false otherwise</returns>
        public bool ValidatePairs(List<SearchReplacePair> pairs)
        {
            if (pairs == null || pairs.Count == 0)
            {
                return false;
            }

            foreach (var pair in pairs)
            {
                if (!pair.IsValid())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets error messages for invalid pairs
        /// </summary>
        /// <param name="pairs">List of search/replace pairs</param>
        /// <returns>List of error messages</returns>
        public List<string> GetValidationErrors(List<SearchReplacePair> pairs)
        {
            var errors = new List<string>();

            if (pairs == null || pairs.Count == 0)
            {
                errors.Add("No valid search/replace pairs found in CSV file");
                return errors;
            }

            foreach (var pair in pairs)
            {
                if (!pair.IsValid())
                {
                    errors.Add($"Line {pair.LineNumber}: Search pattern is empty or invalid");
                }
            }

            return errors;
        }
    }
}
