using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    /// <summary>
    /// Abstract base class for sculpting brushes in Unity's editor.
    /// Defines shared logic for handling input events, GUI, vertex displacement,
    /// and brush falloff behavior.
    /// </summary>
    public abstract class BrushBase : IBrush
    {
        /// <summary>
        /// Gets the display name of the brush. Must be implemented by derived classes.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Reference to the mesh manager for manipulating vertex data.
        /// </summary>
        protected MeshManager meshManager;

        /// <summary>
        /// Stores the result of the latest raycast hit during brush interaction.
        /// </summary>
        protected bool hasValidHit = false;
        protected RaycastHit lastHit;

        /// <summary>
        /// Mouse input flags for tracking user actions.
        /// </summary>
        private bool isMouseLeftClick = false;
        private bool isMouseLeftDrag = false;

        /// <summary>
        /// Buffers used for calculating affected vertices and brush effects.
        /// </summary>
        protected List<Vector3> verticesBuffer;
        protected readonly List<int> hitIndices = new();
        protected readonly List<float> falloffValues = new();
        protected readonly List<float> magnitudeValues = new();
        protected Vector3 GetAffectedVertex(int hitZoneIndex) => verticesBuffer[hitIndices[hitZoneIndex]];


        /// <summary>
        /// Brush shape settings (modifiable in GUI).
        /// </summary>
        protected float radius = 3f;
        private const float RadiusMin = 0.1f;
        private const float RadiusMax = 10f;
        protected float intensity = 0.05f;

        /// <summary>
        /// Defines the brush application direction.
        /// </summary>
        protected enum BrushDirection { Pull, Push }
        protected BrushDirection brushDirection;

        /// <summary>
        /// Internal state to manage Ctrl override for brush direction toggle.
        /// </summary>
        private bool wasCtrlPressed = false;
        private BrushDirection? savedBrushDirection = null;

        /// <summary>
        /// Called when the brush is enabled. Can be overridden for setup.
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// Called when the brush is disabled. Can be overridden for cleanup.
        /// </summary>
        public virtual void OnDisable() { }

        /// <summary>
        /// Draws GUI controls for brush settings inside the Unity Editor window.
        /// Can be overridden to add custom parameters.
        /// </summary>
        public virtual void GetGUI()
        {
            radius = EditorGUILayout.Slider("Radius", radius, RadiusMin, RadiusMax);
            intensity = EditorGUILayout.Slider("Intensity", intensity, 0.01f, 1f);
            brushDirection = (BrushDirection)EditorGUILayout.EnumPopup("Direction", brushDirection);
        }

        /// <summary>
        /// Handles Unity editor events relevant to brush input and behavior.
        /// </summary>
        /// <param name="e">Current event</param>
        /// <param name="mm">MeshManager reference</param>
        public void HandleEvent(Event e, MeshManager mm)
        {
            this.meshManager = mm;
            UpdateControlOverride(e);

            switch (e.type)
            {
                case EventType.Layout:
                    UpdateBrush(e);
                    ApplyMeshChanges();
                    break;

                case EventType.Repaint:
                    DrawBrushHandles();
                    break;

                case EventType.MouseDown:
                    if (e.button == 0) OnLeftMouseDown(e);
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0) OnLeftMouseDrag(e);
                    break;

                case EventType.MouseUp:
                    if (e.button == 0) OnLeftMouseUp(e);
                    break;

                case EventType.ScrollWheel:
                    HandleScrollWheel(e);
                    break;
            }
        }

        /// <summary>
        /// Toggles brush direction while Ctrl is held and restores it on release.
        /// </summary>
        private void UpdateControlOverride(Event e)
        {
            bool isCtrlNow = e.control;

            if (isCtrlNow && !wasCtrlPressed)
            {
                savedBrushDirection = brushDirection;
                brushDirection = (savedBrushDirection == BrushDirection.Push) ? BrushDirection.Pull : BrushDirection.Push;
            }
            else if (!isCtrlNow && wasCtrlPressed && savedBrushDirection.HasValue)
            {
                brushDirection = savedBrushDirection.Value;
                savedBrushDirection = null;
            }

            wasCtrlPressed = isCtrlNow;
        }

        /// <summary>
        /// Adjusts the brush radius using the mouse scroll wheel while Ctrl is held.
        /// </summary>
        private void HandleScrollWheel(Event e)
        {
            if (e.control)
            {
                radius -= e.delta.y * 0.1f;
                radius = Mathf.Clamp(radius, RadiusMin, RadiusMax);
                e.Use();
            }
        }

        /// <summary>
        /// Updates internal brush state during the layout phase.
        /// Computes hit location and affected vertices.
        /// </summary>
        private void UpdateBrush(Event e)
        {
            hasValidHit = TryGetMouseHit(e, out lastHit);

            if (!hasValidHit) return;

            verticesBuffer = meshManager.GetVerticesBuffer();
            ComputeHitZone(lastHit, verticesBuffer, radius);
            OnLayoutUpdate(e);
            ComputeMagnitudes();
        }

        /// <summary>
        /// Optional hook for custom brush logic during layout update.
        /// </summary>
        protected virtual void OnLayoutUpdate(Event e) { }

        /// <summary>
        /// Recalculates magnitude values for affected vertices.
        /// </summary>
        private void ComputeMagnitudes()
        {
            magnitudeValues.Clear();
            for (int i = 0; i < hitIndices.Count; i++)
                magnitudeValues.Add(CalculateMagnitude(i));
        }

        /// <summary>
        /// Performs a raycast into the scene from the current mouse position.
        /// </summary>
        private bool TryGetMouseHit(Event e, out RaycastHit hit)
        {
            hit = default;

            if (meshManager?.Collider == null)
                return false;

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            return meshManager.Collider.Raycast(ray, out hit, Mathf.Infinity);
        }

        /// <summary>
        /// Computes which vertices fall within the brush radius and applies falloff.
        /// </summary>
        private void ComputeHitZone(RaycastHit hit, List<Vector3> meshVertices, float radius)
        {
            Vector3 localHitPoint = hit.collider.transform.InverseTransformPoint(hit.point);

            hitIndices.Clear();
            falloffValues.Clear();

            float sqrRadius = radius * radius;
            for (int i = 0; i < meshVertices.Count; i++)
            {
                float sqrDistance = (meshVertices[i] - localHitPoint).sqrMagnitude;
                if (sqrDistance <= sqrRadius)
                {
                    hitIndices.Add(i);
                    float t = Mathf.Clamp01(sqrDistance / sqrRadius);
                    falloffValues.Add(FalloffLogic(t));
                }
            }
        }

        /// <summary>
        /// Calculates a falloff weight based on normalized distance from brush center.
        /// Can be overridden to change the falloff curve (e.g. linear, smoothstep, etc.).
        /// </summary>
        protected virtual float FalloffLogic(float normalizedDistance)
        {
            return normalizedDistance;
        }

        /// <summary>
        /// Returns the direction of displacement for a given vertex.
        /// Default is Vector3.up.
        /// </summary>
        protected virtual Vector3 GetDisplacementDirection(int hitZoneIndex)
        {
            return Vector3.up;
        }

        /// <summary>
        /// Calculates the displacement magnitude for a vertex in the affected zone.
        /// This uses the <paramref name="hitZoneIndex"/> to access precomputed data
        /// (like falloff, delta, etc.) for that vertex.
        /// </summary>
        /// <param name="hitZoneIndex">
        /// Index into hit zone arrays (e.g., hitIndices, falloffValues, etc.), 
        /// not a direct vertex buffer index.
        /// </param>
        protected virtual float CalculateMagnitude(int hitZoneIndex)
        {
            return falloffValues[hitZoneIndex];
        }

        /// <summary>
        /// Applies displacement to affected vertices based on brush logic and user input.
        /// </summary>
        private void ApplyMeshChanges()
        {
            if (meshManager == null || !hasValidHit) return;
            if (!isMouseLeftClick && !isMouseLeftDrag) return;

            for (int i = 0; i < hitIndices.Count; i++)
            {
                Vector3 displacement = GetDisplacementDirection(i) * magnitudeValues[i] * intensity;
                verticesBuffer[hitIndices[i]] += displacement * ((brushDirection == BrushDirection.Push) ? -1f : 1f);
            }

            meshManager.ApplyVerticesBuffer();

            isMouseLeftClick = false;
            isMouseLeftDrag = false;
        }

        /// <summary>
        /// Draws visual indicators for the brush, such as spheres and radius disc.
        /// </summary>
        protected virtual void DrawBrushHandles()
        {
            if (!hasValidHit) return;

            var meshVertices = meshManager.GetVerticesBuffer();

            for (int i = 0; i < hitIndices.Count; i++)
            {
                float size = Mathf.Lerp(0.1f, 0.2f, Mathf.Abs(magnitudeValues[i]));
                Vector3 worldVertex = lastHit.transform.TransformPoint(meshVertices[hitIndices[i]]);
                Color color = (brushDirection == BrushDirection.Push || magnitudeValues[i] < 0f) ? Color.red : Color.green;

                Handles.color = Color.Lerp(Color.black, color, falloffValues[i]);
                Handles.SphereHandleCap(0, worldVertex, Quaternion.identity, size, EventType.Repaint);
            }

            Handles.color = new Color(1f, 1f, 0f, 0.3f);
            Handles.DrawWireDisc(lastHit.point, lastHit.normal, radius);
        }

        /// <summary>
        /// Called when the left mouse button is pressed.
        /// </summary>
        protected virtual void OnLeftMouseDown(Event e)
        {
            isMouseLeftClick = true;
            e.Use();
        }

        /// <summary>
        /// Called during left mouse drag (holding + moving).
        /// </summary>
        protected virtual void OnLeftMouseDrag(Event e)
        {
            isMouseLeftDrag = true;
            e.Use();
        }

        /// <summary>
        /// Called when the left mouse button is released.
        /// </summary>
        protected virtual void OnLeftMouseUp(Event e)
        {
            isMouseLeftClick = false;
            isMouseLeftDrag = false;
            e.Use();
        }
    }
}
