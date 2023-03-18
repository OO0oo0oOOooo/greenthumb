using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class GTGrassRenderer : MonoBehaviour
{
    [Header("Parameters")]
    public Mesh _mesh;
    public Material _material;

    [Header("Chunk Parameters")]
    public List<Chunk> Chunks = new List<Chunk>();

    [SerializeField] private int _chunkSize = 25; // this is also inside the EditorWindow Script and that is dumb and bad
    [SerializeField] private float _renderDistanceInChunks = 4;
    private float _renderDistance  => _renderDistanceInChunks * _chunkSize;
    [SerializeField] private bool _debugChunks = false;

    [Header("Shadows")]
    private ShadowCastingMode _castShadows = ShadowCastingMode.Off;
    private bool _receiveShadows = true;

    private void OnDestroy()
    {
        ReleaseChunks();
    }

    private void Update()
    {
        // Debug.Log("Update");
        RenderChunks();
    }

    private void RenderChunks()
    {
        if(Camera.main == null)
            return;

        for (int i = 0; i < Chunks.Count; i++)
        {
            float dist = Vector3.Distance(Camera.main.transform.position, Chunks[i].Bounds.center);

            if(dist < _renderDistance)
                Graphics.DrawMeshInstancedIndirect(Chunks[i].Mesh, 0, Chunks[i].Material, Chunks[i].Bounds, Chunks[i].ArgsBuffer, 0, null, _castShadows, _receiveShadows);
        }
    }

    // private void RenderChunks()
    // {
    //     if(Camera.main == null)
    //         return;

    //     for (int i = 0; i < Chunks.Count; i++)
    //     {
    //         float dist = Vector3.Distance(Camera.main.transform.position, Chunks[i].Bounds.center);

    //         if(dist < _renderDistance)
    //         {
    //             if (Application.isPlaying)
    //                 Graphics.DrawMeshInstancedIndirect(Chunks[i].Mesh, 0, Chunks[i].Material, Chunks[i].Bounds, Chunks[i].ArgsBuffer, 0, null, _castShadows, _receiveShadows);
    //             else
    //             {
    //                 // Use Graphics.DrawMeshInstanced() in edit mode
    //                 // Note: You will need to provide additional data such as an array of matrices and an array of materials to use this method
    //                 // See the documentation for more details: https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstanced.html
    //                 Matrix4x4[] matrices = ...;
    //                 MaterialPropertyBlock[] properties = ...;
    //                 Graphics.DrawMeshInstanced(Chunks[i].Mesh, 0, Chunks[i].Material, Chunks[i].MeshData, matrices.Length, properties);
    //             }
    //         }
    //     }
    // }

    void ReleaseChunks()
    {
        // OnDisable
        for (int i = 0; i < Chunks.Count; ++i) 
        {
            ReleaseChunk(Chunks[i]);
        }

        Chunks = null;
        _ChunkCache.Clear();
    }

    void ReleaseChunk(Chunk chunk)
    {
        Debug.Log("Chunk " + chunk.ID + " releasing buffers");
        if (chunk.MeshDataBuffer != null)
        {
            chunk.MeshDataBuffer.Release();
            chunk.MeshDataBuffer = null;
        }

        if (chunk.ArgsBuffer != null)
        {
            chunk.ArgsBuffer.Release();
            chunk.ArgsBuffer = null;
        }

    }

    public void UpdateChunkData(MeshData mesh)
    {
        Chunk chunk = GetChunk(mesh.mat.GetPosition());
        chunk.MeshData.Add(mesh);
        chunk.UpdateBuffers();
        // UpdateBuffers(chunk);
    }

    // Chunk InitChunk(Vector3 center, int id)
    // {
    //     // if(_args == null) InitArgs();

    //     Chunk chunk = new Chunk();

    //     chunk.id = id;
    //     chunk.mesh = _mesh;
    //     chunk.population = 0;
    //     chunk.meshData = new List<MeshData>();
    //     chunk.bounds = new Bounds(center, new Vector3(_chunkSize, _chunkSize, _chunkSize));

    //     // Create Buffers
    //     chunk.argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    //     chunk.meshDataBuffer = new ComputeBuffer(_maxChunkPopulation, MeshData.Size(), ComputeBufferType.Append);
    //     chunk.meshDataBuffer.SetCounterValue(0);

    //     // Set Buffer Data
    //     chunk.argsBuffer.SetData(_args);

    //     // Create Material
    //     chunk.material = new Material(_material);

    //     return chunk;
    // }

    // private void UpdateBuffers(Chunk chunk)
    // {
    //     chunk.Population++;
    //     // if(chunk.population > _maxChunkPopulation)
    //     // {
    //     //     chunk.population--;
    //     //     return;
    //     // }

    //     // if(chunk.population < 0) return;
    //     // // Compare Y of new item and bounds and reset bounds.
    //     // if(chunk.meshData[(int)chunk.population-1].mat.GetPosition().y > chunk.bounds.size.y)
    //     // {
    //     //     chunk.bounds.size = new Vector3(chunk.bounds.size.x, chunk.meshData[(int)chunk.population].mat.GetPosition().y, chunk.bounds.size.z);
    //     // }
    //     // else if(chunk.meshData[(int)chunk.population].mat.GetPosition().y < chunk.bounds.center.y)
    //     // {
    //     //     chunk.bounds.size = new Vector3(chunk.bounds.center.x, chunk.meshData[(int)chunk.population].mat.GetPosition().y, chunk.bounds.center.z);
    //     // }

    //     // I think this would add all the items in chunk.meshdata not just the new entrys.
    //     // chunk.meshDataBuffer.SetCounterValue(chunk.population);
    //     // chunk.meshDataBuffer.SetData(chunk.meshData);

    //     // TODO: Add the new items to the buffer only. chunk.meshDataBuffer.SetData(chunk.meshData, chunk.population, chunk.population, chunk.meshData.Length);
    //     chunk.MeshDataBuffer.SetData(chunk.MeshData);

    //     chunk.material.SetBuffer("_Properties", chunk.MeshDataBuffer);

    //     // Chunk[] chunkArray = _ChunkCache.Values.ToArray();
    // }

    private Dictionary<string, Chunk> _ChunkCache = new Dictionary<string, Chunk>();
    private Chunk GetChunkDictionary(string id, Vector3 center)
    {
        if (!_ChunkCache.ContainsKey(id))
        {
            Bounds bounds = new Bounds(center, new Vector3(_chunkSize, _chunkSize, _chunkSize));
            Chunk chunk = new Chunk(id, _mesh, bounds, _material);

            _ChunkCache[id] = chunk;
            Chunks.Add(chunk);
        }

        return _ChunkCache[id];
    }

    private Chunk GetChunk(Vector3 point)
    {
        Grid grid = GetComponent<Grid>();
        grid.cellSize = new Vector3(25, 25, 25);

        Vector3Int cellPosition = grid.WorldToCell(point);
        Vector3 center = grid.GetCellCenterWorld(cellPosition);
        string id = GetCellID(cellPosition);

        Debug.Log("ID: " + id);

        Chunk chunk = GetChunkDictionary(id, center);

        return chunk;
    }

    public string GetCellID(Vector3Int position)
    {
        int x = position.x;
        int z = position.z;

        return $"Cell: {x}, {z}";
    }

    void OnDrawGizmos()
    {
        if(!_debugChunks)
            return;

        Gizmos.color = Color.green;
        if (Chunks != null)
        {
            for (int i = 0; i < Chunks.Count; ++i)
            {
                Gizmos.DrawWireCube(Chunks[i].Bounds.center, Chunks[i].Bounds.size);
            }
        }

        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
