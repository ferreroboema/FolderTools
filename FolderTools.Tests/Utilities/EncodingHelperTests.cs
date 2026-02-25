using System;
using System.IO;
using System.Text;
using FluentAssertions;
using FolderTools.Models;
using FolderTools.Utilities;
using Xunit;

namespace FolderTools.Tests.Utilities
{
    public class EncodingHelperTests
    {
        [Fact]
        public void GetEncoding_WithUtf8_ShouldReturnUtf8Encoding()
        {
            var encoding = EncodingHelper.GetEncoding(FileEncoding.Utf8);
            encoding.Should().BeOfType<UTF8Encoding>();
            ((UTF8Encoding)encoding).GetPreamble().Should().BeEmpty();
        }

        [Fact]
        public void GetEncoding_WithAscii_ShouldReturnAsciiEncoding()
        {
            var encoding = EncodingHelper.GetEncoding(FileEncoding.Ascii);
            encoding.Should().Be(Encoding.ASCII);
        }

        [Fact]
        public void GetEncoding_WithUnicode_ShouldReturnUnicodeEncoding()
        {
            var encoding = EncodingHelper.GetEncoding(FileEncoding.Unicode);
            encoding.Should().Be(Encoding.Unicode);
        }

        [Fact]
        public void GetEncoding_WithAuto_ShouldReturnUtf8Encoding()
        {
            var encoding = EncodingHelper.GetEncoding(FileEncoding.Auto);
            encoding.Should().BeOfType<UTF8Encoding>();
        }

        [Fact]
        public void ReadFile_WithAutoEncoding_ShouldDetectEncoding()
        {
            var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Files", "ascii.txt");

            if (File.Exists(testFile))
            {
                var content = EncodingHelper.ReadFile(testFile, FileEncoding.Auto);
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void ReadFile_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            Action act = () => EncodingHelper.ReadFile("nonexistent.txt", FileEncoding.Auto);
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void WriteFile_ThenReadFile_ShouldPreserveContent()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "test_encoding_" + Guid.NewGuid() + ".txt");

            try
            {
                const string originalContent = "Hello World, Wörld!";
                EncodingHelper.WriteFile(tempFile, originalContent, FileEncoding.Utf8);

                var readContent = EncodingHelper.ReadFile(tempFile, FileEncoding.Utf8);
                readContent.Should().Be(originalContent);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void WriteFile_WithUtf8Encoding_ShouldWriteUtf8()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "test_utf8_" + Guid.NewGuid() + ".txt");

            try
            {
                const string content = "Hello Wörld";
                EncodingHelper.WriteFile(tempFile, content, FileEncoding.Utf8);

                var bytes = File.ReadAllBytes(tempFile);
                bytes.Should().NotContain(0xEF); // Should not have BOM
                bytes.Should().NotContain(0xBB);
                bytes.Should().NotContain(0xBF);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void WriteFile_WithAsciiEncoding_ShouldWriteAscii()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "test_ascii_" + Guid.NewGuid() + ".txt");

            try
            {
                const string content = "Hello World";
                EncodingHelper.WriteFile(tempFile, content, FileEncoding.Ascii);

                var readContent = EncodingHelper.ReadFile(tempFile, FileEncoding.Ascii);
                readContent.Should().Be(content);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void WriteFile_WithUnicodeEncoding_ShouldWriteUtf16()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "test_unicode_" + Guid.NewGuid() + ".txt");

            try
            {
                const string content = "Hello World";
                EncodingHelper.WriteFile(tempFile, content, FileEncoding.Unicode);

                var readContent = EncodingHelper.ReadFile(tempFile, FileEncoding.Unicode);
                readContent.Should().Be(content);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void DetectEncoding_WithUtf8BomFile_ShouldDetectUtf8()
        {
            var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Files", "utf8-bom.txt");

            if (File.Exists(testFile))
            {
                var encoding = EncodingHelper.DetectEncoding(testFile);
                encoding.Should().BeOfType<UTF8Encoding>();
                ((UTF8Encoding)encoding).GetPreamble().Should().NotBeEmpty(); // Should have BOM
            }
        }

        [Fact]
        public void DetectEncoding_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            Action act = () => EncodingHelper.DetectEncoding("nonexistent.txt");
            act.Should().Throw<FileNotFoundException>();
        }
    }
}
