using System.Collections.Generic;
using UnityEngine;

namespace SculptTool.Editor.Utils
{
    /// <summary>
    /// A factory and cache system for managing <see cref="MeshManager"/> instances tied to specific <see cref="MeshFilter"/> components.
    /// Ensures that each MeshFilter has a single, reusable MeshManager, avoiding redundant allocations.
    /// </summary>
    public static class MeshManagerFactory
    {
        /// <summary>
        /// Internal cache mapping each MeshFilter to its corresponding MeshManager.
        /// </summary>
        private static readonly Dictionary<MeshFilter, MeshManager> managers = new();

        /// <summary>
        /// Retrieves an existing <see cref="MeshManager"/> for the given <see cref="MeshFilter"/>,
        /// or creates a new one if it doesn't exist yet.
        /// </summary>
        /// <param name="filter">The target MeshFilter.</param>
        /// <returns>The associated MeshManager instance, or null if filter is null.</returns>
        public static MeshManager GetOrCreate(MeshFilter filter)
        {
            if (filter == null)
                return null;

            if (!managers.TryGetValue(filter, out var manager))
            {
                manager = new MeshManager(filter);
                managers[filter] = manager;
            }

            return manager;
        }

        /// <summary>
        /// Cleans up and removes all registered MeshManagers.
        /// Should be called when resetting or unloading the sculpting context.
        /// </summary>
        public static void ClearAll()
        {
            foreach (var manager in managers.Values)
            {
                manager?.Cleanup();
            }

            managers.Clear();
        }

        /// <summary>
        /// Cleans up and removes a specific <see cref="MeshManager"/> tied to the given <see cref="MeshFilter"/>.
        /// </summary>
        /// <param name="filter">The MeshFilter whose MeshManager should be removed.</param>
        public static void Remove(MeshFilter filter)
        {
            if (filter == null)
                return;

            if (managers.TryGetValue(filter, out var manager))
            {
                manager?.Cleanup();
                managers.Remove(filter);
            }
        }
    }
}
