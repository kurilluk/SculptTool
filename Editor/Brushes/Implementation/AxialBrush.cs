using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    public class AxialBrush : BrushBase
    {
        public override string Name => "Axial Brush";

        private float radius = 3f;
        private float intensity = 0.05f;
        private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        private Axis selectedAxis = Axis.Y;

        private List<int> lockedVertexIndices = new();
        private Vector3[] originalPositions;
        private Vector2 startMousePos;
        private bool dragging = false;
        private bool shiftDirection = false;

        private Vector3[] previewVertices;

        private enum Axis { X, Y, Z }

        public override void GetGUI()
        {
            EditorGUILayout.LabelField("Axial Brush Settings", EditorStyles.boldLabel);
            radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 10f);
            intensity = EditorGUILayout.Slider("Intensity", intensity, -1f, 1f);
            selectedAxis = (Axis)EditorGUILayout.EnumPopup("Direction Axis", selectedAxis);
            falloffCurve = EditorGUILayout.CurveField("Falloff", falloffCurve);
        }

        protected override void BrushLogic(RaycastHit hit, MeshManager mm)
        {
            if (!hasValidHit || mm == null) return;

            var meshVertices = mm.GetVerticesBuffer();

            //result: init - hitIndices and weightValues
            CalculateHitZone(hit, meshVertices, radius);

        }

        protected override void UpdateMesh()
        {
            if (!base.isMouseLeftClick || base.meshManager == null || !hasValidHit) return;

            var meshVertices = this.meshManager.GetVerticesBuffer();

            Vector3 axisDirection = GetAxisVector(selectedAxis).normalized;
            // Vector3 worldAxis = lastHit.collider.transform.TransformDirection(axisDirection);

            for (int i = 0; i < hitIndices.Count; i++)
            {
                float displacement = weightValues[i] * intensity;
                Vector3 offset = axisDirection * displacement;
                meshVertices[hitIndices[i]] += offset;
            }

            base.meshManager.ApplyVerticesBuffer();
        }

        private void CalculateHitZone(RaycastHit hit, List<Vector3> meshVertices, float radius)
        {
            Vector3 localHitPoint = hit.collider.transform.InverseTransformPoint(hit.point);
            // Vector3 localHitPoint = tf.InverseTransformPoint(hit.point);

            hitIndices.Clear();
            weightValues.Clear();

            // NOTE: I suggest that: Capacity should not change between different runs
            //hitIndices.Capacity = Mathf.Max(hitIndices.Capacity, meshVertices.Count / 4);
            //sqrDistances.Capacity = Mathf.Max(hitIndices.Capacity, meshVertices.Count / 4);


            float sqrRadius = radius * radius;
            for (int i = 0; i < meshVertices.Count; i++)
            {
                float sqrDistance = (meshVertices[i] - localHitPoint).sqrMagnitude;
                if (sqrDistance <= sqrRadius)
                {
                    hitIndices.Add(i);
                    float t = Mathf.Clamp01(sqrDistance / sqrRadius);
                    float weight = falloffCurve.Evaluate(t);
                    weightValues.Add(weight);
                }
            }

        }

        protected override void DrawHandles()
        {
            if (!hasValidHit) return;

            var meshVertices = this.meshManager.GetVerticesBuffer();

            // Rotation to align with axis
            // Quaternion rotation = Quaternion.LookRotation(worldAxis);

            for (int i = 0; i < hitIndices.Count; i++)
            {
                // Hit vertices and weight
                float size = Mathf.Lerp(0.05f, 0.2f, weightValues[i]);
                Vector3 worldVertex = lastHit.transform.TransformPoint(meshVertices[hitIndices[i]]);
                // if intensity is negative change green to red
                Color color = Color.green;
                if (intensity < 0) color = Color.red;

                Handles.color = Color.Lerp(Color.black, color, weightValues[i]); //Color.cyan;
                Handles.SphereHandleCap(0, worldVertex, Quaternion.identity, size, EventType.Repaint);

                // Brush radius circle
                Handles.color = new Color(1f, 1f, 0f, 0.3f);
                Handles.DrawWireDisc(lastHit.point, lastHit.normal, radius);
            }
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

        // public void HandleEvent(Event e, RaycastHit hit, MeshManager meshManager)
        // {
        //     Mesh mesh = meshManager.MeshInstance;
        //     Transform tf = meshManager.TargetTransform;
        //     Vector3[] vertices = mesh.vertices;

        //     switch (e.type)
        //     {
        //         case EventType.MouseDown when e.button == 0 && !e.alt:
        //             // Uzamkni vertexy blízko kurzoru
        //             lockedVertexIndices = FindVerticesNear(mesh, tf, hit.point, radius: 0.1f);
        //             originalPositions = mesh.vertices.Clone() as Vector3[];
        //             startMousePos = e.mousePosition;
        //             dragging = true;
        //             shiftDirection = e.control; // Ctrl prepína smer
        //             e.Use();
        //             break;

        //         case EventType.MouseDrag when dragging:
        //             float deltaY = e.mousePosition.y - startMousePos.y;
        //             float strength = Mathf.Clamp01(deltaY / 100f); // 0..1 intenzita

        //             Vector3 offset = hit.normal * strength * (shiftDirection ? -1f : 1f);

        //             foreach (int i in lockedVertexIndices)
        //             {
        //                 vertices[i] = originalPositions[i] + offset;
        //             }

        //             mesh.vertices = vertices;
        //             mesh.RecalculateNormals();
        //             mesh.RecalculateBounds();

        //             e.Use();
        //             break;

        //         case EventType.MouseUp when dragging:
        //             dragging = false;

        //             // TODO: UNDO system – registruj stav tu
        //             // meshManager.RegisterUndo("LockingBrush Stroke", originalPositions);

        //             e.Use();
        //             break;
        //     }
        // }



        // public Vector3[] Preview(MeshManager mm, RaycastHit hit)
        // {
        //     // Read data from MeshManager
        //     Transform tf = mm.TargetTransform;
        //     Vector3[] originalVertices = mm.GetVertices();

        //     if (originalVertices == null || tf == null)
        //         return null;

        //     // Init copy of previewVertices (limit number of allocations)
        //     if (previewVertices == null || previewVertices.Length != originalVertices.Length)
        //     {
        //         previewVertices = new Vector3[originalVertices.Length];
        //     }
        //     System.Array.Copy(originalVertices, previewVertices, originalVertices.Length);

        //     // Vector3 localHitPoint = tf.InverseTransformPoint(hit.point);
        //     Vector3 axisDirection = GetAxisVector(selectedAxis).normalized;

        //     bool anyAffected = false;

        //     for (int i = 0; i < originalVertices.Length; i++)
        //     {
        //         Vector3 worldVertex = tf.TransformPoint(originalVertices[i]);
        //         float dist = Vector3.Distance(worldVertex, hit.point);
        //         if (dist > radius) continue;

        //         float t = Mathf.Clamp01(dist / radius);
        //         float falloff = falloffCurve.Evaluate(t);
        //         float displacement = falloff * intensity;

        //         anyAffected = true;

        //         Vector3 offset = axisDirection * displacement;

        //         // World space
        //         // Vector3 worldOrigin = tf.TransformPoint(originalVertices[i]);
        //         Vector3 worldAxis = tf.TransformDirection(axisDirection);

        //         // Gradient color
        //         Color color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(1f, -1f, displacement));
        //         Handles.color = color;

        //         // Draw cylinder: base at original vertex, height = |displacement|, radius = falloff-based
        //         float cylinderHeight = Mathf.Abs(displacement);
        //         float cylinderRadius = Mathf.Lerp(0.05f, 0.2f, falloff); // tweak scale if needed

        //         // Cylinder center: halfway along the direction vector
        //         Vector3 cylinderCenter = worldVertex; //+ worldAxis * (cylinderHeight / 2f);

        //         // Rotation to align with axis
        //         Quaternion rotation = Quaternion.LookRotation(worldAxis);

        //         // Uložíme pôvodnú maticu
        //         Matrix4x4 oldMatrix = Handles.matrix;

        //         // Nastavíme vlastnú TRS transformáciu s rôznou mierkou pre každú os
        //         Handles.matrix = Matrix4x4.TRS(
        //             cylinderCenter,
        //             Quaternion.identity,
        //             new Vector3(1.0f, 1.0f, 1.0f) // X/Z normálne, Y = výška šípky (menšia)
        //         );

        //         Handles.SphereHandleCap(0, Vector3.zero, rotation, cylinderRadius, EventType.Repaint);


        //         // Obnovíme pôvodnú maticu
        //         Handles.matrix = oldMatrix;

        //         // Apply displacement
        //         previewVertices[i] = originalVertices[i] + offset;
        //     }

        //     // Brush radius circle
        //     Handles.color = new Color(1f, 1f, 0f, 0.3f);
        //     Handles.DrawWireDisc(hit.point, hit.normal, radius);

        //     return anyAffected ? previewVertices : null;
        // }

        // public void ApplyBrush(MeshManager mm)
        // {
        //     if (previewVertices == null)
        //         return;

        //     Undo.RegisterCompleteObjectUndo(mm.MeshInstance, "Apply Axial Brush");
        //     mm.ApplyVertices(previewVertices);
        //     previewVertices = null;
        // }

    }
}
