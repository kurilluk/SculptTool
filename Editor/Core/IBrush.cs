using UnityEngine;

namespace SculptMode
{
    public interface IBrush
    {
        string Name { get; }
        void GetGUI();
        Vector3[] Preview(MeshManager mm, RaycastHit hit);  
        void ApplyBrush(MeshManager mm);                    
    }
}
