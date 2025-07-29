using UnityEditor;
using UnityEngine;

namespace SculptTool.Editor.Utils
{
    /// <summary>
    /// Utility class for saving Mesh objects as .asset files in the Unity project.
    /// </summary>
    public static class MeshSaver
    {
        /// <summary>
        /// Saves a given Mesh instance as a new asset file in the project.
        /// A file dialog allows the user to choose the save location.
        /// </summary>
        /// <param name="mesh">The mesh to save as an asset.</param>
        /// <param name="assetPath">Not used — will be overwritten by user selection.</param>
        public static void SaveMeshAsAsset(Mesh mesh, string assetPath)
        {
            if (mesh == null) return;

            // Prompt the user for a save location and filename
            string path = EditorUtility.SaveFilePanelInProject(
                "Uložiť Mesh ako Asset",
                mesh.name + "_Edited.asset",
                "asset",
                "Zadaj názov súboru pre nový mesh asset"
            );

            if (string.IsNullOrEmpty(path))
                return;

            // Create and register the mesh asset
            AssetDatabase.CreateAsset(Object.Instantiate(mesh), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Mesh saved to: {path}");
        }
    }
}
