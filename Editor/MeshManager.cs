using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;

namespace SculptMode
{
    public class MeshManager
    {

        public MeshFilter TargetFilter
        {
            get { return meshFilter; }
        }

        public MeshCollider Collider
        {
            get { return meshCollider; }
        }

        public Mesh MeshInstance
        {
            get { return workingMesh; }
        }

        private Mesh sharedMesh;
        private Mesh backupMesh;
        private Mesh workingMesh;

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private readonly bool isColliderGenerated;

        public MeshManager(MeshFilter filter)
        {
            if (filter == null)
                throw new System.ArgumentNullException(nameof(filter), "MeshFilter cannot be null.");

            meshFilter = filter;
            sharedMesh = meshFilter.sharedMesh;

            if (!meshFilter.TryGetComponent(out meshCollider))
            {
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                isColliderGenerated = true;
            }
        }

        public void BackupAndClone()
        {
            if (backupMesh == null && sharedMesh != null && !sharedMesh.name.Contains("(Clone)"))
            {
                backupMesh = Object.Instantiate(sharedMesh);
                backupMesh.name = sharedMesh.name;
            }

            ResetToBackup();
        }

        public void ResetToBackup()
        {
            if (backupMesh == null) return;

            if (workingMesh != null)
                Object.DestroyImmediate(workingMesh);

            workingMesh = Object.Instantiate(backupMesh);
            workingMesh.name = backupMesh.name + "(Clone)";
            meshFilter.sharedMesh = workingMesh;
            meshCollider.sharedMesh = workingMesh;
        }

        public void ModifyMesh()
        {
            if (workingMesh == null) return;

            var vertices = workingMesh.vertices;
            var normals = workingMesh.normals;

            if (vertices.Length > 2 && normals.Length > 2)
                vertices[2] -= normals[2] * 0.01f;

            workingMesh.vertices = vertices;
            workingMesh.RecalculateNormals();
            workingMesh.RecalculateBounds();
        }

        public void Cleanup()
        {
            if (workingMesh != null)
            {
                Object.DestroyImmediate(workingMesh);
                meshFilter.sharedMesh = sharedMesh;
            }

            if (backupMesh != null)
            {
                Object.DestroyImmediate(backupMesh);
            }

            if (meshCollider != null)
            {
                if (isColliderGenerated)
                    Object.DestroyImmediate(meshCollider);
                else
                    meshCollider.sharedMesh = sharedMesh;
            }
        }
    }
}
