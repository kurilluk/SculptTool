using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    /// <summary>
    /// A brush that flattens terrain or geometry by pulling vertices toward an average height level.
    /// The amount of displacement is based on each vertex’s vertical distance from the average,
    /// modulated by a falloff curve and normalized delta strength.
    /// </summary>
    public class FlattenBrush : BrushBase
    {
        /// <summary>
        /// Display name of the brush shown in the UI.
        /// </summary>
        public override string Name => "Flatten Brush";

        /// <summary>
        /// Curve defining the falloff from the center of the brush to the edges.
        /// </summary>
        private AnimationCurve falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

        /// <summary>
        /// Precomputed delta distances for affected vertices, normalized after computing.
        /// </summary>
        private List<float> deltaValues = new();

        /// <summary>
        /// Minimum delta value under which vertex displacement is ignored.
        /// Used to avoid micro-adjustments that are visually irrelevant.
        /// </summary>
        private float deltaThreshold = 0.01f;

        /// <summary>
        /// Renders GUI controls for falloff curve and precision threshold.
        /// </summary>
        public override void GetGUI()
        {
            base.GetGUI();
            falloffCurve = EditorGUILayout.CurveField("Falloff", falloffCurve);
            deltaThreshold = EditorGUILayout.FloatField("Precision Threshold", deltaThreshold);
        }

        /// <summary>
        /// Evaluates the falloff influence using the current falloff curve.
        /// </summary>
        protected override float FalloffLogic(float normalizedDistance)
        {
            return falloffCurve.Evaluate(normalizedDistance);
        }

        /// <summary>
        /// Calculates how much a vertex should be displaced based on its distance
        /// from the average height and the falloff value.
        /// </summary>
        protected override float CalculateMagnitude(int hitZoneIndex)
        {
            if (verticesBuffer == null || hitIndices == null || deltaValues?.Count == 0)
                return 0f;

            return deltaValues[hitZoneIndex] * falloffValues[hitZoneIndex];
        }

        /// <summary>
        /// Cached average Y height of all affected vertices. Reset every frame.
        /// </summary>
        private float? cachedAverageY = null;

        /// <summary>
        /// Calculates and caches the average Y value (height) of affected vertices.
        /// </summary>
        private float GetAverageHeight()
        {
            if (cachedAverageY.HasValue)
                return cachedAverageY.Value;

            float sum = 0f;

            for (int i = 0; i < hitIndices.Count; i++)
                sum += verticesBuffer[hitIndices[i]].y;

            float avg = hitIndices.Count > 0 ? sum / hitIndices.Count : 0f;
            cachedAverageY = avg;
            return avg;
        }

        /// <summary>
        /// Computes and normalizes the height difference of each affected vertex relative to the average height.
        /// Applies the delta threshold to avoid insignificant displacements.
        /// </summary>
        private void NormalizeDeltaDistance()
        {
            deltaValues.Clear();
            float maxAbsDelta = 0f;
            cachedAverageY = null;

            for (int i = 0; i < hitIndices.Count; i++)
            {
                float delta = GetAverageHeight() - verticesBuffer[hitIndices[i]].y;
                if (Mathf.Abs(delta) < deltaThreshold)
                    delta = 0f;
                deltaValues.Add(delta);
                maxAbsDelta = Mathf.Max(maxAbsDelta, Mathf.Abs(delta));
            }

            for (int i = 0; i < hitIndices.Count; i++)
            {
                if (maxAbsDelta > 0f && Mathf.Abs(deltaValues[i]) > deltaThreshold)
                    deltaValues[i] /= maxAbsDelta;
                else
                    deltaValues[i] = 0f;
            }
        }

        /// <summary>
        /// Clears cached values. Called at the beginning of each frame.
        /// </summary>
        private void Reset()
        {
            cachedAverageY = null;
        }

        /// <summary>
        /// Called during layout update to recalculate vertex deltas before sculpting begins.
        /// </summary>
        protected override void OnLayoutUpdate(Event e)
        {
            if (!hasValidHit) return;
            NormalizeDeltaDistance();
        }
    }
}
