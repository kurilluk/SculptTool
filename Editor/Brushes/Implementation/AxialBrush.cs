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
    }
}
