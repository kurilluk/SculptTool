using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    public class FlattenBrush : BrushBase
    {
        public override string Name => "Flatten Brush";

        private AnimationCurve falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

        private List<float> deltaValues = new ();

        private float deltaThreshold = 0.01f;


        public override void GetGUI()
        {
            base.GetGUI();
            falloffCurve = EditorGUILayout.CurveField("Falloff", falloffCurve);
            deltaThreshold = EditorGUILayout.FloatField("Precision Threshold", deltaThreshold);
        }

        protected override float FalloffLogic(float normalizedDistance)
        {
            return falloffCurve.Evaluate(normalizedDistance);
        }

        protected override float DisplacementMagnitude(int index)
        {
            if (verticesBuffer == null || hitIndices == null || deltaValues?.Count == 0)
                return 0f;

        //    // float falloff = falloffCurve.Evaluate(normalizedDistance);

        //     // Výška aktuálneho vertexu
        //     float currentY = verticesBuffer[hitIndices[index]].y;

        //     // Priemerná výška vypočítaná raz pre celý buffer
        //     float averageY = GetAverageHeight();

        //     // Rozdiel medzi aktuálnou a cieľovou výškou
        //     float delta = averageY - currentY;

        //     // Falloff + intensity aplikované
            return deltaValues[index] * falloffValues[index];
        }

        private float? cachedAverageY = null;

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

        private void Reset()
        {
            cachedAverageY = null;
        }

        // Reset cached value na nový frame
        // public override void OnEnable()
        // {
        //     base.OnEnable();
        //     ResetAverageHeight();
        // }

        protected override void OnLayoutUpdate(Event e)
        {
            if (!hasValidHit) return;
            NormalizeDeltaDistance();
        }

        // protected override void OnLeftMouseDrag(Event e)
        // {
        //     base.OnLeftMouseDrag(e);
        //     ResetAverageHeight();
        // }
    }
}
