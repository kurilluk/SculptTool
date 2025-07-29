using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    /// <summary>
    /// A sculpting brush that applies Perlin noise to the mesh surface,
    /// creating randomized, natural-looking height variation ("stamping").
    /// Noise is sampled in world-space (XZ) and modulated by a falloff curve.
    /// </summary>
    public class StampBrush : BrushBase
    {
        /// <summary>
        /// Display name of the brush shown in the brush selection UI.
        /// </summary>
        public override string Name => "Stamp (Perlin Noise)";

        /// <summary>
        /// Scale factor applied to vertex positions before sampling the Perlin noise function.
        /// Controls the frequency of the noise pattern.
        /// </summary>
        private float noiseScale = 0.5f;

        /// <summary>
        /// Falloff curve used to attenuate the noise effect from the center of the brush outward.
        /// </summary>
        private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// Renders the custom inspector UI for noise scaling and falloff.
        /// </summary>
        public override void GetGUI()
        {
            base.GetGUI();
            noiseScale = EditorGUILayout.Slider("Noise Scale", noiseScale, 0.1f, 10f);
            falloffCurve = EditorGUILayout.CurveField("Falloff Curve", falloffCurve);
        }

        /// <summary>
        /// Computes the falloff influence for a given normalized distance from the brush center.
        /// </summary>
        protected override float FalloffLogic(float normalizedDistance)
        {
            return falloffCurve.Evaluate(normalizedDistance);
        }

        /// <summary>
        /// Calculates the magnitude of displacement for a given vertex in the hit zone
        /// by sampling Perlin noise and applying falloff attenuation.
        /// </summary>
        protected override float CalculateMagnitude(int hitZoneIndex)
        {
            Vector3 noiseSamplePos = verticesBuffer[hitIndices[hitZoneIndex]] * noiseScale;
            float noise = Mathf.PerlinNoise(noiseSamplePos.x, noiseSamplePos.z);
            return noise * falloffValues[hitZoneIndex];
        }
    }
}
