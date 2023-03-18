using UnityEngine;
using UnityEditor;

public partial class GreenthumbEditor
{
    private Mesh _mesh;
    private Material _material;

    // GUI
    private bool _grassFoldout = false;

    [Header("Chunk Parameters")]
    // private int _chunksXZ = 5; // Automatically calculate based on the bounds of the paint

    private int _chunkSize = 25;
    // private int _population = 0;
    // private int _maxChunkPopulation = 1000;

    // private float _renderDistance { get => _renderDistanceInChunks * _chunkSize.x; }
    // private int _chunkCount { get => _chunks.Length; }
    // private int _chunkPopulation { get => _population / _chunkCount; }
    // private Chunk[] _chunks;

    // private uint[] _args;

    // private Dictionary<int, Chunk> _ChunkCache = new Dictionary<int, Chunk>();
    // private Chunk GetChunkDictionary(Vector3 center, int xIndex, int zIndex)
    // {
    //     int i = xIndex + zIndex;

    //     if (!_ChunkCache.ContainsKey(i))
    //     {
    //         _ChunkCache[i] = InitChunk(center, i);
    //     }
    //     return _ChunkCache[i];
    // }

    // private void LoadChunksFromRenderer(Chunk[] arr)
    // {
    //     if(_obj == null) return;
    //     _ChunkCache.Clear();

    //     foreach (var item in _obj.GetComponent<GTGrassRenderer>().Chunks)
    //     {
    //         _ChunkCache[item.id] = item;
    //     }
    // }

    // private void ClearChunkCache()
    // {
    //     foreach (KeyValuePair<int, Chunk> kvp in _ChunkCache)
    //     {
    //         // Im 90% sure this is overkill but that last 10% keeps me up at night
    //         // if(kvp.Value.meshDataBuffer != null)
    //         // {
    //         //     kvp.Value.meshDataBuffer.Release();
    //         // }

    //         // if(kvp.Value.argsBuffer != null)
    //         // {
    //         //     kvp.Value.argsBuffer.Release();
    //         // }

    //         // if(kvp.Value.meshData != null)
    //         // {
    //         //     kvp.Value.meshData.Clear();
    //         // }

    //         // Release and set buffers to null
    //         Chunk chunk = kvp.Value;

    //         if (chunk.meshDataBuffer != null)
    //         {
    //             chunk.meshDataBuffer.Release();
    //             chunk.meshDataBuffer = null;
    //         }

    //         if (chunk.argsBuffer != null)
    //         {
    //             chunk.argsBuffer.Release();
    //             chunk.argsBuffer = null;
    //         }

    //         if(chunk.meshData != null)
    //         {
    //             chunk.meshData.Clear();
    //             chunk.meshData = null;
    //         }

    //         _ChunkCache[kvp.Key] = chunk;
    //     }
    //     _ChunkCache.Clear();
    // }

    private float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / _chunkSize * scale + offset, (position.y + 0.1f) / _chunkSize * scale + offset);
    }

    private void PlaceGrass(RaycastHit hit)
    {
        // Pos
        Vector3 position = hit.point;
        // Vector3 position = Vector3.zero;

        // Rot
        // Quaternion rot = Quaternion.identity.WeightedNormal(hit.normal, _brushSettings.BrushNormalWeight).RandomizeAxisRotation(new Vector3(0, 180, 0));

        // // Recalculates normal with the weight set to 1 so the normalLimit threshold can be calculated correctly
        // float dot = Quaternion.Dot(Quaternion.identity.WeightedNormal(hit.normal), Quaternion.identity);
        // if(dot <= _brushSettings.BrushNormalLimit) return;

        Quaternion rot = Quaternion.identity;

        // Scale
        // float n = Get2DPerlin(position, 0, 1);
        // Vector3 scale = new Vector3(0, n, 0); // Set Scale from noise
        Vector3 scale = Vector3.one;

        MeshData mesh = new MeshData();
        mesh.mat = Matrix4x4.TRS(position, rot, scale);
        mesh.color = Color.green;

        _obj.GetComponent<GTGrassRenderer>().UpdateChunkData(mesh);
    }

    private void RemoveGrass(RaycastHit hit)
    {
        // Debug.Log("Remove Grass");
        // Check chunk matrix and find positions in range
    }

    public void SelectedGrassGUI()
    {
        EditorGUILayout.BeginVertical("HelpBox");
        if(_grassFoldout = EditorGUILayout.Foldout(_grassFoldout, "Grass Settings"))
        {
            Mesh newMesh = EditorGUILayout.ObjectField(_mesh, typeof(Mesh), false) as Mesh;
            if (newMesh != _mesh)
            {
                _mesh = newMesh;
                _obj.GetComponent<GTGrassRenderer>()._mesh = _mesh;
            }

            Material newMaterial = EditorGUILayout.ObjectField(_material, typeof(Material), false) as Material;
            if (newMaterial != _material)
            {
                _material = newMaterial;
                _obj.GetComponent<GTGrassRenderer>()._material = _material;
            }
        }
        EditorGUILayout.EndVertical();
    }
}
