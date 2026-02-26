using System;
using System.Reflection;
using FluentAssertions;
using FolderTools.Models;
using FolderTools.Utilities;
using System.IO;
using Xunit;

namespace FolderTools.Tests.Utilities
{
    public class CommandLineParserTests
    {
        // Use the current directory which should be the test bin directory
        private static readonly string TestDirectory = Environment.CurrentDirectory;

        [Fact]
        public void ParseArguments_WithValidArguments_ShouldParseCorrectly()
        {
            var args = new[] { "search", "replace", TestDirectory, "-e", ".txt,.cs", "-c", "-r" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.Pattern.Should().Be("search");
            options.Replacement.Should().Be("replace");
            options.CaseSensitive.Should().BeTrue();
            options.IsRegex.Should().BeTrue();
            filter.Extensions.Should().Contain(new[] { ".txt", ".cs" });
        }

        [Fact]
        public void ParseArguments_WithInsufficientArguments_ShouldReturnError()
        {
            var args = new[] { "search", "replace" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeFalse();
            error.Should().Contain("Insufficient arguments");
        }

        [Fact]
        public void ParseArguments_WithHelpFlag_ShouldReturnHelp()
        {
            var args = new[] { "--help" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeFalse();
            error.Should().Be("HELP");
        }

        [Fact]
        public void ParseArguments_WithShortHelpFlag_ShouldReturnHelp()
        {
            var args = new[] { "-h" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeFalse();
            error.Should().Be("HELP");
        }

        [Fact]
        public void ParseArguments_WithDryRun_ShouldSetIsDryRun()
        {
            var args = new[] { "search", "replace", TestDirectory, "-d" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.IsDryRun.Should().BeTrue();
        }

        [Fact]
        public void ParseArguments_WithLongDryRunFlag_ShouldSetIsDryRun()
        {
            var args = new[] { "search", "replace", TestDirectory, "--dry-run" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.IsDryRun.Should().BeTrue();
        }

        [Fact]
        public void ParseArguments_WithVerbose_ShouldSetVerbose()
        {
            var args = new[] { "search", "replace", TestDirectory, "-v" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.Verbose.Should().BeTrue();
        }

        [Fact]
        public void ParseArguments_WithQuiet_ShouldSetQuiet()
        {
            var args = new[] { "search", "replace", TestDirectory, "-q" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.Quiet.Should().BeTrue();
        }

        [Fact]
        public void ParseArguments_WithFileNamePattern_ShouldSetPattern()
        {
            var args = new[] { "search", "replace", TestDirectory, "-f", "*config*" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            filter.FileNamePattern.Should().Be("*config*");
        }

        [Fact]
        public void ParseArguments_WithMinSize_ShouldSetMinSize()
        {
            var args = new[] { "search", "replace", TestDirectory, "--min-size", "1000" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            filter.MinSize.Should().Be(1000);
        }

        [Fact]
        public void ParseArguments_WithMaxSize_ShouldSetMaxSize()
        {
            var args = new[] { "search", "replace", TestDirectory, "--max-size", "10000" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            filter.MaxSize.Should().Be(10000);
        }

        [Fact]
        public void ParseArguments_WithEncoding_ShouldSetEncoding()
        {
            var args = new[] { "search", "replace", TestDirectory, "--encoding", "utf8" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.Encoding.Should().Be(FileEncoding.Utf8);
        }

        [Theory]
        [InlineData("auto", FileEncoding.Auto)]
        [InlineData("utf8", FileEncoding.Utf8)]
        [InlineData("utf-8", FileEncoding.Utf8)]
        [InlineData("ascii", FileEncoding.Ascii)]
        [InlineData("unicode", FileEncoding.Unicode)]
        public void ParseArguments_WithDifferentEncodings_ShouldParseCorrectly(string encodingValue, FileEncoding expectedEncoding)
        {
            var args = new[] { "search", "replace", TestDirectory, "--encoding", encodingValue };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.Encoding.Should().Be(expectedEncoding);
        }

        [Fact]
        public void ParseArguments_WithInvalidEncoding_ShouldReturnError()
        {
            var args = new[] { "search", "replace", TestDirectory, "--encoding", "invalid" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeFalse();
            error.Should().Contain("Invalid encoding value");
        }

        [Fact]
        public void ParseArguments_WithMaxDepth_ShouldSetMaxDepth()
        {
            var args = new[] { "search", "replace", TestDirectory, "--max-depth", "5" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.MaxDepth.Should().Be(5);
        }

        [Fact]
        public void ParseArguments_WithIncludeHidden_ShouldSetIncludeHidden()
        {
            var args = new[] { "search", "replace", TestDirectory, "--include-hidden" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeTrue();
            options.IncludeHidden.Should().BeTrue();
            filter.IncludeHidden.Should().BeTrue();
        }

        [Fact]
        public void ParseArguments_WithUnknownArgument_ShouldReturnError()
        {
            var args = new[] { "search", "replace", TestDirectory, "--unknown-arg" };
            var parser = new CommandLineParser(args);
            var result = parser.ParseArguments(out var options, out var filter, out var error);

            result.Should().BeFalse();
            error.Should().Contain("Unknown argument");
        }

        [Fact]
        public void GetRootDirectory_ShouldReturnCorrectDirectory()
        {
            var args = new[] { "search", "replace", "C:\\TestDir" };
            var parser = new CommandLineParser(args);
            var rootDir = parser.GetRootDirectory();

            rootDir.Should().Be("C:\\TestDir");
        }

        [Fact]
        public void GetRootDirectory_WithInsufficientArgs_ShouldReturnNull()
        {
            var args = new[] { "search" };
            var parser = new CommandLineParser(args);
            var rootDir = parser.GetRootDirectory();

            rootDir.Should().BeNull();
        }
    }
}
