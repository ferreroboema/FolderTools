using System;
using System.Collections.Generic;
using FolderTools.Models;

namespace FolderTools.Utilities
{
    /// <summary>
    /// Parses command-line arguments into SearchOptions and FileFilter objects
    /// </summary>
    public class CommandLineParser
    {
        private readonly List<string> _args;
        private int _currentIndex;

        /// <summary>
        /// Indicates whether bulk mode is enabled
        /// </summary>
        public bool IsBulkMode { get; private set; }

        /// <summary>
        /// Path to the CSV file containing search/replace pairs (in bulk mode)
        /// </summary>
        public string BulkFilePath { get; private set; }

        public CommandLineParser(string[] args)
        {
            _args = new List<string>(args);
            _currentIndex = 0;
            IsBulkMode = false;
            BulkFilePath = null;
        }

        /// <summary>
        /// Parses the command-line arguments and returns the search options and file filter
        /// </summary>
        /// <param name="options">Output: parsed search options</param>
        /// <param name="filter">Output: parsed file filter</param>
        /// <param name="error">Output: error message if parsing fails</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public bool ParseArguments(out SearchOptions options, out FileFilter filter, out string error)
        {
            options = new SearchOptions();
            filter = new FileFilter();
            error = null;
            IsBulkMode = false;
            BulkFilePath = null;

            try
            {
                // Check for help flag first (before validating argument count)
                if (_args.Count > 0)
                {
                    string firstArg = _args[0].ToLower();
                    if (firstArg == "-h" || firstArg == "--help")
                    {
                        error = "HELP";
                        return false;
                    }
                }

                // Check for bulk mode flag
                bool bulkModeFound = false;
                int bulkFileIndex = -1;
                for (int i = 0; i < _args.Count; i++)
                {
                    if (_args[i].ToLower() == "-b" || _args[i].ToLower() == "--bulk-file")
                    {
                        bulkModeFound = true;
                        bulkFileIndex = i;
                        break;
                    }
                }

                if (bulkModeFound)
                {
                    return ParseBulkModeArguments(bulkFileIndex, out options, out filter, out error);
                }

                // Parse standard mode positional arguments
                if (_args.Count < 3)
                {
                    error = "Insufficient arguments. Required: <search-pattern> <replace-pattern> <directory>";
                    return false;
                }

                options.Pattern = _args[0];
                options.Replacement = _args[1];
                string directory = _args[2];
                _currentIndex = 3;

                // Validate directory exists
                if (!System.IO.Directory.Exists(directory))
                {
                    error = $"Directory not found: {directory}";
                    return false;
                }

                // Store root directory in options for later use
                options.Verbose = false;
                filter.IncludeHidden = false;

                // Parse optional arguments
                while (_currentIndex < _args.Count)
                {
                    string arg = _args[_currentIndex];

                    switch (arg.ToLower())
                    {
                        case "-e":
                        case "--extensions":
                            if (!HasNextArg())
                            {
                                error = $"Missing value for {arg}";
                                return false;
                            }
                            filter.AddExtensions(GetNextArg());
                            _currentIndex++;
                            break;

                        case "-f":
                        case "--filename":
                            if (!HasNextArg())
                            {
                                error = $"Missing value for {arg}";
                                return false;
                            }
                            filter.FileNamePattern = GetNextArg();
                            _currentIndex++;
                            break;

                        case "--min-size":
                            if (!HasNextArg() || !long.TryParse(GetNextArg(), out long minSize))
                            {
                                error = $"Invalid value for {arg}";
                                return false;
                            }
                            filter.MinSize = minSize;
                            _currentIndex++;
                            break;

                        case "--max-size":
                            if (!HasNextArg() || !long.TryParse(GetNextArg(), out long maxSize))
                            {
                                error = $"Invalid value for {arg}";
                                return false;
                            }
                            filter.MaxSize = maxSize;
                            _currentIndex++;
                            break;

                        case "-c":
                        case "--case-sensitive":
                            options.CaseSensitive = true;
                            _currentIndex++;
                            break;

                        case "-r":
                        case "--regex":
                            options.IsRegex = true;
                            _currentIndex++;
                            break;

                        case "-d":
                        case "--dry-run":
                            options.IsDryRun = true;
                            _currentIndex++;
                            break;

                        case "--encoding":
                            if (!HasNextArg() || !ParseEncoding(GetNextArg(), out FileEncoding encoding))
                            {
                                error = $"Invalid encoding value. Valid values: auto, utf8, ascii, unicode";
                                return false;
                            }
                            options.Encoding = encoding;
                            _currentIndex++;
                            break;

                        case "--include-hidden":
                            options.IncludeHidden = true;
                            filter.IncludeHidden = true;
                            _currentIndex++;
                            break;

                        case "--max-depth":
                            if (!HasNextArg() || !int.TryParse(GetNextArg(), out int maxDepth) || maxDepth < 0)
                            {
                                error = $"Invalid value for {arg}. Must be a non-negative integer.";
                                return false;
                            }
                            options.MaxDepth = maxDepth;
                            _currentIndex++;
                            break;

                        case "-v":
                        case "--verbose":
                            options.Verbose = true;
                            _currentIndex++;
                            break;

                        case "-q":
                        case "--quiet":
                            options.Quiet = true;
                            _currentIndex++;
                            break;

                        case "--collision-behavior":
                            if (!HasNextArg() || !ParseCollisionBehavior(GetNextArg(), out CollisionBehavior behavior))
                            {
                                error = $"Invalid value for {arg}. Valid values: prompt, warn, fail, ignore";
                                return false;
                            }
                            options.CollisionBehavior = behavior;
                            _currentIndex++;
                            break;

                        case "-h":
                        case "--help":
                            error = "HELP";
                            return false;

                        default:
                            error = $"Unknown argument: {arg}";
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Error parsing arguments: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Parses command-line arguments for bulk mode
        /// </summary>
        /// <param name="bulkFileIndex">Index of the --bulk-file flag</param>
        /// <param name="options">Output: parsed search options</param>
        /// <param name="filter">Output: parsed file filter</param>
        /// <param name="error">Output: error message if parsing fails</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        private bool ParseBulkModeArguments(int bulkFileIndex, out SearchOptions options, out FileFilter filter, out string error)
        {
            options = new SearchOptions();
            filter = new FileFilter();
            error = null;

            try
            {
                // Get the CSV file path
                if (bulkFileIndex + 1 >= _args.Count)
                {
                    error = "Missing value for --bulk-file argument";
                    return false;
                }

                BulkFilePath = _args[bulkFileIndex + 1];

                // Validate CSV file exists
                if (!System.IO.File.Exists(BulkFilePath))
                {
                    error = $"CSV file not found: {BulkFilePath}";
                    return false;
                }

                IsBulkMode = true;

                // For bulk mode, the directory is the first positional argument (or after --bulk-file)
                // Expected format: --bulk-file <csv> <directory> [options]
                // OR: <directory> --bulk-file <csv> [options]

                string directory = null;

                // Try to find directory argument (should be before or after --bulk-file, but not part of flags)
                if (bulkFileIndex >= 1)
                {
                    // Directory might be before --bulk-file
                    string potentialDir = _args[bulkFileIndex - 1];
                    if (!potentialDir.StartsWith("-") && System.IO.Directory.Exists(potentialDir))
                    {
                        directory = potentialDir;
                    }
                }

                if (directory == null && bulkFileIndex + 2 < _args.Count)
                {
                    // Directory might be after --bulk-file <csv>
                    string potentialDir = _args[bulkFileIndex + 2];
                    if (!potentialDir.StartsWith("-") && System.IO.Directory.Exists(potentialDir))
                    {
                        directory = potentialDir;
                    }
                }

                // If still not found, search all args for a valid directory
                if (directory == null)
                {
                    for (int i = 0; i < _args.Count; i++)
                    {
                        if (i != bulkFileIndex && i != bulkFileIndex + 1 && !_args[i].StartsWith("-"))
                        {
                            if (System.IO.Directory.Exists(_args[i]))
                            {
                                directory = _args[i];
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(directory))
                {
                    error = "Directory not found or not specified. In bulk mode, provide: --bulk-file <csv> <directory>";
                    return false;
                }

                // Store root directory for retrieval
                _rootDirectory = directory;

                // Initialize options with defaults
                options.Verbose = false;
                filter.IncludeHidden = false;

                // Parse optional arguments (skip --bulk-file and its value, and the directory)
                _currentIndex = 0;
                while (_currentIndex < _args.Count)
                {
                    string arg = _args[_currentIndex];

                    // Skip the bulk file flag and its value
                    if (arg.ToLower() == "-b" || arg.ToLower() == "--bulk-file")
                    {
                        _currentIndex += 2;
                        continue;
                    }

                    // Skip the directory argument
                    if (arg == directory)
                    {
                        _currentIndex++;
                        continue;
                    }

                    int beforeIndex = _currentIndex;
                    if (!ParseOptionalArgument(arg, options, filter, out error))
                    {
                        return false;
                    }

                    // Only increment if ParseOptionalArgument didn't increment
                    // (it increments for arguments with values)
                    if (_currentIndex == beforeIndex)
                    {
                        _currentIndex++;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Error parsing bulk mode arguments: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Parses a single optional argument
        /// </summary>
        private bool ParseOptionalArgument(string arg, SearchOptions options, FileFilter filter, out string error)
        {
            error = null;

            switch (arg.ToLower())
            {
                case "-e":
                case "--extensions":
                    if (!HasNextArg())
                    {
                        error = $"Missing value for {arg}";
                        return false;
                    }
                    filter.AddExtensions(GetNextArg());
                    _currentIndex++;
                    break;

                case "-f":
                case "--filename":
                    if (!HasNextArg())
                    {
                        error = $"Missing value for {arg}";
                        return false;
                    }
                    filter.FileNamePattern = GetNextArg();
                    _currentIndex++;
                    break;

                case "--min-size":
                    if (!HasNextArg() || !long.TryParse(GetNextArg(), out long minSize))
                    {
                        error = $"Invalid value for {arg}";
                        return false;
                    }
                    filter.MinSize = minSize;
                    _currentIndex++;
                    break;

                case "--max-size":
                    if (!HasNextArg() || !long.TryParse(GetNextArg(), out long maxSize))
                    {
                        error = $"Invalid value for {arg}";
                        return false;
                    }
                    filter.MaxSize = maxSize;
                    _currentIndex++;
                    break;

                case "-c":
                case "--case-sensitive":
                    options.CaseSensitive = true;
                    break;

                case "-r":
                case "--regex":
                    options.IsRegex = true;
                    break;

                case "-d":
                case "--dry-run":
                    options.IsDryRun = true;
                    break;

                case "--encoding":
                    if (!HasNextArg() || !ParseEncoding(GetNextArg(), out FileEncoding encoding))
                    {
                        error = $"Invalid encoding value. Valid values: auto, utf8, ascii, unicode";
                        return false;
                    }
                    options.Encoding = encoding;
                    _currentIndex++;
                    break;

                case "--include-hidden":
                    options.IncludeHidden = true;
                    filter.IncludeHidden = true;
                    break;

                case "--max-depth":
                    if (!HasNextArg() || !int.TryParse(GetNextArg(), out int maxDepth) || maxDepth < 0)
                    {
                        error = $"Invalid value for {arg}. Must be a non-negative integer.";
                        return false;
                    }
                    options.MaxDepth = maxDepth;
                    _currentIndex++;
                    break;

                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;

                case "-q":
                case "--quiet":
                    options.Quiet = true;
                    break;

                case "--collision-behavior":
                    if (!HasNextArg() || !ParseCollisionBehavior(GetNextArg(), out CollisionBehavior behavior))
                    {
                        error = $"Invalid value for {arg}. Valid values: prompt, warn, fail, ignore";
                        return false;
                    }
                    options.CollisionBehavior = behavior;
                    _currentIndex++;
                    break;

                case "-h":
                case "--help":
                    error = "HELP";
                    return false;

                default:
                    error = $"Unknown argument: {arg}";
                    return false;
            }

            return true;
        }

        private string _rootDirectory;

        /// <summary>
        /// Gets the root directory from the parsed arguments
        /// </summary>
        /// <returns>Root directory path</returns>
        public string GetRootDirectory()
        {
            if (IsBulkMode)
            {
                return _rootDirectory;
            }

            if (_args.Count >= 3)
            {
                return _args[2];
            }
            return null;
        }

        private bool HasNextArg()
        {
            return _currentIndex + 1 < _args.Count;
        }

        private string GetNextArg()
        {
            _currentIndex++;
            return _args[_currentIndex];
        }

        private bool ParseEncoding(string value, out FileEncoding encoding)
        {
            switch (value.ToLower())
            {
                case "auto":
                    encoding = FileEncoding.Auto;
                    return true;
                case "utf8":
                case "utf-8":
                    encoding = FileEncoding.Utf8;
                    return true;
                case "ascii":
                    encoding = FileEncoding.Ascii;
                    return true;
                case "unicode":
                    encoding = FileEncoding.Unicode;
                    return true;
                default:
                    encoding = FileEncoding.Auto;
                    return false;
            }
        }

        private bool ParseCollisionBehavior(string value, out CollisionBehavior behavior)
        {
            switch (value.ToLower())
            {
                case "prompt":
                    behavior = CollisionBehavior.Prompt;
                    return true;
                case "warn":
                    behavior = CollisionBehavior.Warn;
                    return true;
                case "fail":
                    behavior = CollisionBehavior.Fail;
                    return true;
                case "ignore":
                    behavior = CollisionBehavior.Ignore;
                    return true;
                default:
                    behavior = CollisionBehavior.Prompt;
                    return false;
            }
        }

        /// <summary>
        /// Displays help information for the command-line tool
        /// </summary>
        public static void PrintHelp()
        {
            Console.WriteLine("FolderTools - CLI Find and Replace Tool");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Standard mode: FolderTools.exe <search-pattern> <replace-pattern> <directory> [options]");
            Console.WriteLine("  Bulk mode:     FolderTools.exe --bulk-file <csv> <directory> [options]");
            Console.WriteLine();
            Console.WriteLine("Standard mode - Required arguments:");
            Console.WriteLine("  <search-pattern>    Text or regex pattern to search for");
            Console.WriteLine("  <replace-pattern>   Text to replace matches with (use \"\" for empty)");
            Console.WriteLine("  <directory>         Starting directory to search in");
            Console.WriteLine();
            Console.WriteLine("Bulk mode - Required arguments:");
            Console.WriteLine("  -b, --bulk-file <csv>            CSV file with search/replace pairs (format: search,replace)");
            Console.WriteLine("  <directory>                       Starting directory to search in");
            Console.WriteLine();
            Console.WriteLine("Options (both modes):");
            Console.WriteLine("  -e, --extensions <ext1,ext2>    File extensions to process (e.g., \".txt,.cs\")");
            Console.WriteLine("  -f, --filename <pattern>         Filename wildcard pattern (e.g., \"*config*\")");
            Console.WriteLine("  --min-size <bytes>               Minimum file size");
            Console.WriteLine("  --max-size <bytes>               Maximum file size");
            Console.WriteLine("  -c, --case-sensitive             Case sensitive matching (default: false)");
            Console.WriteLine("  -r, --regex                      Use regex pattern matching (default: literal)");
            Console.WriteLine("  -d, --dry-run                    Preview changes without modifying files");
            Console.WriteLine("  --encoding <utf8|ascii|auto>     Text encoding (default: auto)");
            Console.WriteLine("  --include-hidden                 Include hidden/system files");
            Console.WriteLine("  --max-depth <number>             Maximum recursion depth");
            Console.WriteLine("  --collision-behavior <mode>      How to handle bulk collisions (default: prompt)");
            Console.WriteLine("                                   Modes: prompt, warn, fail, ignore");
            Console.WriteLine("  -v, --verbose                    Verbose output");
            Console.WriteLine("  -q, --quiet                      Quiet mode (minimal output)");
            Console.WriteLine("  -h, --help                       Show this help message");
            Console.WriteLine();
            Console.WriteLine("Standard mode examples:");
            Console.WriteLine("  FolderTools.exe \"old\" \"new\" \"C:\\MyFiles\" -e \".txt,.cs\"");
            Console.WriteLine("  FolderTools.exe \"foo\" \"bar\" \".\" -r -d");
            Console.WriteLine("  FolderTools.exe \"\\d+\" \"NUM\" \".\" -r -e \".txt\" --max-depth 2");
            Console.WriteLine();
            Console.WriteLine("Bulk mode examples:");
            Console.WriteLine("  FolderTools.exe --bulk-file replacements.csv \"C:\\MyFolder\" -e \".txt,.cs\" -d");
            Console.WriteLine("  FolderTools.exe -b pairs.csv \"C:\\Project\" -e \".cs,.js,.json\" -c -v");
            Console.WriteLine();
            Console.WriteLine("CSV file format:");
            Console.WriteLine("  # Comment lines start with #");
            Console.WriteLine("  old,new");
            Console.WriteLine("  \"pattern, with, commas\",replacement");
            Console.WriteLine("  TODO, (empty replacement)");
        }
    }
}
