using System.Collections.Generic;
using UnityEngine;

namespace SculptMode
{
    public static class MeshManagerFactory
    {
        private static readonly Dictionary<MeshFilter, MeshManager> managers = new();

        public static MeshManager GetOrCreate(MeshFilter filter)
        {
            if (filter == null) return null;

            if (!managers.TryGetValue(filter, out var manager))
            {
                manager = new MeshManager(filter);
                managers[filter] = manager;
            }

            return manager;
        }

        public static void ClearAll()
        {
            foreach (var manager in managers.Values)
                manager?.Cleanup();

            managers.Clear();
        }

        public static void Remove(MeshFilter filter)
        {
            if (filter == null) return;

            if (managers.TryGetValue(filter, out var manager))
            {
                manager?.Cleanup();
                managers.Remove(filter);
            }
        }
    }
}
