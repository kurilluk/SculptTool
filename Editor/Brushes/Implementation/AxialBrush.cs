using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    public class AxialBrush : BrushBase
    {
        public override string Name => "Axial Brush";

        private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        private Axis selectedAxis = Axis.Y;

        // private List<int> lockedVertexIndices = new();
        // private Vector3[] originalPositions;
        // private Vector2 startMousePos;
        // private bool dragging = false;
        // private bool shiftDirection = false;
        // private Vector3[] previewVertices;

        private enum Axis { X, Y, Z }

        public override void GetGUI()
        {
            base.GetGUI();

            selectedAxis = (Axis)EditorGUILayout.EnumPopup("Direction Axis", selectedAxis);
            falloffCurve = EditorGUILayout.CurveField("Falloff", falloffCurve);
        }

        protected override float FalloffLogic(float normalizedDistance)
        {
            return falloffCurve.Evaluate(normalizedDistance);
        }

        protected override Vector3 DisplacementDirection(int index)
        {
            return GetAxisVector(selectedAxis).normalized;
        }

        private Vector3 GetAxisVector(Axis axis)
        {
            return axis switch
            {
                Axis.X => Vector3.right,
                Axis.Y => Vector3.up,
                Axis.Z => Vector3.forward,
                _ => Vector3.up
            };
        }

        // protected override void BrushLogic(RaycastHit hit, MeshManager mm)
        // {
        //     if (!hasValidHit || mm == null) return;

        //     base.verticesBuffer = mm.GetVerticesBuffer();

        //     //result: init - hitIndices and weightValues
        //     CalculateHitZone(hit, verticesBuffer, radius);

        // }

        // protected override void UpdateMesh()
        // { }

        // private void CalculateHitZone(RaycastHit hit, List<Vector3> meshVertices, float radius)
        // {
        //     Vector3 localHitPoint = hit.collider.transform.InverseTransformPoint(hit.point);
        //     // Vector3 localHitPoint = tf.InverseTransformPoint(hit.point);

        //     hitIndices.Clear();
        //     weightValues.Clear();

        //     // NOTE: I suggest that: Capacity should not change between different runs
        //     //hitIndices.Capacity = Mathf.Max(hitIndices.Capacity, meshVertices.Count / 4);
        //     //sqrDistances.Capacity = Mathf.Max(hitIndices.Capacity, meshVertices.Count / 4);


        //     float sqrRadius = radius * radius;
        //     for (int i = 0; i < meshVertices.Count; i++)
        //     {
        //         float sqrDistance = (meshVertices[i] - localHitPoint).sqrMagnitude;
        //         if (sqrDistance <= sqrRadius)
        //         {
        //             hitIndices.Add(i);
        //             float t = Mathf.Clamp01(sqrDistance / sqrRadius);
        //             float weight = falloffCurve.Evaluate(t);
        //             weightValues.Add(weight);
        //         }
        //     }
        // }

        // protected override void DrawHandles()
        // {
        //     if (!hasValidHit) return;

        //     var meshVertices = this.meshManager.GetVerticesBuffer();

        //     // Rotation to align with axis
        //     // Quaternion rotation = Quaternion.LookRotation(worldAxis);

        //     for (int i = 0; i < hitIndices.Count; i++)
        //     {
        //         // Hit vertices and weight
        //         float size = Mathf.Lerp(0.05f, 0.2f, weightValues[i]);
        //         Vector3 worldVertex = lastHit.transform.TransformPoint(meshVertices[hitIndices[i]]);
        //         // if intensity is negative change green to red
        //         Color color = Color.green;
        //         if (intensity < 0) color = Color.red;

        //         Handles.color = Color.Lerp(Color.black, color, weightValues[i]); //Color.cyan;
        //         Handles.SphereHandleCap(0, worldVertex, Quaternion.identity, size, EventType.Repaint);

        //         // Brush radius circle
        //         Handles.color = new Color(1f, 1f, 0f, 0.3f);
        //         Handles.DrawWireDisc(lastHit.point, lastHit.normal, radius);
        //     }
        // }
    }
}
