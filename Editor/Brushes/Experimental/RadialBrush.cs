// using UnityEngine;
// using UnityEditor;

// namespace SculptTool
// {
//     public class RadialBrush //: IBrush
//     {
//         public string Name => "Radial Brush";

//         private float radius = 0.5f;
//         private float intensity = 0.1f;
//         private Vector3[] previewVertices;

//         public void GetGUI()
//         {
//             EditorGUILayout.LabelField("Radial Brush Settings", EditorStyles.boldLabel);
//             radius = EditorGUILayout.Slider("Radius", radius, 0.01f, 2f);
//             intensity = EditorGUILayout.Slider("Intensity", intensity, -1f, 1f);
//         }

//         public Vector3[] Preview(MeshManager mm, RaycastHit hit)
//         {
//             Transform tf = mm.TargetTransform;

//             if (tf == null)
//                 return null;

//             Vector3[] originalVertices = mm.GetVertices();
//             Vector3[] normals = mm.GetNormals();

//             if (originalVertices == null || normals == null)
//                 return null;

//             previewVertices = new Vector3[originalVertices.Length];
//             originalVertices.CopyTo(previewVertices, 0);

//             Vector3 localHitPoint = tf.InverseTransformPoint(hit.point);
//             bool anyAffected = false;

//             for (int i = 0; i < originalVertices.Length; i++)
//             {
//                 float dist = Vector3.Distance(originalVertices[i], localHitPoint);
//                 if (dist > radius) continue;

//                 anyAffected = true;
//                 float falloff = Mathf.Pow(1f - (dist / radius), 2f);
//                 float displacement = falloff * intensity;

//                 Vector3 worldPos = tf.TransformPoint(originalVertices[i]);
//                 Vector3 normalDir = tf.TransformDirection(normals[i]);

//                 // vizualizácia posunu
//                 Handles.color = (displacement >= 0f) ? Color.green : Color.red;
//                 Handles.ArrowHandleCap(0, worldPos + normalDir * displacement,
//                 Quaternion.identity, Mathf.Abs(displacement) * 0.5f, EventType.Repaint);

//                 previewVertices[i] = originalVertices[i] + normals[i] * displacement;
//             }

//             return anyAffected ? previewVertices : null;
//         }

//         public void ApplyBrush(MeshManager mm)
//         {
//             if (previewVertices == null)
//                 return;

//             Undo.RegisterCompleteObjectUndo(mm.MeshInstance, "Apply RadialBrush");

//             mm.ApplyVertices(previewVertices);

//             previewVertices = null;
//         }
//     }
// }
