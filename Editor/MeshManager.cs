using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;

namespace SculptMode
{
    public class MeshManager
    {
        public Mesh SharedMesh { get; private set; }
        public Mesh WorkingMesh { get; private set; }
        public Mesh BackupMesh { get; private set; }

        public MeshFilter TargetFilter
        {
            get { return meshFilter; }
        }

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private readonly bool isColliderGenerated;

        public MeshManager(MeshFilter filter)
        {
            if (filter == null)
                throw new System.ArgumentNullException(nameof(filter), "MeshFilter cannot be null.");

            meshFilter = filter;
            SharedMesh = meshFilter.sharedMesh;

            if (!meshFilter.TryGetComponent(out meshCollider))
            {
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                isColliderGenerated = true;
            }
        }

        public void BackupAndClone()
        {
            if (BackupMesh == null && SharedMesh != null && !SharedMesh.name.Contains("(Clone)"))
            {
                BackupMesh = Object.Instantiate(SharedMesh);
                BackupMesh.name = SharedMesh.name;
            }

            ResetToBackup();
        }

        public void ResetToBackup()
        {
            if (BackupMesh == null) return;

            if (WorkingMesh != null)
                Object.DestroyImmediate(WorkingMesh);

            WorkingMesh = Object.Instantiate(BackupMesh);
            WorkingMesh.name = BackupMesh.name + "(Clone)";
            meshFilter.sharedMesh = WorkingMesh;
            meshCollider.sharedMesh = WorkingMesh;
        }

        public void ModifyMesh()
        {
            if (WorkingMesh == null) return;

            var vertices = WorkingMesh.vertices;
            var normals = WorkingMesh.normals;

            if (vertices.Length > 2 && normals.Length > 2)
                vertices[2] -= normals[2] * 0.01f;

            WorkingMesh.vertices = vertices;
            WorkingMesh.RecalculateNormals();
            WorkingMesh.RecalculateBounds();
        }

        public void Cleanup()
        {
            if (WorkingMesh != null)
            {
                Object.DestroyImmediate(WorkingMesh);
                meshFilter.sharedMesh = SharedMesh;
            }

            if (BackupMesh != null)
            {
                Object.DestroyImmediate(BackupMesh);
            }

            if (meshCollider != null)
            {
                if (isColliderGenerated)
                    Object.DestroyImmediate(meshCollider);
                else
                    meshCollider.sharedMesh = SharedMesh;
            }
        }
    }
}
