using System;
using System.IO;
using System.Text;

namespace FolderTools.Utilities
{
    /// <summary>
    /// Utilities for detecting and working with text file encodings
    /// </summary>
    public static class EncodingHelper
    {
        /// <summary>
        /// Detects the encoding of a file by analyzing its BOM (Byte Order Mark) and content
        /// </summary>
        /// <param name="filePath">Path to the file to analyze</param>
        /// <returns>Detected encoding (defaults to UTF-8 if unable to determine)</returns>
        public static Encoding DetectEncoding(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            // Read the first 5 bytes to check for BOM
            byte[] bom = new byte[5];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(bom, 0, 5);
            }

            // Check for BOM and return appropriate encoding
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
            {
                return new UTF8Encoding(true);
            }
            if (bom[0] == 0xFF && bom[1] == 0xFE)
            {
                return Encoding.Unicode; // UTF-16 LE
            }
            if (bom[0] == 0xFE && bom[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode; // UTF-16 BE
            }
            if (bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
            {
                return Encoding.UTF32; // UTF-32 LE
            }
            if (bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF)
            {
                return new UTF32Encoding(true, true); // UTF-32 BE
            }

            // No BOM found, try to detect by reading the file content
            return DetectByContent(filePath);
        }

        /// <summary>
        /// Attempts to detect encoding by analyzing file content
        /// </summary>
        private static Encoding DetectByContent(string filePath)
        {
            try
            {
                // Read entire file to analyze
                byte[] buffer = File.ReadAllBytes(filePath);

                // Heuristic: check if the file is likely ASCII or UTF-8
                bool isAscii = true;
                bool hasUtf8Sequence = false;

                for (int i = 0; i < buffer.Length; i++)
                {
                    byte b = buffer[i];

                    // ASCII range
                    if (b <= 0x7F)
                    {
                        continue;
                    }

                    isAscii = false;

                    // Check for UTF-8 multi-byte sequences
                    if ((b & 0xE0) == 0xC0) // 2-byte sequence
                    {
                        if (i + 1 < buffer.Length && (buffer[i + 1] & 0xC0) == 0x80)
                        {
                            hasUtf8Sequence = true;
                            i++;
                        }
                    }
                    else if ((b & 0xF0) == 0xE0) // 3-byte sequence
                    {
                        if (i + 2 < buffer.Length &&
                            (buffer[i + 1] & 0xC0) == 0x80 &&
                            (buffer[i + 2] & 0xC0) == 0x80)
                        {
                            hasUtf8Sequence = true;
                            i += 2;
                        }
                    }
                    else if ((b & 0xF8) == 0xF0) // 4-byte sequence
                    {
                        if (i + 3 < buffer.Length &&
                            (buffer[i + 1] & 0xC0) == 0x80 &&
                            (buffer[i + 2] & 0xC0) == 0x80 &&
                            (buffer[i + 3] & 0xC0) == 0x80)
                        {
                            hasUtf8Sequence = true;
                            i += 3;
                        }
                    }
                }

                if (isAscii)
                {
                    return Encoding.ASCII;
                }

                if (hasUtf8Sequence)
                {
                    return new UTF8Encoding(false);
                }

                // Default to UTF-8 without BOM
                return new UTF8Encoding(false);
            }
            catch
            {
                // If detection fails, default to UTF-8
                return new UTF8Encoding(false);
            }
        }

        /// <summary>
        /// Reads a file with automatic encoding detection
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>File content as a string</returns>
        public static string ReadFileWithEncoding(string filePath)
        {
            Encoding encoding = DetectEncoding(filePath);
            return File.ReadAllText(filePath, encoding);
        }

        /// <summary>
        /// Gets an Encoding object from a FileEncoding enum value
        /// </summary>
        /// <param name="encoding">The encoding type</param>
        /// <returns>System.Text.Encoding object</returns>
        public static Encoding GetEncoding(Models.FileEncoding encoding)
        {
            switch (encoding)
            {
                case Models.FileEncoding.Utf8:
                    return new UTF8Encoding(false);
                case Models.FileEncoding.Ascii:
                    return Encoding.ASCII;
                case Models.FileEncoding.Unicode:
                    return Encoding.Unicode;
                case Models.FileEncoding.Auto:
                default:
                    return new UTF8Encoding(false); // Default for Auto
            }
        }

        /// <summary>
        /// Reads a file with the specified encoding, or auto-detect if set to Auto
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="encoding">Encoding to use</param>
        /// <returns>File content as a string</returns>
        public static string ReadFile(string filePath, Models.FileEncoding encoding)
        {
            if (encoding == Models.FileEncoding.Auto)
            {
                return ReadFileWithEncoding(filePath);
            }
            else
            {
                return File.ReadAllText(filePath, GetEncoding(encoding));
            }
        }

        /// <summary>
        /// Writes content to a file with the specified encoding
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="content">Content to write</param>
        /// <param name="encoding">Encoding to use</param>
        public static void WriteFile(string filePath, string content, Models.FileEncoding encoding)
        {
            File.WriteAllText(filePath, content, GetEncoding(encoding));
        }
    }
}
