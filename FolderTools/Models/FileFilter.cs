using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FolderTools.Models
{
    /// <summary>
    /// Criteria for filtering files during processing
    /// </summary>
    public class FileFilter
    {
        /// <summary>
        /// File extensions to include (e.g., ".txt", ".cs"). Empty = all extensions.
        /// </summary>
        public HashSet<string> Extensions { get; set; }

        /// <summary>
        /// Filename wildcard pattern (e.g., "*config*", "Test?.txt")
        /// </summary>
        public string FileNamePattern { get; set; }

        /// <summary>
        /// Minimum file size in bytes (null = no minimum)
        /// </summary>
        public long? MinSize { get; set; }

        /// <summary>
        /// Maximum file size in bytes (null = no maximum)
        /// </summary>
        public long? MaxSize { get; set; }

        /// <summary>
        /// Include hidden and system files
        /// </summary>
        public bool IncludeHidden { get; set; }

        public FileFilter()
        {
            Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether a file should be processed based on the filter criteria
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <returns>True if the file should be processed, false otherwise</returns>
        public bool ShouldProcessFile(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                // Check hidden/system files
                if (!IncludeHidden)
                {
                    if ((fileInfo.Attributes & FileAttributes.Hidden) != 0 ||
                        (fileInfo.Attributes & FileAttributes.System) != 0)
                    {
                        return false;
                    }
                }

                // Check file extension
                if (Extensions.Count > 0)
                {
                    string extension = fileInfo.Extension;
                    if (string.IsNullOrEmpty(extension) || !Extensions.Contains(extension))
                    {
                        return false;
                    }
                }

                // Check filename pattern
                if (!string.IsNullOrEmpty(FileNamePattern))
                {
                    if (!MatchesWildcard(FileNamePattern, fileInfo.Name))
                    {
                        return false;
                    }
                }

                // Check file size
                if (MinSize.HasValue && fileInfo.Length < MinSize.Value)
                {
                    return false;
                }

                if (MaxSize.HasValue && fileInfo.Length > MaxSize.Value)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a comma-separated list of file extensions to the filter
        /// </summary>
        /// <param name="extensions">Comma-separated extensions (e.g., ".txt,.cs,.json")</param>
        public void AddExtensions(string extensions)
        {
            if (string.IsNullOrWhiteSpace(extensions))
                return;

            string[] parts = extensions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ext in parts)
            {
                string trimmedExt = ext.Trim();
                if (!string.IsNullOrEmpty(trimmedExt))
                {
                    // Ensure extension starts with a dot
                    if (!trimmedExt.StartsWith("."))
                    {
                        trimmedExt = "." + trimmedExt;
                    }
                    Extensions.Add(trimmedExt);
                }
            }
        }

        /// <summary>
        /// Matches a wildcard pattern against a filename
        /// </summary>
        private static bool MatchesWildcard(string pattern, string text)
        {
            try
            {
                // Convert wildcard pattern to regex pattern
                string regexPattern = "^" + Regex.Escape(pattern)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".") + "$";
                return Regex.IsMatch(text, regexPattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
