using UnityEngine;
using UnityEditor;

namespace SculptMode
{
    public class SculptToolWindow : EditorWindow
    {
        private static SculptToolWindow window;

        private MeshManager meshManager;

        [MenuItem("Tools/Sculpt Mode")]
        public static void ShowWindow()
        {
            window = GetWindow<SculptToolWindow>("Sculpt Mode");
            SceneView.duringSceneGui -= window.OnSceneGUI;
            SceneView.duringSceneGui += window.OnSceneGUI;
            window.autoRepaintOnSceneChange = true;
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
            if (selected != null && selected.TryGetComponent(out MeshFilter mf))
            {
                meshManager = MeshManagerFactory.GetOrCreate(mf);
                meshManager.BackupAndClone();

                EditorGUILayout.HelpBox("Selected: " + selected.name, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Select a GameObject with a MeshFilter.", MessageType.Warning);
                return;
            }

            // if (GUILayout.Button("Backup and Clone SharedMesh"))
            // {
            //     meshManager.BackupAndClone();
            // }

            if (GUILayout.Button("Modify Working Mesh"))
            {
                meshManager.ModifyMesh();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Reset Mesh to Original"))
            {
                meshManager.ResetToBackup();
                SceneView.RepaintAll();
            }
        }

       private void OnSceneGUI(SceneView sceneView)
        {
            if (meshManager == null || meshManager.Collider == null)
                return;

            Event e = Event.current;

            // Prevzatie vstupu
            if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Ray mouseWorldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (meshManager.Collider.Raycast(mouseWorldRay, out RaycastHit hit, Mathf.Infinity))
            {
                float scale = HandleUtility.GetHandleSize(hit.point) * 0.05f;
                Handles.color = Color.green;
                Handles.CubeHandleCap(0, hit.point, Quaternion.LookRotation(Vector3.up), scale, EventType.Repaint);
            }
        }

    }
}
