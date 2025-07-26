// using UnityEngine;
// using System.Collections.Generic;

// namespace SculptTool
// {
//     public class InflateBrush : BrushBase
//     {
//         public override string Name => "Inflate Brush";

//         // private readonly var vertexBuffer = new();
//         private readonly List<int> affectedIndices = new();

//         protected override void OnLayoutEvent(Event e)
//         {
//             if (meshManager == null) return;

//             // vertexBuffer.Clear();
//             affectedIndices.Clear();

//             //meshManager.GetVertexBuffer().CopyTo(vertexBuffer);
//             var vertexBuffer = meshManager.GetVertexBuffer();

//             float sqrRadius = radius * radius;
//             for (int i = 0; i < vertexBuffer.Count; i++)
//             {
//                 float sqrDistance = (vertexBuffer[i] - hitPosition).sqrMagnitude;
//                 if (sqrDistance < sqrRadius)
//                 {
//                     affectedIndices.Add(i);
//                 }
//             }
//         }

//         protected override void OnBrushDrag(Event e)
//         {
//             Vector3 direction = Vector3.up; // Nahradiť priemerným normálom, ak treba
//             var vertexBuffer = meshManager.GetVertexBuffer();

//             foreach (int i in affectedIndices)
//             {
//                 vertexBuffer[i] += direction * strength;
//             }

//             meshManager.ApplyVertexBuffer();
//         }
//     }
// }
