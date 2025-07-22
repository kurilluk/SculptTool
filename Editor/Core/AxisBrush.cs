using UnityEngine;
using UnityEditor;

namespace SculptMode
{
    public class AxisBrush : IBrush
    {
        public string Name => "Axis Brush";

        private float radius = 0.5f;
        private float intensity = 0.1f;
        private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        private Axis selectedAxis = Axis.Y;

        private Vector3[] previewVertices;

        private enum Axis { X, Y, Z }

        public void GetGUI()
        {
            EditorGUILayout.LabelField("Axis Brush Settings", EditorStyles.boldLabel);
            radius = EditorGUILayout.Slider("Radius", radius, 0.01f, 2f);
            intensity = EditorGUILayout.Slider("Intensity", intensity, -1f, 1f);
            selectedAxis = (Axis)EditorGUILayout.EnumPopup("Direction Axis", selectedAxis);
            falloffCurve = EditorGUILayout.CurveField("Falloff", falloffCurve);
        }

        public Vector3[] Preview(MeshManager mm, RaycastHit hit)
        {
            // Read data from MeshManager
            Transform tf = mm.TargetTransform;
            Vector3[] originalVertices = mm.GetVertices();

            if (originalVertices == null || tf == null)
                return null;

            // Init copy of previewVertices (limit number of allocations)
            if (previewVertices == null || previewVertices.Length != originalVertices.Length)
            {
                previewVertices = new Vector3[originalVertices.Length];
            }
            System.Array.Copy(originalVertices, previewVertices, originalVertices.Length);

            // Vector3 localHitPoint = tf.InverseTransformPoint(hit.point);
            Vector3 axisDirection = GetAxisVector(selectedAxis).normalized;

            bool anyAffected = false;

            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 worldVertex = tf.TransformPoint(originalVertices[i]);
                float dist = Vector3.Distance(worldVertex, hit.point);
                if (dist > radius) continue;

                float t = Mathf.Clamp01(dist / radius);
                float falloff = falloffCurve.Evaluate(t);
                float displacement = falloff * intensity;

                anyAffected = true;

                Vector3 offset = axisDirection * displacement;

                // World space
                // Vector3 worldOrigin = tf.TransformPoint(originalVertices[i]);
                Vector3 worldAxis = tf.TransformDirection(axisDirection);

                // Gradient color
                Color color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(1f, -1f, displacement));
                Handles.color = color;

                // Draw cylinder: base at original vertex, height = |displacement|, radius = falloff-based
                float cylinderHeight = Mathf.Abs(displacement);
                float cylinderRadius = Mathf.Lerp(0.05f, 0.2f, falloff); // tweak scale if needed

                // Cylinder center: halfway along the direction vector
                Vector3 cylinderCenter = worldVertex; //+ worldAxis * (cylinderHeight / 2f);

                // Rotation to align with axis
                Quaternion rotation = Quaternion.LookRotation(worldAxis);

                // Uložíme pôvodnú maticu
                Matrix4x4 oldMatrix = Handles.matrix;

                // Nastavíme vlastnú TRS transformáciu s rôznou mierkou pre každú os
                Handles.matrix = Matrix4x4.TRS(
                    cylinderCenter,
                    Quaternion.identity,
                    new Vector3(1.0f, 1.0f, 1.0f) // X/Z normálne, Y = výška šípky (menšia)
                );

                Handles.SphereHandleCap(0, Vector3.zero, rotation, cylinderRadius, EventType.Repaint);


                // Obnovíme pôvodnú maticu
                Handles.matrix = oldMatrix;

                // Apply displacement
                previewVertices[i] = originalVertices[i] + offset;
            }

            // Brush radius circle
            Handles.color = new Color(1f, 1f, 0f, 0.3f);
            Handles.DrawWireDisc(hit.point, hit.normal, radius);

            return anyAffected ? previewVertices : null;
        }


        public void ApplyBrush(MeshManager mm)
        {
            if (previewVertices == null)
                return;

            Undo.RegisterCompleteObjectUndo(mm.MeshInstance, "Apply Axis Brush");
            mm.ApplyVertices(previewVertices);
            previewVertices = null;
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
    }
}
