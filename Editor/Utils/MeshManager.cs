using UnityEngine;
using System.Collections.Generic;

namespace SculptTool.Editor.Utils
{
    /// <summary>
    /// Manages a working mesh instance for safe in-editor editing, 
    /// including support for undo/redo and collider management.
    /// </summary>
    public class MeshManager
    {
        #region === Public Accessors ===

        public MeshFilter TargetFilter => meshFilter;
        public Transform TargetTransform => meshFilter.transform;
        public MeshCollider Collider => meshCollider;
        public Mesh MeshInstance => editingMesh;

        #endregion

        #region === Private Fields ===

        private readonly Mesh originalMesh;
        private readonly Mesh backupMesh;
        private Mesh editingMesh;

        private readonly MeshFilter meshFilter;
        private readonly MeshCollider meshCollider;
        private readonly bool isColliderGenerated;

        private readonly List<Vector3> verticesBuffer = new();
        private readonly List<Vector3> normalsBuffer = new();

        private readonly MeshUndoBuffer undoBuffer;
        private readonly bool enableUndo;

        #endregion

        #region === Constructor ===

        /// <summary>
        /// Initializes a MeshManager for the given MeshFilter.
        /// </summary>
        /// <param name="filter">The target MeshFilter to edit.</param>
        /// <param name="enableUndo">Whether undo/redo functionality should be enabled.</param>
        public MeshManager(MeshFilter filter, bool enableUndo = true)
        {
            if (filter == null)
                throw new System.ArgumentNullException(nameof(filter), "MeshFilter cannot be null.");

            this.enableUndo = enableUndo;
            meshFilter = filter;
            originalMesh = meshFilter.sharedMesh;

            // Initialize mesh geometry buffers - not needed
            // verticesBuffer = new List<Vector3>(originalMesh.vertexCount);
            // normalsBuffer = new List<Vector3>(originalMesh.vertexCount);

            // Initialize or create collider
            if (!meshFilter.TryGetComponent(out meshCollider))
            {
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                isColliderGenerated = true;
            }

            // Backup original mesh
            if (originalMesh != null)
            {
                backupMesh = Object.Instantiate(originalMesh);
                backupMesh.name = originalMesh.name;
            }

            // Initialize undo buffer if needed
            if (enableUndo)
                undoBuffer = new MeshUndoBuffer();

            ResetToBackup();
        }

        #endregion

        #region === Mesh State Management ===

        /// <summary>
        /// Resets the working mesh to a fresh copy of the backup mesh.
        /// </summary>
        public void ResetToBackup()
        {
            if (backupMesh == null)
            {
                Debug.LogWarning("MeshManager: Backup mesh is null.");
                return;
            }

            if (editingMesh != null)
                Object.DestroyImmediate(editingMesh);

            editingMesh = Object.Instantiate(backupMesh);
            editingMesh.name = backupMesh.name + "(Clone)";

            ApplyMesh();
        }

        private void ApplyMesh()
        {
            if (editingMesh == null)
            {
                Debug.LogWarning("MeshManager: Cannot apply null editing mesh.");
                return;
            }

            meshFilter.sharedMesh = editingMesh;
            meshCollider.sharedMesh = editingMesh;
        }

        private void RefreshMesh()
        {
            editingMesh.RecalculateNormals();
            editingMesh.RecalculateBounds();
        }

        private void ReplaceEditingMesh(Mesh mesh)
        {
            if (editingMesh != null)
                Object.DestroyImmediate(editingMesh);

            editingMesh = mesh;
            ApplyMesh();
        }

        private static Mesh CloneMesh(Mesh source)
        {
            var clone = Object.Instantiate(source);
            clone.name = source.name;
            return clone;
        }

        #endregion

        #region === Vertex Editing ===

        /// <summary>
        /// Returns a reference to the internal Vertices buffer (auto-refreshed from mesh).
        /// </summary>
        public List<Vector3> GetVerticesBuffer()
        {
            if (editingMesh != null)
                editingMesh.GetVertices(verticesBuffer);

            return verticesBuffer;
        }

        /// <summary>
        /// Returns a reference to the internal normal buffer (auto-refreshed from mesh).
        /// </summary>
        public List<Vector3> GetNormalsBuffer()
        {
            if (editingMesh != null)
                editingMesh.GetNormals(normalsBuffer);

            return normalsBuffer;
        }

        /// <summary>
        /// Applies changes from the vertex buffer to the mesh.
        /// Also pushes the current state to the undo stack.
        /// </summary>
        public void ApplyVerticesBuffer()
        {
            if (!ValidateVerticesBuffer())
                return;

            PushUndo();
            // Debug.Log("PushUndo: " + undoBuffer.undoStack.Count);

            editingMesh.SetVertices(verticesBuffer);
            RefreshMesh();
            ApplyMesh();
        }

        /// <summary>
        /// Validates that the vertex buffer matches the mesh structure.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool ValidateVerticesBuffer()
        {
            if (editingMesh == null || verticesBuffer == null)
            {
                Debug.LogWarning("MeshManager: editingMesh or verticesBuffer is null.");
                return false;
            }

            if (editingMesh.vertexCount != verticesBuffer.Count)
            {
                Debug.LogError($"MeshManager: Vertex count mismatch (mesh: {editingMesh.vertexCount}, buffer: {verticesBuffer.Count}).");
                return false;
            }

            return true;
        }

        #endregion

        #region === Cleanup ===

        /// <summary>
        /// Destroys mesh instances and restores the original mesh and collider.
        /// </summary>
        public void Cleanup()
        {
            if (editingMesh != null)
            {
                Object.DestroyImmediate(editingMesh);
                meshFilter.sharedMesh = originalMesh;
            }

            if (backupMesh != null)
                Object.DestroyImmediate(backupMesh);

            if (meshCollider != null)
            {
                if (isColliderGenerated)
                    Object.DestroyImmediate(meshCollider);
                else
                    meshCollider.sharedMesh = originalMesh;
            }

            verticesBuffer?.Clear();
            normalsBuffer?.Clear();
            undoBuffer?.Clear();
        }

        #endregion

        #region === Undo/Redo ===

        /// <summary>
        /// Pushes the current mesh state into the undo buffer.
        /// </summary>
        public void PushUndo()
        {
            if (!enableUndo || editingMesh == null)
                return;

            undoBuffer.PushUndo(CloneMesh(editingMesh));
        }

        /// <summary>
        /// Undoes the last mesh change, if available.
        /// </summary>
        public void Undo()
        {
            if (!enableUndo) return;

            var undoMesh = undoBuffer.PopUndo();
            if (undoMesh == null) return;

            PushRedo(); // Save current before replacing
            ReplaceEditingMesh(undoMesh);
        }

        /// <summary>
        /// Redoes the last undone mesh change, if available.
        /// </summary>
        public void Redo()
        {
            if (!enableUndo) return;

            var redoMesh = undoBuffer.PopRedo();
            if (redoMesh == null) return;

            undoBuffer.PushUndo(CloneMesh(editingMesh), clearRedo: false);
            ReplaceEditingMesh(redoMesh);
        }

        private void PushRedo()
        {
            if (!enableUndo || editingMesh == null)
                return;

            undoBuffer.PushRedo(CloneMesh(editingMesh));
        }

        #endregion

        #region === Internal Undo Buffer ===

        /// <summary>
        /// Efficient and safe undo/redo buffer for mesh history,
        /// using LinkedList to support fast FIFO limit management.
        /// </summary>
        private class MeshUndoBuffer
        {
            private readonly LinkedList<Mesh> undoHistory = new();
            private readonly LinkedList<Mesh> redoHistory = new();

            private readonly int maxHistory;

            /// <summary>
            /// Initializes the buffer with a maximum history size.
            /// </summary>
            /// <param name="maxHistory">Maximum total number of mesh versions to keep in undo/redo.</param>
            public MeshUndoBuffer(int maxHistory = 100)
            {
                this.maxHistory = System.Math.Max(1, maxHistory); // ensure at least one entry allowed
            }

            /// <summary>
            /// Pushes a mesh into the undo history.
            /// Optionally clears the redo history (which should be done on new user action).
            /// </summary>
            public void PushUndo(Mesh mesh, bool clearRedo = true)
            {
                if (mesh == null) return;

                // Add new mesh to undo history
                undoHistory.AddLast(mesh);

                // Enforce max size across both histories
                EnforceLimit();

                // Clear redo stack on forward action
                if (clearRedo)
                    ClearRedo();
            }

            /// <summary>
            /// Pushes a mesh into the redo history.
            /// </summary>
            public void PushRedo(Mesh mesh)
            {
                if (mesh == null) return;

                redoHistory.AddLast(mesh);
                EnforceLimit();
            }

            /// <summary>
            /// Pops the most recent undo mesh.
            /// </summary>
            public Mesh PopUndo()
            {
                if (undoHistory.Count == 0) return null;

                var last = undoHistory.Last.Value;
                undoHistory.RemoveLast();
                return last;
            }

            /// <summary>
            /// Pops the most recent redo mesh.
            /// </summary>
            public Mesh PopRedo()
            {
                if (redoHistory.Count == 0) return null;

                var last = redoHistory.Last.Value;
                redoHistory.RemoveLast();
                return last;
            }

            /// <summary>
            /// Clears the redo history and destroys associated meshes.
            /// </summary>
            public void ClearRedo()
            {
                foreach (var mesh in redoHistory)
                    Object.DestroyImmediate(mesh);

                redoHistory.Clear();
            }

            /// <summary>
            /// Clears both undo and redo histories and destroys all associated meshes.
            /// </summary>
            public void Clear()
            {
                foreach (var mesh in undoHistory)
                    Object.DestroyImmediate(mesh);
                undoHistory.Clear();

                ClearRedo();
            }

            /// <summary>
            /// Removes oldest meshes from undo/redo histories if total exceeds limit.
            /// Prioritizes removing from undo history first.
            /// </summary>
            private void EnforceLimit()
            {
                while (undoHistory.Count + redoHistory.Count > maxHistory)
                {
                    if (undoHistory.Count > 0)
                    {
                        var oldest = undoHistory.First.Value;
                        Object.DestroyImmediate(oldest);
                        undoHistory.RemoveFirst();
                    }
                    else if (redoHistory.Count > 0)
                    {
                        var oldest = redoHistory.First.Value;
                        Object.DestroyImmediate(oldest);
                        redoHistory.RemoveFirst();
                    }
                }
            }
        }


        #endregion
    }
}
