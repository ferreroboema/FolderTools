using System;
using FolderTools.Models;
using FolderTools.Services;
using FolderTools.Utilities;
using FolderTools.Outputs;

namespace FolderTools
{
    /// <summary>
    /// Main entry point for the FolderTools CLI application
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            // Create the command-line parser
            var parser = new CommandLineParser(args);

            // Parse arguments
            if (!parser.ParseArguments(out SearchOptions options, out FileFilter filter, out string error))
            {
                if (error == "HELP")
                {
                    CommandLineParser.PrintHelp();
                    return 0;
                }

                Console.Error.WriteLine($"Error: {error}");
                Console.WriteLine();
                CommandLineParser.PrintHelp();
                return 1;
            }

            // Get the root directory
            string rootDirectory = parser.GetRootDirectory();

            // Normalize the directory path
            try
            {
                rootDirectory = System.IO.Path.GetFullPath(rootDirectory);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: Invalid directory path - {ex.Message}");
                return 1;
            }

            // Validate that pattern is not empty
            if (string.IsNullOrWhiteSpace(options.Pattern))
            {
                Console.Error.WriteLine("Error: Search pattern cannot be empty");
                return 1;
            }

            // Create the result formatter
            var formatter = new ResultFormatter();

            // Print header
            if (!options.Quiet)
            {
                formatter.PrintHeader(rootDirectory, options.Pattern, options.Replacement, options.IsDryRun);
            }

            // Create the file processor and process the directory
            var processor = new FileProcessor();
            ReplacementResult result = null;

            try
            {
                result = processor.ProcessDirectory(rootDirectory, options, filter);
            }
            catch (Exception ex)
            {
                formatter.PrintError($"Unexpected error during processing: {ex.Message}");
                return 1;
            }

            // Print detailed results
            if (!options.Quiet)
            {
                formatter.PrintFileResults(result, rootDirectory, options.Verbose);
            }

            // Print summary
            if (!options.Quiet)
            {
                formatter.PrintSummary(result, options.IsDryRun);
            }

            // Return exit code based on whether there were errors
            return result.FilesWithErrors > 0 ? 1 : 0;
        }
    }
}
