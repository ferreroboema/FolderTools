using System;
using System.IO;

namespace FolderTools.Utilities
{
    /// <summary>
    /// File system utility functions
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Determines if a file is a text file vs binary by checking for null bytes
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file appears to be a text file, false if binary</returns>
        public static bool IsTextFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                // Read first 8192 bytes to check for null bytes (common in binary files)
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[Math.Min(8192, fs.Length)];
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);

                    for (int i = 0; i < bytesRead; i++)
                    {
                        // Check for null byte (indicator of binary file)
                        if (buffer[i] == 0)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a file is currently locked (in use by another process)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file is locked</returns>
        public static bool IsFileLocked(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    fs.Close();
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
        }

        /// <summary>
        /// Safely reads a file with proper error handling
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="encoding">Encoding to use</param>
        /// <param name="content">Output parameter for file content</param>
        /// <param name="error">Output parameter for error message (if failed)</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool TryReadFile(string filePath, Models.FileEncoding encoding, out string content, out string error)
        {
            content = null;
            error = null;

            try
            {
                if (!File.Exists(filePath))
                {
                    error = "File not found";
                    return false;
                }

                if (!IsTextFile(filePath))
                {
                    error = "Binary file detected";
                    return false;
                }

                if (IsFileLocked(filePath))
                {
                    error = "File is locked by another process";
                    return false;
                }

                content = EncodingHelper.ReadFile(filePath, encoding);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Safely writes content to a file with proper error handling
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="content">Content to write</param>
        /// <param name="encoding">Encoding to use</param>
        /// <param name="error">Output parameter for error message (if failed)</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool TryWriteFile(string filePath, string content, Models.FileEncoding encoding, out string error)
        {
            error = null;

            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                EncodingHelper.WriteFile(filePath, content, encoding);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Gets the relative path from a base directory to a file
        /// </summary>
        /// <param name="basePath">Base directory path</param>
        /// <param name="fullPath">Full file path</param>
        /// <returns>Relative path</returns>
        public static string GetRelativePath(string basePath, string fullPath)
        {
            try
            {
                Uri baseUri = new Uri(basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basePath : basePath + Path.DirectorySeparatorChar);
                Uri fullUri = new Uri(fullPath);
                Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
                return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
            }
            catch
            {
                return fullPath;
            }
        }

        /// <summary>
        /// Converts a file size in bytes to a human-readable format
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <returns>Human-readable size string</returns>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
