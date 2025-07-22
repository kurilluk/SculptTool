using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace SculptMode
{
    public class SculptToolWindow : EditorWindow
    {
        private static SculptToolWindow window;

        private GameObject previousSelection;
        private MeshManager meshManager;

        private List<IBrush> brushes = new();
        private int selectedBrushIndex = 0;
        private IBrush SelectedBrush => brushes[selectedBrushIndex];

        private Vector3[] previewVertices;  // Just in case of need :)

        [MenuItem("Tools/Sculpt Mode")]
        public static void ShowWindow()
        {
            window = GetWindow<SculptToolWindow>("Sculpt Mode");
            SceneView.duringSceneGui -= window.OnSceneGUI;
            SceneView.duringSceneGui += window.OnSceneGUI;
            window.autoRepaintOnSceneChange = true;
        }

        private void OnEnable()
        {
            brushes = new List<IBrush>()
            {
                new AxisBrush(),
                new RadialBrush(),
                new StampBrush(),
                new TestingBrush(),
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

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sculpt Mode", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GameObject selected = Selection.activeGameObject;

            if (selected != previousSelection)
            {
                previousSelection = selected;

                if (selected != null && selected.TryGetComponent(out MeshFilter newFilter))
                {
                    meshManager = MeshManagerFactory.GetOrCreate(newFilter);
                }
                else
                {
                    meshManager = null;
                }
            }

            if (meshManager == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject with a MeshFilter.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Selected: " + selected.name, MessageType.Info);

            // Testing
            // if (GUILayout.Button("Modify Working Mesh"))
            // {
            //     meshManager.ModifyMesh();
            //     SceneView.RepaintAll();
            // }

            if (GUILayout.Button("Reset Mesh to Backup"))
            {
                meshManager.ResetToBackup();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Brush Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            string[] names = brushes.Select(b => b.Name).ToArray();
            selectedBrushIndex = EditorGUILayout.Popup("Select Brush", selectedBrushIndex, names);

            SelectedBrush.GetGUI();
            EditorGUILayout.Space();

            if (GUILayout.Button("(payed version only) Save Mesh to Asset"))
            {
                MeshSaver.SaveMeshAsAsset(meshManager.MeshInstance, "Assets/SavedMeshes/ModifiedMesh.asset");
            }
        }


        private void OnSceneGUI(SceneView sceneView)
        {
            if (meshManager == null || meshManager.Collider == null)
                return;

            Event e = Event.current;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (meshManager.Collider.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity))
            {
                // zobrazíme zelený bod pod kurzorom
                float scale = HandleUtility.GetHandleSize(hit.point) * 0.05f;
                Handles.color = Color.green;
                Handles.CubeHandleCap(0, hit.point, Quaternion.LookRotation(Vector3.up), scale, EventType.Repaint);

                // Preview brush deformácie
                previewVertices = SelectedBrush.Preview(meshManager, hit);

                // if (previewVertices != null)
                // {
                //     Mesh mesh = meshManager.MeshInstance;
                //     mesh.vertices = previewVertices;
                //     mesh.RecalculateNormals();
                //     mesh.RecalculateBounds();
                // }

                if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && !e.alt)
                {
                    SelectedBrush.ApplyBrush(meshManager);
                    e.Use();
                }
            }
        }
    }
}
