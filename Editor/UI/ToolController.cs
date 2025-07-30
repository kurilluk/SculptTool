using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Brushes;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.UI
{
    /// <summary>
    /// Main editor window for Sculpt Mode tool.
    /// Handles UI, brush selection, GameObject targeting, mesh management and scene interaction.
    /// </summary>
    public class ToolController : EditorWindow
    {
        private static ToolController window;

        private GameObject previousSelection;
        private MeshManager meshManager;

        private bool sculptModeActive = false;

        private List<IBrush> brushes = new();
        private int selectedBrushIndex = 0;

        /// <summary>
        /// Currently selected brush based on the dropdown index.
        /// </summary>
        private IBrush SelectedBrush => brushes[selectedBrushIndex];

        /// <summary>
        /// Opens the Sculpt Mode editor window from the Unity menu.
        /// </summary>
        [MenuItem("Tools/SculptTool")]
        public static void ShowWindow()
        {
            window = GetWindow<ToolController>("SculptTool");
            SceneView.duringSceneGui -= window.OnSceneGUI;
            SceneView.duringSceneGui += window.OnSceneGUI;
            window.autoRepaintOnSceneChange = true;
        }

        /// <summary>
        /// Called when the window is enabled.
        /// Initializes the list of available brushes.
        /// </summary>
        private void OnEnable()
        {
            brushes = new List<IBrush>()
            {
                new AxialBrush(),
                new FlattenBrush(),
                new StampBrush(),
                // Add other brushes here
            };
        }

        /// <summary>
        /// Cleans up resources when the window is disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            MeshManagerFactory.ClearAll();
            Tools.hidden = false;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            MeshManagerFactory.ClearAll();
        }

        /// <summary>
        /// Main GUI rendering method.
        /// Displays the sculpt controls and brush-specific settings.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sculpt Mode", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            HandleSelectionChange();

            if (meshManager == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject with a MeshFilter.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Selected: " + Selection.activeGameObject.name, MessageType.Info);

            ToolPoweButton();

            if (!sculptModeActive)
                return;

            DrawMeshButtons();
            DrawUndoRedoButtons();
            DrawBrushSettings();
        }

        /// <summary>
        /// Renders the activate/deactivate sculpt mode toggle button.
        /// </summary>
        private void ToolPoweButton()
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = sculptModeActive ? new Color(0.7f, 0.2f, 0.2f) : new Color(0.2f, 0.7f, 0.2f);

            if (GUILayout.Button(sculptModeActive ? "Deactivate Sculpt Mode" : "Activate Sculpt Mode"))
            {
                sculptModeActive = !sculptModeActive;

                Tools.hidden = sculptModeActive;
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = originalColor;
        }

        /// <summary>
        /// Monitors selection changes in the Unity Editor.
        /// Updates the mesh manager when a new mesh object is selected.
        /// </summary>
        private void HandleSelectionChange()
        {
            GameObject selected = Selection.activeGameObject;

            if (selected != previousSelection)
            {
                previousSelection = selected;

                if (selected != null && selected.TryGetComponent(out MeshFilter meshFilter))
                {
                    meshManager = MeshManagerFactory.GetOrCreate(meshFilter);
                }
                else
                {
                    meshManager = null;
                }
            }
        }

        /// <summary>
        /// Renders mesh control buttons for resetting or saving the current mesh.
        /// </summary>
        private void DrawMeshButtons()
        {
            if (GUILayout.Button("Reset Mesh to Backup"))
            {
                meshManager.ResetToBackup();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Save Mesh to Asset (Premium only)"))
            {
                EditorUtility.DisplayDialog("Premium Feature",
                    "Saving meshes is only available in the premium version.\n\nClick OK to HIRE ME and unlock the feature!\n\nThank you for your consideration. :)",
                    "OK");

                MeshSaver.SaveMeshAsAsset(meshManager.MeshInstance, "Assets/SavedMeshes/ModifiedMesh.asset");
            }
        }

        /// <summary>
        /// Renders Undo and Redo buttons to allow sculpting history navigation.
        /// </summary>
        private void DrawUndoRedoButtons()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUI.enabled = meshManager != null;

            if (GUILayout.Button("Undo"))
            {
                meshManager.Undo();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Redo"))
            {
                meshManager.Redo();
                SceneView.RepaintAll();
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders brush selection dropdown and delegates brush-specific GUI to the active brush.
        /// </summary>
        private void DrawBrushSettings()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Brush Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            string[] names = brushes.Select(b => b.Name).ToArray();
            selectedBrushIndex = EditorGUILayout.Popup("Select Brush", selectedBrushIndex, names);

            SelectedBrush.GetGUI();

            EditorGUILayout.Space();
        }

        /// <summary>
        /// Handles events in the Scene view, passing them to the currently active brush.
        /// Also ensures brush previews or interactions are drawn in-scene.
        /// </summary>
        private void OnSceneGUI(SceneView sceneView)
        {
            if (meshManager == null || meshManager.Collider == null)
                return;

            if (!sculptModeActive)
                return;

            Event e = Event.current;

            // Capture scene input
            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            SelectedBrush?.HandleEvent(e, meshManager);

            // Debug logging
            // Debug.Log($"[Frame: {Time.frameCount}] Event Type: {e.type} | Mouse Pos: {e.mousePosition}");
        }
    }
}
