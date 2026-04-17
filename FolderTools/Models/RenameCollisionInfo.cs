using System.Collections.Generic;
using System.Linq;

namespace FolderTools.Models
{
    /// <summary>
    /// Represents a single rename collision (duplicate target or existing file)
    /// </summary>
    public class RenameCollision
    {
        /// <summary>
        /// The target filename that would cause a collision
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Source files that map to this target
        /// </summary>
        public List<string> SourceFiles { get; set; }

        /// <summary>
        /// Whether the target file already exists on disk
        /// </summary>
        public bool TargetExistsOnDisk { get; set; }

        public RenameCollision()
        {
            SourceFiles = new List<string>();
        }

        /// <summary>
        /// Returns a description of this collision
        /// </summary>
        public string GetDescription()
        {
            if (SourceFiles.Count > 1)
            {
                return $"Multiple files map to \"{TargetName}\": {string.Join(", ", SourceFiles)}";
            }
            return $"Target already exists: \"{TargetName}\" (from {SourceFiles[0]})";
        }
    }

    /// <summary>
    /// Result of rename collision detection
    /// </summary>
    public class RenameCollisionResult
    {
        /// <summary>
        /// List of detected collisions
        /// </summary>
        public List<RenameCollision> Collisions { get; set; }

        /// <summary>
        /// Whether any collisions were detected
        /// </summary>
        public bool HasCollisions => Collisions.Count > 0;

        public RenameCollisionResult()
        {
            Collisions = new List<RenameCollision>();
        }

        /// <summary>
        /// Returns a summary message of detected collisions
        /// </summary>
        public string GetSummaryMessage()
        {
            if (!HasCollisions) return "No collisions detected.";
            return $"{Collisions.Count} rename collision(s) detected.";
        }
    }
}
