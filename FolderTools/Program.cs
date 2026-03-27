using System;
using System.Collections.Generic;
using System.Reflection;
using FolderTools.Models;
using FolderTools.Services;
using FolderTools.Utilities;
using FolderTools.Outputs;
using System.IO;

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

                if (error == "VERSION")
                {
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    Console.WriteLine($"FolderTools v{version}");
                    return 0;
                }

                Console.Error.WriteLine($"Error: {error}");
                Console.WriteLine();
                CommandLineParser.PrintHelp();
                return 1;
            }

            // Get the root directory
            string rootDirectory = parser.GetRootDirectory();

            // If using current directory (default), prompt for confirmation
            if (rootDirectory == ".")
            {
                string currentDir = System.IO.Directory.GetCurrentDirectory();
                Console.WriteLine($"No directory specified. Current directory: {currentDir}");
                if (!PromptYesNo("Proceed with current directory?"))
                {
                    Console.WriteLine("Operation cancelled by user.");
                    return 0;
                }
            }

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

            // Route to appropriate processing mode
            if (parser.IsBulkMode)
            {
                return ProcessBulkMode(parser, rootDirectory, options, filter);
            }
            else
            {
                return ProcessStandardMode(rootDirectory, options, filter);
            }
        }

        /// <summary>
        /// Processes a single search/replace operation (standard mode)
        /// </summary>
        private static int ProcessStandardMode(string rootDirectory, SearchOptions options, FileFilter filter)
        {
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

        /// <summary>
        /// Processes multiple search/replace operations from a CSV file (bulk mode)
        /// </summary>
        private static int ProcessBulkMode(CommandLineParser parser, string rootDirectory, SearchOptions options, FileFilter filter)
        {
            // Create the result formatter
            var formatter = new ResultFormatter();

            // Parse the CSV file
            var csvParser = new CsvSearchReplaceParser();
            List<SearchReplacePair> pairs;

            try
            {
                pairs = csvParser.ParseFile(parser.BulkFilePath);

                if (pairs.Count == 0)
                {
                    formatter.PrintError("No valid search/replace pairs found in CSV file");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                formatter.PrintError($"Error reading CSV file: {ex.Message}");
                return 1;
            }

            // Detect collisions before processing
            var collisionValidator = new BulkCollisionValidator();
            var collisionResult = collisionValidator.DetectCollisions(pairs);

            if (collisionResult.HasCollisions)
            {
                bool shouldContinue = false;

                switch (options.CollisionBehavior)
                {
                    case CollisionBehavior.Fail:
                        formatter.PrintError($"Collisions detected: {collisionResult.GetSummaryMessage()}");
                        formatter.PrintCollisionWarnings(collisionResult);
                        return 1;

                    case CollisionBehavior.Ignore:
                        shouldContinue = true;
                        break;

                    case CollisionBehavior.Warn:
                        formatter.PrintWarning($"Collisions detected: {collisionResult.GetSummaryMessage()}");
                        formatter.PrintCollisionWarnings(collisionResult);
                        shouldContinue = true;
                        break;

                    case CollisionBehavior.Prompt:
                    default:
                        formatter.PrintWarning($"Collisions detected: {collisionResult.GetSummaryMessage()}");
                        formatter.PrintCollisionWarnings(collisionResult);
                        shouldContinue = PromptYesNo("Do you want to continue?");
                        break;
                }

                if (!shouldContinue)
                {
                    formatter.PrintInfo("Operation cancelled by user.");
                    return 0;
                }
            }

            // Print bulk header
            if (!options.Quiet)
            {
                formatter.PrintBulkHeader(parser.BulkFilePath, rootDirectory, pairs.Count, options.IsDryRun);
            }

            // Create the bulk processor and process all pairs
            var bulkProcessor = new BulkFileProcessor();
            BulkReplacementResult bulkResult;

            try
            {
                bulkResult = bulkProcessor.ProcessBulk(pairs, rootDirectory, options, filter);
            }
            catch (Exception ex)
            {
                formatter.PrintError($"Unexpected error during bulk processing: {ex.Message}");
                return 1;
            }

            // Print detailed results
            if (!options.Quiet)
            {
                formatter.PrintBulkResults(bulkResult, rootDirectory, options.Verbose);
            }

            // Print summary
            if (!options.Quiet)
            {
                formatter.PrintBulkSummary(bulkResult, options.IsDryRun);
            }

            // Return exit code based on whether there were any failed pairs
            return bulkResult.FailedPairs > 0 ? 1 : 0;
        }

        /// <summary>
        /// Prompts the user with a yes/no question, defaulting to yes
        /// </summary>
        /// <param name="message">The message to display before the prompt</param>
        /// <returns>True if user wants to proceed (Y/yes/Enter), false otherwise</returns>
        private static bool PromptYesNo(string message)
        {
            Console.Write($"{message} (Y/n): ");
            string response = Console.ReadLine()?.Trim().ToLower();
            return string.IsNullOrEmpty(response) || response == "y" || response == "yes";
        }
    }
}
