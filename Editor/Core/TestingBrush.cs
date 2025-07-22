using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace SculptMode
{
    public class TestingBrush : IBrush
    {
        public string Name => "Testing Brush";

        // Parametre brushu
        private float radius = 0.5f;
        private float strength = 0.2f;

        // Cache preview výsledkov
        private Vector3[] previewVertices;

        public void GetGUI()
        {
            radius = EditorGUILayout.Slider("Radius", radius, 0.01f, 2f);
            strength = EditorGUILayout.Slider("Strength", strength, 0.001f, 1f);
        }

        public Vector3[] Preview(MeshManager mm, RaycastHit hit)
        {
            Mesh mesh = mm.MeshInstance;
            Vector3[] originalVerts = mesh.vertices;
            Vector3[] modifiedVerts = new Vector3[originalVerts.Length];
            Transform tf = mm.TargetTransform;

            Vector3 localHitPoint = tf.InverseTransformPoint(hit.point);

            for (int i = 0; i < originalVerts.Length; i++)
            {
                float dist = Vector3.Distance(originalVerts[i], localHitPoint);

                if (dist <= radius)
                {
                    float falloff = 1f - (dist / radius);
                    Vector3 offset = hit.normal * strength * falloff;
                    modifiedVerts[i] = originalVerts[i] + offset;
                }
                else
                {
                    modifiedVerts[i] = originalVerts[i];
                }
            }

            previewVertices = modifiedVerts;

            // Nanes preview do meshu (dočasne)
            mesh.vertices = previewVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return modifiedVerts;
        }

        public void ApplyBrush(MeshManager mm)
        {
            // Pozor! Tento krok sa volá z Tool a nie je Undoovaný priamo tu.
            // Tu sa len aplikuje previewVertices ako trvalý zápis
            // TODO: Kde sa bude volat?

            if (previewVertices == null) return;

            // MeshManager už má túto instanciu mesh, takže iba zápis:
            var mesh = Selection.activeGameObject.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
            {
                mesh.vertices = previewVertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }

            previewVertices = null; // clear cache
        }
    }
}
