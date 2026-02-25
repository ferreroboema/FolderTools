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

        public CommandLineParser(string[] args)
        {
            _args = new List<string>(args);
            _currentIndex = 0;
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

            try
            {
                // Parse positional arguments first
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
                            break;

                        case "-f":
                        case "--filename":
                            if (!HasNextArg())
                            {
                                error = $"Missing value for {arg}";
                                return false;
                            }
                            filter.FileNamePattern = GetNextArg();
                            break;

                        case "--min-size":
                            if (!HasNextArg() || !long.TryParse(GetNextArg(), out long minSize))
                            {
                                error = $"Invalid value for {arg}";
                                return false;
                            }
                            filter.MinSize = minSize;
                            break;

                        case "--max-size":
                            if (!HasNextArg() || !long.TryParse(GetNextArg(), out long maxSize))
                            {
                                error = $"Invalid value for {arg}";
                                return false;
                            }
                            filter.MaxSize = maxSize;
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
        /// Gets the root directory from the parsed arguments
        /// </summary>
        /// <returns>Root directory path</returns>
        public string GetRootDirectory()
        {
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

        /// <summary>
        /// Displays help information for the command-line tool
        /// </summary>
        public static void PrintHelp()
        {
            Console.WriteLine("FolderTools - CLI Find and Replace Tool");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  FolderTools.exe <search-pattern> <replace-pattern> <directory> [options]");
            Console.WriteLine();
            Console.WriteLine("Required arguments:");
            Console.WriteLine("  <search-pattern>    Text or regex pattern to search for");
            Console.WriteLine("  <replace-pattern>   Text to replace matches with (use \"\" for empty)");
            Console.WriteLine("  <directory>         Starting directory to search in");
            Console.WriteLine();
            Console.WriteLine("Options:");
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
            Console.WriteLine("  -v, --verbose                    Verbose output");
            Console.WriteLine("  -q, --quiet                      Quiet mode (minimal output)");
            Console.WriteLine("  -h, --help                       Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  FolderTools.exe \"old\" \"new\" \"C:\\MyFiles\" -e \".txt,.cs\"");
            Console.WriteLine("  FolderTools.exe \"foo\" \"bar\" \".\" -r -d");
            Console.WriteLine("  FolderTools.exe \"\\d+\" \"NUM\" \".\" -r -e \".txt\" --max-depth 2");
        }
    }
}
