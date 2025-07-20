using UnityEngine;
using UnityEditor;
// using System.Collections.Generic;

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
            // meshManager?.Cleanup();
            MeshManagerFactory.ClearAll();
        }

        private void OnDestroy()
        {
            // meshManager?.Cleanup();
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
                // if (meshManager == null || mf != meshManager.TargetFilter)
                //     meshManager = new MeshManager(mf);

                EditorGUILayout.HelpBox("Selected: " + selected.name, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Select a GameObject with a MeshFilter.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Backup and Clone SharedMesh"))
            {
                meshManager.BackupAndClone();
            }

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

        private void OnSceneGUI(SceneView sceneView) { }
    }
}
