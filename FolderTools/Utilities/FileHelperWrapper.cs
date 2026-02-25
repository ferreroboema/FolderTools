using FolderTools.Services;

namespace FolderTools.Utilities
{
    /// <summary>
    /// Wrapper around static FileHelper methods - implements IFileHelper for dependency injection
    /// </summary>
    public class FileHelperWrapper : IFileHelper
    {
        /// <summary>
        /// Determines if a file is a text file vs binary by checking for null bytes
        /// </summary>
        public bool IsTextFile(string filePath)
        {
            return FileHelper.IsTextFile(filePath);
        }

        /// <summary>
        /// Checks if a file is currently locked (in use by another process)
        /// </summary>
        public bool IsFileLocked(string filePath)
        {
            return FileHelper.IsFileLocked(filePath);
        }

        /// <summary>
        /// Safely reads a file with proper error handling
        /// </summary>
        public bool TryReadFile(string filePath, Models.FileEncoding encoding, out string content, out string error)
        {
            return FileHelper.TryReadFile(filePath, encoding, out content, out error);
        }

        /// <summary>
        /// Safely writes content to a file with proper error handling
        /// </summary>
        public bool TryWriteFile(string filePath, string content, Models.FileEncoding encoding, out string error)
        {
            return FileHelper.TryWriteFile(filePath, content, encoding, out error);
        }

        /// <summary>
        /// Gets the relative path from a base directory to a file
        /// </summary>
        public string GetRelativePath(string basePath, string fullPath)
        {
            return FileHelper.GetRelativePath(basePath, fullPath);
        }

        /// <summary>
        /// Converts a file size in bytes to a human-readable format
        /// </summary>
        public string FormatFileSize(long bytes)
        {
            return FileHelper.FormatFileSize(bytes);
        }
    }
}
