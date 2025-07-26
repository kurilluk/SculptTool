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

        // Výsledky výpočtov - napriklad vzdialenosti k vrcholom
        // protected Vector3[] affectedVertices;
        // protected Vector3[] displacementVectors;  // NIE JE LEPSIE AKO LIST a ADD ak je potrebne?

        protected readonly List<int> hitIndices = new();
        protected readonly List<float> weightValues = new();

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        /// <summary>
        /// Draws custom GUI controls for the brush inside the editor window.
        /// Can be overridden by derived classes.
        /// Default implementation does nothing.
        /// </summary>
        public virtual void GetGUI()
        {
            // Optional to override
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
                BrushLogic(lastHit, meshManager);
            }
        }

        // protected bool TryGetMouseHit(Event e, out RaycastHit hit)
        // {
        //     Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        //     return meshManager?.Collider.Raycast(ray, out hit, Mathf.Infinity) ?? false;
        // }

        protected bool TryGetMouseHit(Event e, out RaycastHit hit)
        {
            hit = default; // Ensure 'hit' is always assigned

            if (meshManager != null && meshManager.Collider != null)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                return meshManager.Collider.Raycast(ray, out hit, Mathf.Infinity);
            }

            return false;
        }


        protected virtual void BrushLogic(RaycastHit hit, MeshManager mm)
        {
            // // Výpočet zasiahnutých bodov a displacementov podľa brush logiky
            // Vector3 hitPoint = lastHit.Value.point;
            // affectedVertices = meshManager.GetVerticesWithinRadius(hitPoint, GetRadius());
            // displacementVectors = ComputeDisplacement(affectedVertices, hitPoint);
        }

        public void UpdateGUI()
        { }
        protected virtual void DrawHandles()
        { }

        protected virtual void UpdateMesh()
        {
            // if (meshManager == null) return;
            // if (isDraging || isMouseLeftClick)
            // {
            //     // do update of the mesh
            //     meshManager.ApplyVertexBuffer();

            //     // reset InputFlags test - avoid redundanci - or check inside..
            //     // Resetovať iba klik – dragging pokračuje
            //     if (isMouseLeftClick && !isDragging)
            //         isMouseLeftClick = false;
            // }
        }

        // private void UpdateMouseHit(Event e)
        // {
        //     Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        //     if (meshManager.Collider.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity))
        //     {
        //         lastHit = hit;
        //     }
        //     else
        //     {
        //         lastHit = null;
        //     }
        // }

        // private void UpdateBrushLogic()
        // {
        //     if (!lastHit.HasValue) return;

        //     BrushLogic(lastHit);
        // }



        protected virtual void OnLeftMouseDown(Event e)
        {
            // ApplyDisplacement();
            isMouseLeftClick = true;
            e.Use(); // označiť event ako spracovaný
        }

        protected virtual void OnLeftMouseDrag(Event e)
        {
            // ApplyDisplacement();
            // e.Use();
        }

        protected virtual void OnLeftMouseUp(Event e)
        { 
            isMouseLeftClick = false;
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
