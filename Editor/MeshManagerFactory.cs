using System.Collections.Generic;
using UnityEngine;

namespace SculptMode
{
    public static class MeshManagerFactory
    {
        private static readonly Dictionary<int, MeshManager> managerCache = new();

        public static MeshManager GetOrCreate(MeshFilter filter)
        {
            if (filter == null)
                return null;

            int id = filter.GetInstanceID();

            if (!managerCache.TryGetValue(id, out var manager) || manager.TargetFilter == null)
            {
                manager = new MeshManager(filter);
                managerCache[id] = manager;
            }

            return manager;
        }

        public static void Invalidate(MeshFilter filter)
        {
            if (filter == null) return;
            managerCache.Remove(filter.GetInstanceID());
        }

        public static void ClearAll()
        {
            foreach (var mgr in managerCache.Values)
                mgr?.Cleanup();

            managerCache.Clear();
        }
    }
}
