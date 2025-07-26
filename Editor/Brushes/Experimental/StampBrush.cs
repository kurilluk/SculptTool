// using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;


// namespace SculptTool
// {
//     public class StampBrush //: IBrush
//     {
//         public float radius = 1.5f;
//         public float intensity = 0.3f;
//         public float scale = 3f;

//         public string Name => "Stamp (Perlin Noise)";

//         public void GetGUI()
//         {
//             radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 10f);
//             intensity = EditorGUILayout.Slider("Intensity", intensity, -1f, 1f);
//             scale = EditorGUILayout.Slider("Noise Scale", scale, 0.1f, 10f);
//         }

//         public Vector3[] Preview(MeshManager mm, RaycastHit hit)
//         {
//             Mesh mesh = mm.MeshInstance;
//             Transform meshTransform = mm.TargetTransform;

//             if (mesh == null || meshTransform == null) return null;

//             Vector3[] vertices = mesh.vertices;
//             Vector3[] normals = mesh.normals;
//             Vector3 localHit = meshTransform.InverseTransformPoint(hit.point);

//             for (int i = 0; i < vertices.Length; i++)
//             {
//                 float dist = Vector3.Distance(vertices[i], localHit);
//                 if (dist > radius) continue;

//                 float normalizedDist = dist / radius;
//                 float falloff = Mathf.Pow(1f - normalizedDist, 2f);
//                 Vector3 noisePos = vertices[i] * scale;
//                 float noise = Mathf.PerlinNoise(noisePos.x, noisePos.z);
//                 float displacement = intensity * noise * falloff;

//                 Vector3 worldPos = meshTransform.TransformPoint(vertices[i]);
//                 Vector3 offsetDir = meshTransform.TransformDirection(normals[i]);
//                 float size = Mathf.Abs(displacement) * 0.5f;

//                 Handles.color = displacement >= 0 ? Color.green : Color.red;
//                 Handles.CubeHandleCap(0, worldPos + offsetDir * displacement, Quaternion.identity, size, EventType.Repaint);
//             }

//             return vertices; //TODO: update! displacement points needed
//         }

//         public void ApplyBrush(MeshManager mm)
//         {
//             // Vector3[] vertices = mesh.vertices;
//             // Vector3[] normals = mesh.normals;
//             // Vector3 localHit = meshTransform.InverseTransformPoint(hit.point);

//             // Undo.RegisterCompleteObjectUndo(mesh, "Apply Stamp Brush");

//             // for (int i = 0; i < vertices.Length; i++)
//             // {
//             //     float dist = Vector3.Distance(vertices[i], localHit);
//             //     if (dist > radius) continue;

//             //     float normalizedDist = dist / radius;
//             //     float falloff = Mathf.Pow(1f - normalizedDist, 2f);
//             //     Vector3 noisePos = vertices[i] * scale;

//             //     float noise = Mathf.PerlinNoise(noisePos.x, noisePos.z);
//             //     float displacement = intensity * noise * falloff;

//             //     vertices[i] += normals[i] * displacement;
//             // }

//             // mesh.vertices = vertices;
//             // mesh.RecalculateNormals();
//             // mesh.RecalculateBounds();
//             // EditorUtility.SetDirty(mesh);
//         }
//     }
// }
