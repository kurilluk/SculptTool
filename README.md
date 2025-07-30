# SculptTool

# Mesh Manager in Unity

At the beginning of the development process, it was necessary to create a `MeshManager` to avoid modifying the original mesh asset directly. This manager handles a duplicate of the `sharedMesh` and applies all edits to that duplicate.

For the purposes of the Brush Tool—which frequently modifies mesh vertex data—a `List<Vector3>` was chosen over traditional arrays. This list is allocated once and reused, which helps prevent memory leaks that could otherwise occur with dynamic array reallocation. The system architecture was built around this decision: all operations work directly on a reference to the vertex list. In the frequently called `OnSceneGUI()` function, object instantiation is minimized—even at the expense of code readability—to reduce memory pressure.

Following best practices, scene events are split by type:  
- **Layout** for calculations  
- **Repaint** for rendering  
- **Input** for capturing user actions and setting trigger flags for Layout processing.

A potential improvement in the future would be the introduction of a `BrushContext` class, which could improve readability and testability of the tool’s architecture. To maximize efficiency, this context should avoid being re-instantiated on every Scene Event.

---

# Tool GUI

Due to limited time for GUI design, a pragmatic approach was taken. A custom **Menu Item** was created that opens a standalone window using standard `EditorGUILayout` elements. The tool can be launched via the top menu:  
**Tools > Sculpt Mode**

To enhance the user experience, several keyboard shortcuts were added:
- **Hold Ctrl** to toggle between Push/Pull mode  
- **Ctrl + Scroll Wheel** to adjust the brush radius

The user receives visual feedback about the tool’s impact through a set of `Handles` drawn during the Repaint event. Spheres are rendered at affected mesh vertices, with their size and color indicating the strength and direction of the vertex displacement. Additionally, a disc visualizes the brush radius—similar to conventions in other sculpting tools.

In the future, the GUI could be improved with a more modern layout and extended brush settings. Currently, brushes operate in the **local coordinate space** of the mesh object. A future enhancement could allow switching to **world coordinates**.

At the moment, displacement direction indicators are not rendered. This is because most sculpting occurs along the same axis. However, such visualization could be useful—especially if an **Inflate Brush** is developed that moves vertices along their normals.

---

# Brushes and Modularity

This directory contains a set of modular sculpting brushes for Unity's Editor, built on top of the abstract `BrushBase` class. Each brush manipulates vertex positions based on its custom logic, supporting flexible sculpting workflows for terrain, meshes, and procedural editing.

## Architecture

All brushes inherit from:

```csharp
abstract class BrushBase : IBrush
````

Each brush implements:

* `Name`: A display name.
* `GetGUI()`: A Unity Editor UI for brush-specific settings.
* `FalloffLogic(float normalizedDistance)`: Controls influence per vertex.
* `CalculateMagnitude(int hitZoneIndex)`: Defines per-vertex sculpting strength.
* Optionally: `GetDisplacementDirection(int index)` or `OnLayoutUpdate(Event e)`.

Brushes access and modify:

* `verticesBuffer`: The complete vertex array.
* `hitIndices`: Indices of affected vertices.
* `falloffValues`: Precomputed falloff multipliers.
* (Optionally) other cached data like `deltaValues`.

---

## Available Brushes

### 1. **Axial Brush**

Displaces vertices along a selected world axis (X, Y, Z), with customizable falloff.

```csharp
public class AxialBrush : BrushBase
```

**Parameters:**

* `Direction Axis`: Selects X / Y / Z axis.
* `Falloff`: Attenuation curve from center outward.

**Use Case:** Pull/push vertices in one direction, e.g. extrude upward or sideways.

---

### 2. **Flatten Brush**

Flattens terrain or geometry by pulling vertices toward the average height of the sculpt area.

```csharp
public class FlattenBrush : BrushBase
```

**Parameters:**

* `Falloff`: Controls deformation spread.
* `Precision Threshold`: Minimum height delta to act on.

**Core Logic:**

* Computes the average Y height of affected vertices.
* Each vertex is displaced proportionally toward that average.
* Delta is normalized and modulated by falloff.

**Use Case:** Creating plateaus, terraces, or leveling uneven surfaces.

---

### 3. **Stamp Brush (Perlin Noise)**

Applies Perlin noise-based height offsets to vertices, creating randomized natural deformation.

```csharp
public class StampBrush : BrushBase
```

**Parameters:**

* `Noise Scale`: Controls frequency of Perlin noise.
* `Falloff Curve`: Limits intensity from center outward.

**Core Logic:**

* Uses `PerlinNoise(x, z)` sampled from world position.
* Displacement strength is controlled by falloff.

**Use Case:** Adding terrain roughness, noise-based details, or stylized distortion.

---

## Integration Notes

* All brushes are designed for editor-time sculpting.
* They require a system that supplies `verticesBuffer` and `hitIndices` each frame.
* Use `OnLayoutUpdate()` to precalculate cached values like average height or deltas.

---

<!-- ## Directory Structure

```
Brushes/
├── AxialBrush.cs
├── FlattenBrush.cs
├── StampBrush.cs
└── BrushBase.cs
``` -->

---

## 🔧 Dependencies

* UnityEditor
* UnityEngine
* `SculptTool.Editor.Utils` – internal utility scripts (e.g. vertex filtering, common buffers)

---

## Authoring New Brushes

Create a new class inheriting `BrushBase` and implement:

* A unique `Name`
* `FalloffLogic()` for distance weighting
* `CalculateMagnitude()` to define deformation strength
* (Optional) `GetDisplacementDirection()` if direction varies

Example:

```csharp
public class MyCustomBrush : BrushBase
{
    public override string Name => "My Brush";

    protected override float CalculateMagnitude(int index)
    {
        return 1f; // Custom logic here
    }
}
```

---

## 📝 License

MIT / Custom — see root `LICENSE` file.

```

---
