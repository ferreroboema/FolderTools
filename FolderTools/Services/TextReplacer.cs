using System;
using System.Text;
using System.Text.RegularExpressions;
using FolderTools.Models;
using FolderTools.Utilities;

namespace FolderTools.Services
{
    /// <summary>
    /// Implementation of text replacement operations
    /// </summary>
    public class TextReplacer : ITextReplacer
    {
        private readonly IFileHelper _fileHelper;

        public TextReplacer() : this(new Utilities.FileHelperWrapper())
        {
        }

        public TextReplacer(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper ?? new Utilities.FileHelperWrapper();
        }

        /// <summary>
        /// Performs find and replace operations on a file
        /// </summary>
        /// <param name="filePath">Path to the file to process</param>
        /// <param name="options">Search options including pattern and replacement</param>
        /// <returns>Number of replacements made, or -1 if an error occurred</returns>
        public int ReplaceInFile(string filePath, SearchOptions options)
        {
            try
            {
                // Validate pattern
                if (string.IsNullOrEmpty(options.Pattern))
                {
                    if (options.Verbose)
                    {
                        Console.WriteLine("  Error: Pattern cannot be empty");
                    }
                    return -1;
                }

                // Read the file content
                if (!_fileHelper.TryReadFile(filePath, options.Encoding, out string content, out string error))
                {
                    if (options.Verbose)
                    {
                        Console.WriteLine($"  Skipped: {error}");
                    }
                    return -1;
                }

                string originalContent = content;
                int replacementCount = 0;

                if (options.IsRegex)
                {
                    // Use regex replacement
                    var regexOptions = options.CaseSensitive
                        ? RegexOptions.None
                        : RegexOptions.IgnoreCase;

                    replacementCount = RegexCount(content, options.Pattern, regexOptions);

                    if (replacementCount > 0 && !options.IsDryRun)
                    {
                        try
                        {
                            content = Regex.Replace(content, options.Pattern, options.Replacement, regexOptions);
                        }
                        catch (ArgumentException ex)
                        {
                            if (options.Verbose)
                            {
                                Console.WriteLine($"  Error: Invalid regex pattern - {ex.Message}");
                            }
                            return -1;
                        }
                    }
                }
                else
                {
                    // Use literal string replacement
                    var comparison = options.CaseSensitive
                        ? StringComparison.Ordinal
                        : StringComparison.OrdinalIgnoreCase;

                    replacementCount = CountOccurrences(content, options.Pattern, comparison);

                    if (replacementCount > 0 && !options.IsDryRun)
                    {
                        content = ReplaceIgnoreCase(content, options.Pattern, options.Replacement, comparison);
                    }
                }

                // Write the modified content back (unless dry run)
                if (!options.IsDryRun && replacementCount > 0)
                {
                    if (!_fileHelper.TryWriteFile(filePath, content, options.Encoding, out string writeError))
                    {
                        if (options.Verbose)
                        {
                            Console.WriteLine($"  Error: Could not write file - {writeError}");
                        }
                        return -1;
                    }
                }

                return replacementCount;
            }
            catch (Exception ex)
            {
                if (options.Verbose)
                {
                    Console.WriteLine($"  Error: {ex.Message}");
                }
                return -1;
            }
        }

        /// <summary>
        /// Counts the number of times a regex pattern matches in the content
        /// </summary>
        private int RegexCount(string content, string pattern, RegexOptions options)
        {
            try
            {
                return Regex.Matches(content, pattern, options).Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Counts the number of times a substring appears in the content
        /// </summary>
        private int CountOccurrences(string content, string pattern, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(pattern))
                return 0;

            int count = 0;
            int index = 0;

            while ((index = content.IndexOf(pattern, index, comparison)) != -1)
            {
                count++;
                index += pattern.Length;
            }

            return count;
        }

        /// <summary>
        /// Performs a case-insensitive string replacement while preserving the original case of the matched text
        /// or replacing with the exact replacement string provided
        /// </summary>
        private string ReplaceIgnoreCase(string content, string pattern, string replacement, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(pattern))
                return content;

            // For case-insensitive replacement, we need to do it manually
            // to replace all occurrences regardless of case
            if (comparison == StringComparison.OrdinalIgnoreCase)
            {
                return ReplaceAllIgnoreCase(content, pattern, replacement);
            }

            // For case-sensitive, use standard Replace
            return content.Replace(pattern, replacement);
        }

        /// <summary>
        /// Replaces all occurrences of a pattern with replacement, ignoring case.
        /// Uses manual IndexOf loop to ensure the replacement string is treated as a literal
        /// (regex-based approach would interpret $1, $2 etc. as backreferences).
        /// </summary>
        private string ReplaceAllIgnoreCase(string content, string pattern, string replacement)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(pattern))
                return content;

            var sb = new StringBuilder(content.Length);
            int index = 0;
            int patternLength = pattern.Length;

            while (index < content.Length)
            {
                int matchIndex = content.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase);
                if (matchIndex == -1)
                {
                    sb.Append(content, index, content.Length - index);
                    break;
                }

                sb.Append(content, index, matchIndex - index);
                sb.Append(replacement);
                index = matchIndex + patternLength;
            }

            return sb.ToString();
        }
    }
}
