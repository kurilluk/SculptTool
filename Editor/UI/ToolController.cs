using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SculptTool.Editor.Brushes;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.UI
{
    /// <summary>
    /// Window and UI, GameObject - mesh selection, Brush selection and UI
    /// </summary>
    public class ToolController : EditorWindow
    {
        private static ToolController window;

        private GameObject previousSelection;
        private MeshManager meshManager;

        private List<IBrush> brushes = new();
        private int selectedBrushIndex = 0;
        private IBrush SelectedBrush => brushes[selectedBrushIndex];

        [MenuItem("Tools/Sculpt Mode")]
        public static void ShowWindow()
        {
            window = GetWindow<ToolController>("Sculpt Mode");
            SceneView.duringSceneGui -= window.OnSceneGUI;
            SceneView.duringSceneGui += window.OnSceneGUI;
            window.autoRepaintOnSceneChange = true;
        }

        private void OnEnable()
        {
            brushes = new List<IBrush>()
            {
                new AxialBrush(),
                //new InflateBrush(),
                // new AxisBrush(),
                // new RadialBrush(),
                // new StampBrush(),
                // new TestingBrush(),
                // Add other brushes here
            };
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            MeshManagerFactory.ClearAll();
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            MeshManagerFactory.ClearAll();
        }

        /// <summary>
        /// Draws the user interface for the Sculpt Mode tool.
        /// Includes object selection handling, mesh reset, brush settings, and mesh saving.
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

            DrawMeshButtons();

            DrawBrushSettings();

        }

        /// <summary>
        /// Handles changes in the currently selected GameObject.
        /// Initializes or clears the mesh manager depending on whether
        /// the selected object has a MeshFilter component.
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
        /// Draws mesh manipulation buttons like Reset and Save.
        /// </summary>
        private void DrawMeshButtons()
        {
            if (GUILayout.Button("Reset Mesh to Backup"))
            {
                meshManager.ResetToBackup();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("(payed version only) Save Mesh to Asset"))
            {
                MeshSaver.SaveMeshAsAsset(meshManager.MeshInstance, "Assets/SavedMeshes/ModifiedMesh.asset");
            }
        }

        /// <summary>
        /// Draws the brush settings section of the GUI.
        /// Includes brush selection dropdown and brush-specific GUI rendering.
        /// </summary>
        private void DrawBrushSettings()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Brush Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            string[] names = brushes.Select(b => b.Name).ToArray();
            // string[] names = new string[brushes.Count];
            // for (int i = 0; i < brushes.Count; i++)
            // {
            //     names[i] = brushes[i].Name;
            // }
            selectedBrushIndex = EditorGUILayout.Popup("Select Brush", selectedBrushIndex, names);

            SelectedBrush.GetGUI();
            EditorGUILayout.Space();
        }


        // private SceneContext context;
        private void OnSceneGUI(SceneView sceneView)
        {
            if (meshManager == null || meshManager.Collider == null)
                return;

            Event e = Event.current;

            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            SelectedBrush?.HandleEvent(e, meshManager);
            
            // Debug.Log($"[Frame: {Time.frameCount}] Event Type: {e.type} | Mouse Pos: {e.mousePosition}");

        }
    }
}
