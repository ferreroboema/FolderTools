using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FolderTools.Models;

namespace FolderTools.Utilities
{
    /// <summary>
    /// Validates rename operations for collisions before execution
    /// </summary>
    public class RenameCollisionValidator
    {
        /// <summary>
        /// Detects collisions in a dictionary of originalPath -> newPath mappings
        /// Checks for: (a) multiple sources mapping to same target name, (b) target already exists on disk
        /// </summary>
        /// <param name="renameMap">Dictionary mapping original paths to new paths</param>
        /// <returns>Collision detection result</returns>
        public RenameCollisionResult DetectCollisions(Dictionary<string, string> renameMap)
        {
            var result = new RenameCollisionResult();

            if (renameMap == null || renameMap.Count == 0)
            {
                return result;
            }

            // 1. Check for duplicate target names (multiple sources -> same target)
            var targetGroups = renameMap
                .Where(kvp => kvp.Value != null)
                .GroupBy(kvp => Path.GetFullPath(kvp.Value), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in targetGroups)
            {
                var collision = new RenameCollision
                {
                    TargetName = Path.GetFileName(group.First().Value),
                    SourceFiles = group.Select(kvp => kvp.Key).ToList()
                };
                result.Collisions.Add(collision);
            }

            // 2. Check for targets that already exist on disk (and are not the source file itself)
            foreach (var kvp in renameMap)
            {
                if (kvp.Value == null) continue;

                try
                {
                    if (File.Exists(kvp.Value))
                    {
                        string sourceFullPath = Path.GetFullPath(kvp.Key);
                        string targetFullPath = Path.GetFullPath(kvp.Value);

                        if (!string.Equals(sourceFullPath, targetFullPath, StringComparison.OrdinalIgnoreCase))
                        {
                            // Check if this collision was already reported as a duplicate target
                            bool alreadyReported = result.Collisions.Any(c =>
                                c.SourceFiles.Contains(kvp.Key) && c.SourceFiles.Count > 1);

                            if (!alreadyReported)
                            {
                                var collision = new RenameCollision
                                {
                                    TargetName = Path.GetFileName(kvp.Value),
                                    SourceFiles = new List<string> { kvp.Key },
                                    TargetExistsOnDisk = true
                                };
                                result.Collisions.Add(collision);
                            }
                        }
                    }
                }
                catch
                {
                    // Skip files that cause path resolution errors
                }
            }

            return result;
        }
    }
}
