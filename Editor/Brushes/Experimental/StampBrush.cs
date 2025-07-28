using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    public class StampBrush : BrushBase
    {
        public override string Name => "Stamp (Perlin Noise)";

        private float noiseScale = 0.5f;
        private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public override void GetGUI()
        {
            base.GetGUI();
            noiseScale = EditorGUILayout.Slider("Noise Scale", noiseScale, 0.1f, 10f);
            falloffCurve = EditorGUILayout.CurveField("Falloff Curve", falloffCurve);
        }

        protected override float FalloffLogic(float normalizedDistance)
        {
            return falloffCurve.Evaluate(normalizedDistance);
        }

        protected override float DisplacementMagnitude(int index)
        {
            Vector3 noiseSamplePos = verticesBuffer[hitIndices[index]] * noiseScale;
            float noise = Mathf.PerlinNoise(noiseSamplePos.x, noiseSamplePos.z);
            return noise * falloffValues[index];
        }
    }
}
