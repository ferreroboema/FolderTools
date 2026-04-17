using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FolderTools.Models;
using FolderTools.Utilities;

namespace FolderTools.Services
{
    /// <summary>
    /// Implementation of file rename operations
    /// </summary>
    public class FileRenamer : IFileRenamer
    {
        private readonly IFileHelper _fileHelper;
        private readonly RenameCollisionValidator _collisionValidator;

        public FileRenamer() : this(new FileHelperWrapper()) { }

        public FileRenamer(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
            _collisionValidator = new RenameCollisionValidator();
        }

        /// <summary>
        /// Renames files in a directory according to rename options and file filter
        /// </summary>
        public RenameResult RenameFiles(string rootDirectory, RenameOptions renameOptions, FileFilter filter)
        {
            var result = new RenameResult();

            if (!Directory.Exists(rootDirectory))
            {
                result.AddError(rootDirectory, "Directory not found");
                return result;
            }

            try
            {
                // Phase 1: Collect files
                var files = CollectFiles(rootDirectory, renameOptions, filter, result);

                if (files.Count == 0)
                {
                    return result;
                }

                // Phase 2: Build target name mappings
                var renameMap = BuildTargetNames(files, renameOptions);

                // Phase 2b: Check for collisions
                var collisionResult = _collisionValidator.DetectCollisions(renameMap);
                if (collisionResult.HasCollisions)
                {
                    // Record collisions as errors
                    foreach (var collision in collisionResult.Collisions)
                    {
                        foreach (var source in collision.SourceFiles)
                        {
                            result.AddError(source, collision.GetDescription());
                        }
                    }
                    return result;
                }

                // Phase 3: Execute renames
                foreach (var entry in renameMap)
                {
                    if (entry.Value == null) continue;
                    PerformRename(entry.Key, entry.Value, renameOptions, result);
                }
            }
            catch (Exception ex)
            {
                result.AddError(rootDirectory, $"Processing error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Recursively collects files matching the filter criteria
        /// </summary>
        private List<string> CollectFiles(string rootDirectory, RenameOptions options, FileFilter filter, RenameResult result)
        {
            var files = new List<string>();
            CollectFilesRecursive(rootDirectory, options, filter, result, files, 0);
            return files;
        }

        private void CollectFilesRecursive(string currentDirectory, RenameOptions options, FileFilter filter,
            RenameResult result, List<string> files, int currentDepth)
        {
            if (options.MaxDepth.HasValue && currentDepth > options.MaxDepth.Value)
            {
                return;
            }

            try
            {
                var dirFiles = Directory.GetFiles(currentDirectory);

                foreach (string filePath in dirFiles)
                {
                    try
                    {
                        if (!filter.ShouldProcessFile(filePath))
                        {
                            if (options.Verbose)
                            {
                                result.AddSkipped(filePath, "Filtered out");
                            }
                            continue;
                        }

                        // Check if file is locked
                        if (_fileHelper.IsFileLocked(filePath))
                        {
                            result.AddSkipped(filePath, "File locked");
                            continue;
                        }

                        files.Add(filePath);
                    }
                    catch (Exception ex)
                    {
                        result.AddError(filePath, ex.Message);
                    }
                }

                // Recurse into subdirectories
                var subdirectories = Directory.GetDirectories(currentDirectory);
                foreach (string subdirectory in subdirectories)
                {
                    if (!options.IncludeHidden)
                    {
                        var dirInfo = new DirectoryInfo(subdirectory);
                        if ((dirInfo.Attributes & FileAttributes.Hidden) != 0 ||
                            (dirInfo.Attributes & FileAttributes.System) != 0)
                        {
                            continue;
                        }
                    }

                    CollectFilesRecursive(subdirectory, options, filter, result, files, currentDepth + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (options.Verbose)
                {
                    result.AddSkipped(currentDirectory, "Access denied");
                }
            }
            catch (Exception ex)
            {
                if (options.Verbose)
                {
                    result.AddError(currentDirectory, $"Error scanning directory: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Builds a mapping of original paths to new paths based on the rename sub-mode
        /// </summary>
        private Dictionary<string, string> BuildTargetNames(List<string> files, RenameOptions options)
        {
            if (options.IsPrefixSuffixMode)
            {
                return BuildPrefixSuffixNames(files, options);
            }
            else
            {
                return BuildFindReplaceNames(files, options);
            }
        }

        /// <summary>
        /// Builds target names using find/replace pattern on filenames
        /// </summary>
        private Dictionary<string, string> BuildFindReplaceNames(List<string> files, RenameOptions options)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string dir = Path.GetDirectoryName(filePath);
                string newFileName;

                if (options.IsRegex)
                {
                    var regexOptions = options.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    newFileName = Regex.Replace(fileName, options.Pattern, options.Replacement ?? "", regexOptions);
                }
                else if (options.CaseSensitive)
                {
                    newFileName = fileName.Replace(options.Pattern, options.Replacement ?? "");
                }
                else
                {
                    // Case-insensitive literal replacement for .NET Framework
                    newFileName = Regex.Replace(
                        fileName,
                        Regex.Escape(options.Pattern),
                        options.Replacement ?? "",
                        RegexOptions.IgnoreCase);
                }

                // Skip if no change
                if (string.Equals(fileName, newFileName, StringComparison.Ordinal))
                {
                    continue;
                }

                string newPath = Path.Combine(dir, newFileName);
                map[filePath] = newPath;
            }

            return map;
        }

        /// <summary>
        /// Builds target names using prefix/suffix and sequential numbering
        /// </summary>
        private Dictionary<string, string> BuildPrefixSuffixNames(List<string> files, RenameOptions options)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Sort files according to the specified order
            var sortedFiles = SortFiles(files, options.SortOrder);

            int number = options.StartNumber;
            foreach (string filePath in sortedFiles)
            {
                string extension = Path.GetExtension(filePath);
                string prefix = options.Prefix ?? "";
                string suffix = options.Suffix ?? "";

                string numberStr = options.Padding > 0
                    ? number.ToString().PadLeft(options.Padding, '0')
                    : number.ToString();

                string newFileName = $"{prefix}{numberStr}{suffix}{extension}";
                string dir = Path.GetDirectoryName(filePath);
                string newPath = Path.Combine(dir, newFileName);

                map[filePath] = newPath;
                number++;
            }

            return map;
        }

        /// <summary>
        /// Sorts files by the specified order
        /// </summary>
        private List<string> SortFiles(List<string> files, RenameSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case RenameSortOrder.Date:
                    return files.OrderBy(f =>
                    {
                        try { return File.GetLastWriteTime(f); }
                        catch { return DateTime.MaxValue; }
                    }).ToList();

                case RenameSortOrder.Size:
                    return files.OrderBy(f =>
                    {
                        try { return new FileInfo(f).Length; }
                        catch { return 0L; }
                    }).ToList();

                case RenameSortOrder.Name:
                default:
                    return files.OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase).ToList();
            }
        }

        /// <summary>
        /// Performs the actual file rename, handling case-only renames on Windows
        /// </summary>
        private void PerformRename(string originalPath, string newPath, RenameOptions options, RenameResult result)
        {
            try
            {
                if (options.IsDryRun)
                {
                    result.AddResult(originalPath, newPath);
                    return;
                }

                string originalFullPath = Path.GetFullPath(originalPath);
                string newFullPath = Path.GetFullPath(newPath);

                // Check if it's a case-only rename on Windows
                if (string.Equals(originalFullPath, newFullPath, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(originalFullPath, newFullPath, StringComparison.Ordinal))
                {
                    // Two-step rename via temp file to handle case-only changes on Windows
                    string tempPath = Path.Combine(
                        Path.GetDirectoryName(originalPath),
                        $"__folderools_temp_{Guid.NewGuid():N}{Path.GetExtension(originalPath)}");

                    File.Move(originalPath, tempPath);
                    File.Move(tempPath, newPath);
                }
                else
                {
                    File.Move(originalPath, newPath);
                }

                result.AddResult(originalPath, newPath);
            }
            catch (PathTooLongException)
            {
                result.AddError(originalPath, "Path too long");
            }
            catch (Exception ex)
            {
                result.AddError(originalPath, ex.Message);
            }
        }
    }
}
