using UnityEngine;

namespace SculptMode
{
    public class MeshManager
    {

        public MeshFilter TargetFilter => meshFilter;
        public Transform TargetTransform => meshFilter.transform;
        public MeshCollider Collider => meshCollider;
        public Mesh MeshInstance => editingMesh;

        private readonly Mesh originalMesh; // Never modify original sharedMesh directly. Always work with backupMesh or editingMesh.
        private readonly Mesh backupMesh;
        private Mesh editingMesh;

        private readonly MeshFilter meshFilter;
        private readonly MeshCollider meshCollider;
        private readonly bool isColliderGenerated;


        public MeshManager(MeshFilter filter)
        {
            if (filter == null)
                throw new System.ArgumentNullException(nameof(filter), "MeshFilter cannot be null.");

            meshFilter = filter;
            originalMesh = meshFilter.sharedMesh;

            // Initialize Collider - get or create
            if (!meshFilter.TryGetComponent(out meshCollider))
            {
                meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                isColliderGenerated = true;
            }

            //Initialize BackupMesh
            if (backupMesh == null && originalMesh != null) //&& !sharedMesh.name.Contains("(Clone)"))
            {
                backupMesh = Object.Instantiate(originalMesh);
                backupMesh.name = originalMesh.name;
            }

            ResetToBackup();  // Initialize workingMesh
        }

        public void ResetToBackup()
        {
            // Debug.Log("ResetToBackup called");

            if (backupMesh == null)
            {
                Debug.LogWarning("BackupMesh is null, cannot reset mesh to backup.");
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
                Debug.LogWarning("Working mesh is null, cannot apply.");
                return;
            }

            meshFilter.sharedMesh = editingMesh;
            meshCollider.sharedMesh = editingMesh;
        }

        /// <summary>
        /// Prepočíta normály a bounds pre aktuálny workingMesh.
        /// </summary>
        private void RefreshMesh()
        {
            // if (workingMesh == null) return;
            editingMesh.RecalculateNormals();
            editingMesh.RecalculateBounds();
        }

        /// <summary>
        /// Získa aktuálne vertex pozície pracovného meshu.
        /// </summary>
        public Vector3[] GetVertices()
        {
            return editingMesh?.vertices;
        }

        /// <summary>
        /// Získa aktuálne vertex pozície pracovného meshu.
        /// </summary>
        public Vector3[] GetNormals()
        {
            return editingMesh?.normals;
        }

        /// <summary>
        /// Uloží nové vertexy trvalo do pracovného meshu a aktualizuje collider.
        /// </summary>
        public void ApplyVertices(Vector3[] newVertices)
        {
            if (editingMesh == null || newVertices == null)
            {
                Debug.LogWarning("Working mesh or new Vertices are null, cannot apply.");
                return;
            }

            editingMesh.vertices = newVertices;

            RefreshMesh();
            ApplyMesh();
        }

        public void Cleanup()
        {
            if (editingMesh != null)
            {
                Object.DestroyImmediate(editingMesh);
                meshFilter.sharedMesh = originalMesh;
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
                    meshCollider.sharedMesh = originalMesh;
            }
        }

        #region Testing Codde
        // ====== TESTING CODE ====== 
        /*
        public void ModifyMesh()
        {
            if (workingMesh == null) return;
            Debug.Log("Working Mesh loaded for modify.");

            var vertices = workingMesh.vertices;
            var normals = workingMesh.normals;

            if (vertices.Length > 2 && normals.Length > 2)
                vertices[2] -= normals[2] * 0.05f;

            Debug.Log("Vertices modifyied: " + vertices[2]);

            workingMesh.vertices = vertices;
            workingMesh.RecalculateNormals();
            workingMesh.RecalculateBounds();
        }
        */
        #endregion
    }
}
