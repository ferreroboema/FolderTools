using System;

namespace FolderTools.Services
{
    /// <summary>
    /// Interface for file system utility operations - enables testability through mocking
    /// </summary>
    public interface IFileHelper
    {
        /// <summary>
        /// Determines if a file is a text file vs binary by checking for null bytes
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file appears to be a text file, false if binary</returns>
        bool IsTextFile(string filePath);

        /// <summary>
        /// Checks if a file is currently locked (in use by another process)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file is locked</returns>
        bool IsFileLocked(string filePath);

        /// <summary>
        /// Safely reads a file with proper error handling
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="encoding">Encoding to use</param>
        /// <param name="content">Output parameter for file content</param>
        /// <param name="error">Output parameter for error message (if failed)</param>
        /// <returns>True if successful, false otherwise</returns>
        bool TryReadFile(string filePath, Models.FileEncoding encoding, out string content, out string error);

        /// <summary>
        /// Safely writes content to a file with proper error handling
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="content">Content to write</param>
        /// <param name="encoding">Encoding to use</param>
        /// <param name="error">Output parameter for error message (if failed)</param>
        /// <returns>True if successful, false otherwise</returns>
        bool TryWriteFile(string filePath, string content, Models.FileEncoding encoding, out string error);

        /// <summary>
        /// Gets the relative path from a base directory to a file
        /// </summary>
        /// <param name="basePath">Base directory path</param>
        /// <param name="fullPath">Full file path</param>
        /// <returns>Relative path</returns>
        string GetRelativePath(string basePath, string fullPath);

        /// <summary>
        /// Converts a file size in bytes to a human-readable format
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <returns>Human-readable size string</returns>
        string FormatFileSize(long bytes);
    }
}
