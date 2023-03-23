using UnityEngine;

public class Greenthumb : MonoBehaviour
{
    // private GTPrefabPainter _prefabPainter;
    private GTGrassRenderer _grassRenderer;
    public GTGrassRenderer GrassRenderer { get{ return _grassRenderer; } }

    
    void OnEnable()
    {
        if(_grassRenderer == null)
            _grassRenderer = gameObject.AddComponent<GTGrassRenderer>();

        // _prefabPainter = gameObject.AddComponent<GTPrefabPainter>();
    }

    // Grass Renderer
    public void UpdateChunkData(Vector3 position, Quaternion rotation, Vector3 scale, Vector4 color) 
    {
        _grassRenderer.UpdateChunkDataIndirect(position, rotation, scale, color);
    }
    public void SetGrassMesh(Mesh mesh) => _grassRenderer._mesh = mesh;
    public void SetGrassMaterial(Material material) => _grassRenderer._material = material;

    // Prefab Painter

}
