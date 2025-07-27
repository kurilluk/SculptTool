using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    /// <summary>
    /// Base abstract class for sculpting brushes.
    /// Requires derived classes to provide a name.
    /// Provides a virtual GUI method and hides event handling from public API.
    /// </summary>
    public abstract class BrushBase : IBrush
    {
        /// <summary>
        /// Gets the display name of the brush.
        /// Must be implemented by derived classes.
        /// </summary>
        public abstract string Name { get; }

        protected MeshManager meshManager;

        // Aktuálny Raycast hit - aktualizovaný v každom Layout evente
        // protected RaycastHit? lastHit;
        protected bool hasValidHit = false;
        protected RaycastHit lastHit;

        protected bool isMouseLeftClick = false; 
        protected bool isMouseLeftDrag = false;

        // Výsledky výpočtov - napriklad vzdialenosti k vrcholom
        // protected Vector3[] affectedVertices;
        // protected Vector3[] displacementVectors;  // NIE JE LEPSIE AKO LIST a ADD ak je potrebne?

        protected List<Vector3> verticesBuffer;
        protected readonly List<int> hitIndices = new();
        protected readonly List<float> weightValues = new();

        // GUI FIELD
        private float radius = 3f;
        private float intensity = 0.05f;

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        /// <summary>
        /// Draws custom GUI controls for the brush inside the editor window.
        /// Can be overridden by derived classes.
        /// Default implementation does nothing.
        /// </summary>
        public virtual void GetGUI()
        {
            // EditorGUILayout.LabelField(Name + " Settings:", EditorStyles.boldLabel);
            radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 10f);
            intensity = EditorGUILayout.Slider("Intensity", intensity, 0.01f, 1f);
            // TODO PULL OR PUSH - bool or button ...
        }

        public void HandleEvent(Event e, MeshManager mm)
        {
            this.meshManager = mm;

            switch (e.type)
            {
                case EventType.Layout:
                    // UpdateMouseHit(e);
                    UpdateBrush(e);
                    UpdateMesh();
                    // HandleUtility.Repaint(); // TEST CI JE POTREBNE
                    break;

                case EventType.Repaint:
                    UpdateGUI();
                    DrawHandles();
                    // ApplyDisplacement();
                    break;

                case EventType.MouseDown:
                    if (e.button == 0) OnLeftMouseDown(e);
                    // isMouseLeftClick = true;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0) OnLeftMouseDrag(e);
                    // isDragging = true;
                    // HandleUtility.Repaint(); // alebo SceneView.RepaintAll() ak chceš mať istotu
                    break;

                case EventType.MouseUp:
                    if (e.button == 0) OnLeftMouseUp(e);
                    // isDragging = false;
                    // isMouseLeftClick = false;
                    break;
            }
        }

        private void UpdateBrush(Event e)
        {
            hasValidHit = TryGetMouseHit(e, out lastHit);

            if (hasValidHit)
            {
                verticesBuffer = meshManager.GetVerticesBuffer();
                CalculateHitZone(lastHit, verticesBuffer,this.radius);
                // BrushLogic(lastHit, meshManager);
            }
        }

        private bool TryGetMouseHit(Event e, out RaycastHit hit)
        {
            hit = default; // Ensure 'hit' is always assigned

            if (meshManager != null && meshManager.Collider != null)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                return meshManager.Collider.Raycast(ray, out hit, Mathf.Infinity);
            }
            return false;
        }

        // protected virtual void BrushLogic(RaycastHit hit, MeshManager mm)
        // {
        //     // // Výpočet zasiahnutých bodov a displacementov podľa brush logiky
        //     // Vector3 hitPoint = lastHit.Value.point;
        //     // affectedVertices = meshManager.GetVerticesWithinRadius(hitPoint, GetRadius());
        //     // displacementVectors = ComputeDisplacement(affectedVertices, hitPoint);
        // }

        private void CalculateHitZone(RaycastHit hit, List<Vector3> meshVertices, float radius)
        {
            Vector3 localHitPoint = hit.collider.transform.InverseTransformPoint(hit.point);

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
                    float falloff = FalloffLogic(t);
                    weightValues.Add(falloff);
                }
            }
        }

        /// <summary>
        /// Possible to change behaviour of brush falloff
        /// </summary>
        /// <param name="normalizedDistance"> Interaval 0 - 1 ...</param>
        /// <returns>fallov falue for individual vertex</returns>
        protected virtual float FalloffLogic(float normalizedDistance)
        {
            return normalizedDistance;
        }

        public void UpdateGUI()
        { } //DO WE NEED IT?!

        protected virtual void DrawHandles()
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

        // protected virtual Vector3 DisplacementLogic(int index)
        // { 
        //     float magnitude = weightValues[index] * intensity;
        //     Vector3 direction = lastHit.transform.up; //.normalized;
        //     Vector3 displacementVector = direction * magnitude;
        //     return displacementVector;
        // }

        protected virtual Vector3 DisplacementDirection(int index)
        {
            return Vector3.up; //lastHit.transform.up; //.normalized;
        }

        protected virtual float DisplacementMagnitude(int index)
        {
            return weightValues[index] * intensity;
        }

        protected void UpdateMesh()
        {
            if (meshManager == null || !hasValidHit) return;

            if (isMouseLeftDrag || isMouseLeftClick)
            {
                //verticesBuffer = meshManager.GetVerticesBuffer();

                for (int i = 0; i < hitIndices.Count; i++)
                {
                    Vector3 displacement = DisplacementDirection(i) * DisplacementMagnitude(i);
                    verticesBuffer[hitIndices[i]] += displacement;
                }

                meshManager.ApplyVerticesBuffer();
                //  reset InputFlags test - avoid redundanci - or check inside..
                //  or work with wayit - ms... to do new while dragging...
                //  Resetovať iba klik – dragging pokračuje
                // if (isMouseLeftClick && !isMouseLeftDrag)
                isMouseLeftClick = false;
                isMouseLeftDrag = false;
            }   
        }

        protected virtual void OnLeftMouseDown(Event e)
        {
            isMouseLeftClick = true;
            e.Use();
        }

        protected virtual void OnLeftMouseDrag(Event e)
        {
            isMouseLeftDrag = true;
            e.Use();
        }

        protected virtual void OnLeftMouseUp(Event e)
        { 
            isMouseLeftClick = false;
            isMouseLeftDrag = false;
            e.Use(); // označiť event ako spracovaný
        }

        // protected virtual void ApplyDisplacement()
        // {
        //     if (affectedVertices == null || displacementVectors == null) return;
        //     meshManager.ApplyDisplacement(affectedVertices, displacementVectors);
        // }

        // public virtual void DrawGizmos()
        // {
        //     if (lastHit.HasValue)
        //     {
        //         DrawBrushGizmo(lastHit.Value.point);
        //         HighlightAffectedVertices();
        //     }
        // }

        // protected virtual void DrawBrushGizmo(Vector3 position)
        // {
        //     Handles.color = Color.yellow;
        //     Handles.DrawWireDisc(position, Vector3.up, GetRadius());
        // }

        // protected virtual void HighlightAffectedVertices()
        // {
        //     if (affectedVertices == null) return;
        //     Handles.color = Color.green;
        //     foreach (var v in affectedVertices)
        //     {
        //         Handles.DotHandleCap(0, v, Quaternion.identity, 0.02f, EventType.Repaint);
        //     }
        // }

        // protected abstract float GetRadius(); // každá brush môže mať vlastný radius
        // protected abstract Vector3[] ComputeDisplacement(Vector3[] vertices, Vector3 center);

        // private bool TryGetMeshHit(Event e, MeshManager mm, out RaycastHit hit)
        // {
        //     Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        //     return mm.Collider.Raycast(mouseRay, out hit, Mathf.Infinity);
        // }
    }
}
