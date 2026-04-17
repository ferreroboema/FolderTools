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

        /// <summary>
        /// Indicates whether rename mode is enabled
        /// </summary>
        public bool IsRenameMode { get; private set; }

        /// <summary>
        /// Rename-specific options (populated when --rename is used)
        /// </summary>
        public RenameOptions RenameOptions { get; private set; }

        public CommandLineParser(string[] args)
        {
            _args = new List<string>(args);
            _currentIndex = 0;
            IsBulkMode = false;
            BulkFilePath = null;
            IsRenameMode = false;
            RenameOptions = null;
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
                // Check for help/version flags first (before validating argument count)
                if (_args.Count > 0)
                {
                    string firstArg = _args[0].ToLower();
                    if (firstArg == "-h" || firstArg == "--help")
                    {
                        error = "HELP";
                        return false;
                    }
                    if (firstArg == "-V" || firstArg == "--version")
                    {
                        error = "VERSION";
                        return false;
                    }
                }

                // Check for rename mode flag
                for (int i = 0; i < _args.Count; i++)
                {
                    if (_args[i].ToLower() == "--rename")
                    {
                        return ParseRenameModeArguments(i, out options, out filter, out error);
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
                if (_args.Count < 2)
                {
                    error = "Insufficient arguments. Required: <search-pattern> <replace-pattern> [<directory>]";
                    return false;
                }

                options.Pattern = _args[0];
                options.Replacement = _args[1];

                // Check if directory is provided (third argument must not start with "-" to be a directory)
                string directory;
                if (_args.Count >= 3 && !_args[2].StartsWith("-"))
                {
                    directory = _args[2];
                    _currentIndex = 3;
                }
                else
                {
                    directory = ".";  // Default to current directory
                    _currentIndex = 2;
                }

                // Only validate if directory is not "." (current directory always exists)
                if (directory != "." && !System.IO.Directory.Exists(directory))
                {
                    error = $"Directory not found: {directory}";
                    return false;
                }

                // Store root directory for consistent retrieval via GetRootDirectory()
                _rootDirectory = directory;

                // Store root directory in options for later use
                options.Verbose = false;
                filter.IncludeHidden = false;

                // Parse optional arguments using shared method
                while (_currentIndex < _args.Count)
                {
                    int beforeIndex = _currentIndex;
                    if (!ParseOptionalArgument(_args[_currentIndex], options, filter, out error))
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
                    directory = ".";  // Default to current directory
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
        /// Parses command-line arguments for rename mode
        /// </summary>
        private bool ParseRenameModeArguments(int renameFlagIndex, out SearchOptions options, out FileFilter filter, out string error)
        {
            options = new SearchOptions();
            filter = new FileFilter();
            error = null;
            IsRenameMode = true;
            RenameOptions = new RenameOptions();

            try
            {
                // Find the directory and detect sub-mode
                string directory = null;

                // Collect all non-flag, non-value args to find directory and patterns
                var positionalArgs = new List<string>();
                var skipIndices = new HashSet<int>();
                skipIndices.Add(renameFlagIndex);

                // Pre-scan to find rename-file, prefix, suffix and their values
                for (int i = 0; i < _args.Count; i++)
                {
                    string argLower = _args[i].ToLower();
                    if (argLower == "--rename-file" || argLower == "--prefix" ||
                        argLower == "--suffix" || argLower == "--start-number" ||
                        argLower == "--padding" || argLower == "--sort")
                    {
                        skipIndices.Add(i);
                        if (i + 1 < _args.Count) skipIndices.Add(i + 1);
                    }
                }

                for (int i = 0; i < _args.Count; i++)
                {
                    if (skipIndices.Contains(i)) continue;
                    string arg = _args[i];
                    if (arg.ToLower() == "--rename") continue;
                    if (arg.StartsWith("-")) continue;

                    // Try as directory
                    if (System.IO.Directory.Exists(arg))
                    {
                        directory = arg;
                    }
                    else
                    {
                        positionalArgs.Add(arg);
                    }
                }

                // If we have 2+ positional args, treat first two as search/replace pattern
                if (positionalArgs.Count >= 2)
                {
                    RenameOptions.Pattern = positionalArgs[0];
                    RenameOptions.Replacement = positionalArgs[1];
                }
                else if (positionalArgs.Count == 1)
                {
                    RenameOptions.Pattern = positionalArgs[0];
                }

                if (string.IsNullOrEmpty(directory))
                {
                    directory = ".";
                }

                _rootDirectory = directory;

                // Parse all optional arguments
                _currentIndex = 0;
                while (_currentIndex < _args.Count)
                {
                    string arg = _args[_currentIndex];

                    // Skip --rename flag itself
                    if (arg.ToLower() == "--rename")
                    {
                        _currentIndex++;
                        continue;
                    }

                    // Skip directory argument
                    if (arg == directory)
                    {
                        _currentIndex++;
                        continue;
                    }

                    // Skip positional pattern args (if they were consumed as search/replace)
                    if (positionalArgs.Contains(arg) && !arg.StartsWith("-"))
                    {
                        _currentIndex++;
                        continue;
                    }

                    int beforeIndex = _currentIndex;
                    if (!ParseOptionalArgument(arg, options, filter, out error))
                    {
                        return false;
                    }

                    if (_currentIndex == beforeIndex)
                    {
                        _currentIndex++;
                    }
                }

                // Copy shared options from SearchOptions to RenameOptions
                RenameOptions.IsRegex = options.IsRegex;
                RenameOptions.CaseSensitive = options.CaseSensitive;
                RenameOptions.IsDryRun = options.IsDryRun;
                RenameOptions.Verbose = options.Verbose;
                RenameOptions.Quiet = options.Quiet;
                RenameOptions.IncludeHidden = options.IncludeHidden;
                RenameOptions.MaxDepth = options.MaxDepth;

                return true;
            }
            catch (Exception ex)
            {
                error = $"Error parsing rename mode arguments: {ex.Message}";
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

                // Rename mode flags (only valid when --rename is active)
                case "--rename-file":
                    if (!IsRenameMode)
                    {
                        error = $"Unknown argument: {arg}";
                        return false;
                    }
                    if (!HasNextArg())
                    {
                        error = $"Missing value for {arg}";
                        return false;
                    }
                    RenameOptions.RenameFilePath = GetNextArg();
                    _currentIndex++;
                    break;

                case "--prefix":
                    if (!IsRenameMode)
                    {
                        error = $"Unknown argument: {arg}";
                        return false;
                    }
                    if (!HasNextArg())
                    {
                        error = $"Missing value for {arg}";
                        return false;
                    }
                    RenameOptions.Prefix = GetNextArg();
                    _currentIndex++;
                    break;

                case "--suffix":
                    if (!IsRenameMode)
                    {
                        error = $"Unknown argument: {arg}";
                        return false;
                    }
                    if (!HasNextArg())
                    {
                        error = $"Missing value for {arg}";
                        return false;
                    }
                    RenameOptions.Suffix = GetNextArg();
                    _currentIndex++;
                    break;

                case "--start-number":
                    if (!IsRenameMode)
                    {
                        error = $"Unknown argument: {arg}";
                        return false;
                    }
                    if (!HasNextArg() || !int.TryParse(GetNextArg(), out int startNum) || startNum < 0)
                    {
                        error = $"Invalid value for {arg}. Must be a non-negative integer.";
                        return false;
                    }
                    RenameOptions.StartNumber = startNum;
                    _currentIndex++;
                    break;

                case "--padding":
                    if (!IsRenameMode)
                    {
                        error = $"Unknown argument: {arg}";
                        return false;
                    }
                    if (!HasNextArg() || !int.TryParse(GetNextArg(), out int padding) || padding < 0)
                    {
                        error = $"Invalid value for {arg}. Must be a non-negative integer.";
                        return false;
                    }
                    RenameOptions.Padding = padding;
                    _currentIndex++;
                    break;

                case "--sort":
                    if (!IsRenameMode)
                    {
                        error = $"Unknown argument: {arg}";
                        return false;
                    }
                    if (!HasNextArg() || !ParseSortOrder(GetNextArg(), out RenameSortOrder sortOrder))
                    {
                        error = $"Invalid value for {arg}. Valid values: name, date, size";
                        return false;
                    }
                    RenameOptions.SortOrder = sortOrder;
                    _currentIndex++;
                    break;

                case "-V":
                case "--version":
                    error = "VERSION";
                    return false;

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
            return _rootDirectory ?? ".";
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

        private bool ParseSortOrder(string value, out RenameSortOrder sortOrder)
        {
            switch (value.ToLower())
            {
                case "name":
                    sortOrder = RenameSortOrder.Name;
                    return true;
                case "date":
                    sortOrder = RenameSortOrder.Date;
                    return true;
                case "size":
                    sortOrder = RenameSortOrder.Size;
                    return true;
                default:
                    sortOrder = RenameSortOrder.Name;
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
            Console.WriteLine("  Standard mode: FolderTools.exe <search-pattern> <replace-pattern> [<directory>] [options]");
            Console.WriteLine("  Bulk mode:     FolderTools.exe --bulk-file <csv> [<directory>] [options]");
            Console.WriteLine("  Rename mode:   FolderTools.exe --rename <search> <replace> [<directory>] [options]");
            Console.WriteLine("                 FolderTools.exe --rename --rename-file <csv> [<directory>] [options]");
            Console.WriteLine("                 FolderTools.exe --rename --prefix <text> [--suffix <text>] [<directory>] [options]");
            Console.WriteLine("                  (If directory is omitted, current directory is used after prompt)");
            Console.WriteLine();
            Console.WriteLine("Standard mode - Required arguments:");
            Console.WriteLine("  <search-pattern>    Text or regex pattern to search for");
            Console.WriteLine("  <replace-pattern>   Text to replace matches with (use \"\" for empty)");
            Console.WriteLine("  <directory>         Starting directory to search in (optional, defaults to current)");
            Console.WriteLine();
            Console.WriteLine("Bulk mode - Required arguments:");
            Console.WriteLine("  -b, --bulk-file <csv>            CSV file with search/replace pairs (format: search,replace)");
            Console.WriteLine("  <directory>                       Starting directory to search in (optional, defaults to current)");
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
            Console.WriteLine("  -V, --version                    Show version information");
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
            Console.WriteLine("Rename mode - Rename files in a directory:");
            Console.WriteLine("  --rename                         Enable rename mode (operates on filenames)");
            Console.WriteLine("  --rename-file <csv>              CSV file with old_name,new_name pairs");
            Console.WriteLine("  --prefix <text>                  Add prefix before filename (before extension)");
            Console.WriteLine("  --suffix <text>                  Add suffix before the file extension");
            Console.WriteLine("  --start-number <n>               Starting number for numbering (default: 1)");
            Console.WriteLine("  --padding <n>                    Zero-pad width for number (e.g., 3 = \"001\")");
            Console.WriteLine("  --sort <name|date|size>          Sort order for numbering (default: name)");
            Console.WriteLine();
            Console.WriteLine("Rename mode examples:");
            Console.WriteLine("  FolderTools.exe --rename \"old\" \"new\" \"C:\\MyFiles\" -e \".txt\" -d");
            Console.WriteLine("  FolderTools.exe --rename \"\\d+\" \"NUM\" \"C:\\Data\" -r -c -e \".log\"");
            Console.WriteLine("  FolderTools.exe --rename --rename-file renames.csv \"C:\\MyFiles\" -d");
            Console.WriteLine("  FolderTools.exe --rename --prefix \"photo_\" --padding 3 \"C:\\Photos\" -e \".jpg\"");
            Console.WriteLine("  FolderTools.exe --rename --suffix \"_backup\" \"C:\\Data\" --sort date");
            Console.WriteLine();
            Console.WriteLine("CSV file format:");
            Console.WriteLine("  # Comment lines start with #");
            Console.WriteLine("  old,new");
            Console.WriteLine("  \"pattern, with, commas\",replacement");
            Console.WriteLine("  TODO, (empty replacement)");
        }
    }
}
