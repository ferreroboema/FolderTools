using System;
using System.Collections.Generic;
using System.Linq;
using FolderTools.Models;

namespace FolderTools.Utilities
{
    /// <summary>
    /// Detects collisions in bulk search/replace operations where replacement values
    /// from one pair become search patterns in subsequent pairs
    /// </summary>
    public class BulkCollisionValidator
    {
        /// <summary>
        /// Maximum chain length to prevent infinite loops in circular references
        /// </summary>
        private const int MaxChainLength = 100;

        /// <summary>
        /// Detects collisions in a list of search/replace pairs
        /// </summary>
        /// <param name="pairs">The list of search/replace pairs to check</param>
        /// <returns>A CollisionDetectionResult containing all detected collisions</returns>
        public CollisionDetectionResult DetectCollisions(List<SearchReplacePair> pairs)
        {
            var result = new CollisionDetectionResult();

            if (pairs == null || pairs.Count == 0)
            {
                return result;
            }

            // Step 1: Build a dictionary mapping search patterns to their pairs
            // O(n) time complexity
            var searchPatternToPairs = new Dictionary<string, List<SearchReplacePair>>(StringComparer.Ordinal);
            for (int i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                string pattern = pair.SearchPattern;

                if (string.IsNullOrWhiteSpace(pattern))
                    continue;

                if (!searchPatternToPairs.ContainsKey(pattern))
                {
                    searchPatternToPairs[pattern] = new List<SearchReplacePair>();
                }
                searchPatternToPairs[pattern].Add(pair);
            }

            // Step 2: For each pair, check if its replacement exists as a search pattern
            // This detects chains and collisions
            var processedChains = new HashSet<string>();
            var allChains = new List<List<SearchReplacePair>>();

            for (int i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                string replacement = pair.Replacement;

                // Skip empty replacements (deletions) - not a collision
                if (string.IsNullOrEmpty(replacement))
                    continue;

                // Skip self-references (A->A) - no-op, not a collision
                if (replacement == pair.SearchPattern)
                    continue;

                // Check if this replacement exists as a search pattern
                if (searchPatternToPairs.ContainsKey(replacement))
                {
                    // Found a collision - build the chain
                    var chain = BuildChain(pair, searchPatternToPairs);

                    if (chain.Count > 1)
                    {
                        // Create a unique key for this chain to avoid duplicates
                        string chainKey = GetChainKey(chain);
                        if (!processedChains.Contains(chainKey))
                        {
                            processedChains.Add(chainKey);
                            allChains.Add(chain);
                        }
                    }
                }
            }

            // Step 3: Merge overlapping chains and create CollisionInfo objects
            var mergedChains = MergeOverlappingChains(allChains);

            foreach (var chain in mergedChains)
            {
                result.AddCollision(new CollisionInfo(chain));
            }

            return result;
        }

        /// <summary>
        /// Builds a chain starting from a given pair by following replacement links
        /// </summary>
        /// <param name="startPair">The pair to start from</param>
        /// <param name="searchPatternToPairs">Dictionary mapping search patterns to pairs</param>
        /// <returns>The chain of pairs involved in the collision</returns>
        private List<SearchReplacePair> BuildChain(
            SearchReplacePair startPair,
            Dictionary<string, List<SearchReplacePair>> searchPatternToPairs)
        {
            var chain = new List<SearchReplacePair>();
            var visited = new HashSet<string>();
            string currentReplacement = startPair.Replacement;

            chain.Add(startPair);
            visited.Add(startPair.SearchPattern);

            int safetyCounter = 0;
            while (!string.IsNullOrEmpty(currentReplacement) &&
                   searchPatternToPairs.ContainsKey(currentReplacement) &&
                   safetyCounter < MaxChainLength)
            {
                safetyCounter++;

                // Skip if we've already seen this pattern (circular reference)
                if (visited.Contains(currentReplacement))
                    break;

                visited.Add(currentReplacement);

                // Get the first pair that matches this search pattern
                var nextPairs = searchPatternToPairs[currentReplacement];
                if (nextPairs.Count > 0)
                {
                    var nextPair = nextPairs[0];
                    chain.Add(nextPair);
                    currentReplacement = nextPair.Replacement;
                }
                else
                {
                    break;
                }
            }

            return chain;
        }

        /// <summary>
        /// Creates a unique key for a chain to identify duplicates
        /// </summary>
        /// <param name="chain">The chain to create a key for</param>
        /// <returns>A unique string key for the chain</returns>
        private string GetChainKey(List<SearchReplacePair> chain)
        {
            if (chain.Count == 0)
                return "";

            var parts = new List<string>();
            foreach (var pair in chain)
            {
                parts.Add($"{pair.SearchPattern}->{pair.Replacement}");
            }
            return string.Join("|", parts);
        }

        /// <summary>
        /// Merges overlapping chains to avoid duplicate warnings
        /// </summary>
        /// <param name="chains">List of chains to merge</param>
        /// <returns>List of merged chains</returns>
        private List<List<SearchReplacePair>> MergeOverlappingChains(List<List<SearchReplacePair>> chains)
        {
            if (chains.Count == 0)
                return chains;

            var merged = new List<List<SearchReplacePair>>();
            var used = new HashSet<int>();

            for (int i = 0; i < chains.Count; i++)
            {
                if (used.Contains(i))
                    continue;

                var currentChain = new List<SearchReplacePair>(chains[i]);
                used.Add(i);

                // Look for overlapping chains
                bool foundOverlap = true;
                while (foundOverlap)
                {
                    foundOverlap = false;
                    for (int j = 0; j < chains.Count; j++)
                    {
                        if (used.Contains(j))
                            continue;

                        if (ChainsOverlap(currentChain, chains[j]))
                        {
                            // Merge the chains
                            currentChain = MergeChains(currentChain, chains[j]);
                            used.Add(j);
                            foundOverlap = true;
                            break;
                        }
                    }
                }

                merged.Add(currentChain);
            }

            return merged;
        }

        /// <summary>
        /// Checks if two chains share any common pairs
        /// </summary>
        /// <param name="chain1">First chain</param>
        /// <param name="chain2">Second chain</param>
        /// <returns>True if chains overlap</returns>
        private bool ChainsOverlap(List<SearchReplacePair> chain1, List<SearchReplacePair> chain2)
        {
            var patterns1 = new HashSet<string>();
            foreach (var pair in chain1)
            {
                patterns1.Add(pair.SearchPattern);
            }

            foreach (var pair in chain2)
            {
                if (patterns1.Contains(pair.SearchPattern))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Merges two chains, removing duplicates while preserving order
        /// </summary>
        /// <param name="chain1">First chain</param>
        /// <param name="chain2">Second chain</param>
        /// <returns>Merged chain</returns>
        private List<SearchReplacePair> MergeChains(
            List<SearchReplacePair> chain1,
            List<SearchReplacePair> chain2)
        {
            var merged = new List<SearchReplacePair>(chain1);
            var existingPatterns = new HashSet<string>();

            foreach (var pair in chain1)
            {
                existingPatterns.Add(pair.SearchPattern);
            }

            // Add pairs from chain2 that aren't already in the merged chain
            // This is a simple merge - for a more sophisticated approach, we could
            // try to maintain the chain order
            foreach (var pair in chain2)
            {
                if (!existingPatterns.Contains(pair.SearchPattern))
                {
                    merged.Add(pair);
                    existingPatterns.Add(pair.SearchPattern);
                }
            }

            // Sort by line number to maintain CSV order
            return merged.OrderBy(p => p.LineNumber).ToList();
        }
    }
}
