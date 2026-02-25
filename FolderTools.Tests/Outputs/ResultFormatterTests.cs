using System;
using System.IO;
using FluentAssertions;
using FolderTools.Models;
using FolderTools.Outputs;
using Xunit;

namespace FolderTools.Tests.Outputs
{
    public class ResultFormatterTests : IDisposable
    {
        private readonly StringWriter _consoleOutput;
        private readonly TextWriter _originalOutput;

        public ResultFormatterTests()
        {
            _originalOutput = Console.Out;
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _consoleOutput.Dispose();
        }

        [Fact]
        public void PrintSummary_ShouldDisplayCorrectInformation()
        {
            var formatter = new ResultFormatter(useColors: false);
            var result = new ReplacementResult();
            result.AddResult("test.txt", 5);
            result.AddError("error.txt", "Access denied");
            result.AddSkipped("binary.bin", "Binary file");

            formatter.PrintSummary(result, isDryRun: false);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Files processed: 1");
            output.Should().Contain("Total replacements: 5");
            output.Should().Contain("Files with errors: 1");
            output.Should().Contain("Files skipped: 1");
            output.Should().Contain("Operation completed");
        }

        [Fact]
        public void PrintSummary_DryRun_ShouldDisplayDryRun()
        {
            var formatter = new ResultFormatter(useColors: false);
            var result = new ReplacementResult();
            result.AddResult("test.txt", 3);

            formatter.PrintSummary(result, isDryRun: true);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Dry-run completed");
        }

        [Fact]
        public void PrintHeader_ShouldDisplayOperationDetails()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.PrintHeader("C:\\test", "pattern", "replacement", isDryRun: true);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Directory: C:\\test");
            output.Should().Contain("Search pattern: pattern");
            output.Should().Contain("Replacement: replacement");
            output.Should().Contain("Dry-run");
        }

        [Fact]
        public void PrintHeader_LiveMode_ShouldDisplayLiveMode()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.PrintHeader("C:\\test", "pattern", "replacement", isDryRun: false);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Live");
            output.Should().Contain("files will be modified");
        }

        [Fact]
        public void PrintError_ShouldDisplayErrorMessage()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.PrintError("Test error message");

            var output = _consoleOutput.ToString();
            output.Should().Contain("ERROR: Test error message");
        }

        [Fact]
        public void PrintWarning_ShouldDisplayWarningMessage()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.PrintWarning("Test warning message");

            var output = _consoleOutput.ToString();
            output.Should().Contain("WARNING: Test warning message");
        }

        [Fact]
        public void PrintInfo_ShouldDisplayInfoMessage()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.PrintInfo("Test info message");

            var output = _consoleOutput.ToString();
            output.Should().Contain("INFO: Test info message");
        }

        [Fact]
        public void PrintFileResults_Verbose_ShouldDisplayAllResults()
        {
            var formatter = new ResultFormatter(useColors: false);
            var result = new ReplacementResult();
            result.AddResult("success.txt", 5);
            result.AddError("error.txt", "Access denied");
            result.AddSkipped("skip.bin", "Binary file");

            formatter.PrintFileResults(result, "C:\\test", verbose: true);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Detailed Results");
            output.Should().Contain("Errors");
            output.Should().Contain("Skipped Files");
        }

        [Fact]
        public void PrintFileResults_NotVerbose_NoErrorsOrSkipped_ShouldNotDisplayResults()
        {
            var formatter = new ResultFormatter(useColors: false);
            var result = new ReplacementResult();
            result.AddResult("success.txt", 5);

            formatter.PrintFileResults(result, "C:\\test", verbose: false);

            var output = _consoleOutput.ToString();
            output.Should().NotContain("Detailed Results");
        }

        [Fact]
        public void PrintFileResults_WithErrors_ShouldDisplayErrors()
        {
            var formatter = new ResultFormatter(useColors: false);
            var result = new ReplacementResult();
            result.AddError("error.txt", "Access denied");

            formatter.PrintFileResults(result, "C:\\test", verbose: false);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Errors");
            output.Should().Contain("Access denied");
        }

        [Fact]
        public void PrintHeader_WithEmptyPattern_ShouldDisplayEmpty()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.PrintHeader("C:\\test", "", "replacement", isDryRun: false);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Search pattern: (empty)");
        }

        [Fact]
        public void PrintHeader_WithEmptyReplacement_ShouldDisplayEmpty()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.PrintHeader("C:\\test", "pattern", "", isDryRun: false);

            var output = _consoleOutput.ToString();
            output.Should().Contain("Replacement: (empty)");
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            var formatter = new ResultFormatter();
            formatter.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithColorsDisabled_ShouldDisableColors()
        {
            var formatter = new ResultFormatter(useColors: false);
            formatter.Should().NotBeNull();
        }
    }
}
