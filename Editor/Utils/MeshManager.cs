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
            clone.name = source.name + "_Copy";
            return clone;
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

            PushUndo(); // Save current before replacing
            ReplaceEditingMesh(redoMesh);
        }

        private void PushRedo()
        {
            if (!enableUndo || editingMesh == null)
                return;

            undoBuffer.PushRedo(CloneMesh(editingMesh));
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

        #region === Internal Undo Buffer ===

        /// <summary>
        /// Internal helper for managing mesh history.
        /// </summary>
        private class MeshUndoBuffer
        {
            private readonly Stack<Mesh> undoStack = new();
            private readonly Stack<Mesh> redoStack = new();
            private const int MaxHistory = 10;

            public void PushUndo(Mesh mesh)
            {
                if (mesh == null) return;

                if (undoStack.Count >= MaxHistory)
                    Object.DestroyImmediate(undoStack.Pop());

                undoStack.Push(mesh);
                ClearRedo();
            }

            public void PushRedo(Mesh mesh)
            {
                if (mesh == null) return;

                if (redoStack.Count >= MaxHistory)
                    Object.DestroyImmediate(redoStack.Pop());

                redoStack.Push(mesh);
            }

            public Mesh PopUndo() => undoStack.Count > 0 ? undoStack.Pop() : null;
            public Mesh PopRedo() => redoStack.Count > 0 ? redoStack.Pop() : null;

            public void ClearRedo()
            {
                while (redoStack.Count > 0)
                    Object.DestroyImmediate(redoStack.Pop());
            }

            public void Clear()
            {
                while (undoStack.Count > 0)
                    Object.DestroyImmediate(undoStack.Pop());

                ClearRedo();
            }
        }

        #endregion
    }
}
