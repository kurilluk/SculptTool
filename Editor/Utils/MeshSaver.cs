using UnityEditor;
using UnityEngine;

namespace SculptTool.Editor.Utils
{
    public static class MeshSaver // TODO: keep it or include to MeshManager
    {
        public static void SaveMeshAsAsset(Mesh mesh, string assetPath)
        {
            if (mesh == null) return;

        string path = EditorUtility.SaveFilePanelInProject(
            "Uložiť Mesh ako Asset",
            mesh.name + "_Edited.asset",
            "asset",
            "Zadaj názov súboru pre nový mesh asset"
        );

        if (string.IsNullOrEmpty(path))
            return;

            AssetDatabase.CreateAsset(Object.Instantiate(mesh), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Mesh saved to: {assetPath}");
        }
    }
}
