using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FolderTools.Models;

namespace FolderTools.Utilities
{
    /// <summary>
    /// Parses CSV files containing old_name,new_name pairs for rename operations
    /// </summary>
    public class CsvRenameParser
    {
        /// <summary>
        /// Parses a CSV file and returns the list of rename mappings
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>List of rename mappings</returns>
        /// <exception cref="FileNotFoundException">If the file doesn't exist</exception>
        public List<RenameMapping> ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }

            var mappings = new List<RenameMapping>();
            var lines = File.ReadAllLines(filePath);
            int lineNumber = 0;

            foreach (string rawLine in lines)
            {
                lineNumber++;

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
                RenameMapping mapping = ParseLine(line, lineNumber);
                if (mapping != null && mapping.IsValid())
                {
                    mappings.Add(mapping);
                }
            }

            return mappings;
        }

        /// <summary>
        /// Parses a single CSV line into a rename mapping
        /// </summary>
        private RenameMapping ParseLine(string line, int lineNumber)
        {
            try
            {
                var parts = ParseCsvLine(line);

                if (parts.Count < 2)
                {
                    return null;
                }

                string oldName = parts[0];
                string newName = parts[1];

                return new RenameMapping(oldName, newName, lineNumber);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a CSV line, handling quoted strings with commas or semicolons
        /// Auto-detects delimiter: semicolon (;) takes precedence over comma (,)
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            char delimiter = line.Contains(";") ? ';' : ',';

            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString());

            if (inQuotes)
            {
                throw new InvalidDataException("Unclosed quotes in CSV line");
            }

            return values;
        }

        /// <summary>
        /// Validates that all mappings in the list are valid
        /// </summary>
        public bool ValidateMappings(List<RenameMapping> mappings)
        {
            if (mappings == null || mappings.Count == 0)
            {
                return false;
            }

            foreach (var mapping in mappings)
            {
                if (!mapping.IsValid())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets error messages for invalid mappings
        /// </summary>
        public List<string> GetValidationErrors(List<RenameMapping> mappings)
        {
            var errors = new List<string>();

            if (mappings == null || mappings.Count == 0)
            {
                errors.Add("No valid rename mappings found in CSV file");
                return errors;
            }

            foreach (var mapping in mappings)
            {
                if (!mapping.IsValid())
                {
                    if (string.IsNullOrWhiteSpace(mapping.OldName))
                    {
                        errors.Add($"Line {mapping.LineNumber}: Old filename is empty");
                    }
                    else if (string.IsNullOrWhiteSpace(mapping.NewName))
                    {
                        errors.Add($"Line {mapping.LineNumber}: New filename is empty");
                    }
                    else if (string.Equals(mapping.OldName, mapping.NewName, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"Line {mapping.LineNumber}: Old and new filenames are the same");
                    }
                }
            }

            return errors;
        }
    }
}
