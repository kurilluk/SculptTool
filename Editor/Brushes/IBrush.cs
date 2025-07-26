using UnityEngine;
using SculptTool.Editor.Utils;

namespace SculptTool.Editor.Brushes
{
    /// <summary>
    /// Interface for sculpting brushes used in the sculpting tool.
    /// Each brush defines its name, custom GUI, and event handling logic in the scene.
    /// </summary>
    public interface IBrush
    {
        /// <summary>
        /// Gets the display name of the brush.
        /// This is used for GUI dropdowns and brush identification.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Draws custom GUI controls for the brush inside the editor window.
        /// Called by the ToolController during OnGUI.
        /// </summary>
        void GetGUI();

        /// <summary>
        /// Handles scene events such as mouse input for brush interaction.
        /// Called by the ToolController during OnSceneGUI.
        /// </summary>
        /// <param name="e">The current event from the scene view.</param>
        /// <param name="mm">Reference to the active MeshManager handling the sculpt mesh.</param>
        void HandleEvent(Event e, MeshManager mm);
    }
}

