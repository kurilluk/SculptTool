// public class LockingBrush : AbstractBrush
// {
//     public override string Name => "Locking Brush";

//     private List<int> lockedIndices = new();
//     private Vector3[] originalPositions;

//     private BrushParameter<float> strengthParam = new("Strength", 1.0f, 0f, 5f);

//     private bool inverseDirection;

//     public override void DrawGUI()
//     {
//         strengthParam.DrawSlider();
//     }

//     protected override void OnMouseDown(Event e, MeshManager meshManager)
//     {
//         Mesh mesh = meshManager.MeshInstance;
//         Transform tf = meshManager.TargetTransform;

//         lockedIndices = FindVerticesNear(mesh, tf, currentHit.point, 0.1f);
//         originalPositions = mesh.vertices.Clone() as Vector3[];
//         inverseDirection = e.control;
//     }

//     protected override void OnMouseDrag(Event e, MeshManager meshManager)
//     {
//         float deltaY = e.mousePosition.y - startMousePos.y;
//         float strength = Mathf.Clamp01(deltaY / 100f);
//         if (inverseDirection) strength *= -1f;

//         Vector3 offset = currentHit.normal * strength * strengthParam.Value;

//         Mesh mesh = meshManager.MeshInstance;
//         Vector3[] verts = mesh.vertices;

//         for (int i = 0; i < lockedIndices.Count; i++)
//         {
//             int index = lockedIndices[i];
//             verts[index] = originalPositions[index] + offset;
//         }

//         mesh.vertices = verts;
//         mesh.RecalculateNormals();
//         mesh.RecalculateBounds();
//     }

//     protected override void OnMouseUp(Event e, MeshManager meshManager)
//     {
//         meshManager.RegisterUndo("Locking Brush Stroke", originalPositions);
//     }

//     protected override void OnScroll(Event e)
//     {
//         strengthParam.HandleScroll(e);
//         EditorWindow.focusedWindow?.Repaint();
//     }

//     protected override void OnDrawGizmos(Mesh mesh, Transform tf)
//     {
//         if (lockedIndices.Count == 0) return;

//         Handles.color = Color.cyan;
//         foreach (int i in lockedIndices)
//         {
//             Vector3 worldPos = tf.TransformPoint(mesh.vertices[i]);
//             Handles.DotHandleCap(0, worldPos, Quaternion.identity, 0.02f, EventType.Repaint);
//         }
//     }

//     private List<int> FindVerticesNear(Mesh mesh, Transform tf, Vector3 point, float radius)
//     {
//         var result = new List<int>();
//         Vector3[] verts = mesh.vertices;
//         for (int i = 0; i < verts.Length; i++)
//         {
//             if (Vector3.Distance(tf.TransformPoint(verts[i]), point) < radius)
//                 result.Add(i);
//         }
//         return result;
//     }
// }
