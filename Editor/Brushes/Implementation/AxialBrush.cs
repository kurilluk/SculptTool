using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    /// <summary>
    /// A brush that displaces vertices along a selected world axis (X, Y, or Z).
    /// Uses a customizable falloff curve to control the intensity of deformation across the brush radius.
    /// </summary>
    public class AxialBrush : BrushBase
    {
        /// <summary>
        /// Display name of the brush shown in the UI.
        /// </summary>
        public override string Name => "Axial Brush";

        /// <summary>
        /// Curve defining the falloff strength from brush center to its edge.
        /// </summary>
        private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// Enumeration of available displacement directions.
        /// </summary>
        private enum Axis { X, Y, Z }

        /// <summary>
        /// User-selected axis along which vertices will be displaced.
        /// </summary>
        private Axis selectedAxis = Axis.Y;

        /// <summary>
        /// Displays GUI controls specific to the AxialBrush, including axis selection and falloff curve.
        /// </summary>
        public override void GetGUI()
        {
            base.GetGUI();
            selectedAxis = (Axis)EditorGUILayout.EnumPopup("Direction Axis", selectedAxis);
            falloffCurve = EditorGUILayout.CurveField("Falloff", falloffCurve);
        }

        /// <summary>
        /// Applies the selected falloff curve to modulate vertex influence.
        /// </summary>
        protected override float FalloffLogic(float normalizedDistance)
        {
            return falloffCurve.Evaluate(normalizedDistance);
        }

        /// <summary>
        /// Returns a world-space direction vector based on the selected axis.
        /// </summary>
        protected override Vector3 GetDisplacementDirection(int hitZoneIndex)
        {
            return GetAxisVector(selectedAxis).normalized;
        }

        /// <summary>
        /// Converts the selected axis enum into a corresponding Vector3 direction.
        /// </summary>
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
