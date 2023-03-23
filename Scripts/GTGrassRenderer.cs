using System;
using System.Collections.Generic;
using UnityEditor;
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

    void OnEnable()
    {
        this.hideFlags = HideFlags.HideInInspector;

        Debug.Log(Chunks.Count);
        foreach (var chunk in Chunks)
        {
            InitializeBuffers(chunk);
        }
    }

    public void InitializeBuffers(Chunk chunk)
    {
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)chunk.Mesh.GetIndexCount(0);
        args[1] = (uint)chunk.MeshData.Count;
        args[2] = (uint)chunk.Mesh.GetIndexStart(0);
        args[3] = (uint)chunk.Mesh.GetBaseVertex(0);

        chunk.ArgsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        chunk.MeshDataBuffer = new ComputeBuffer(chunk.MaxChunkPopulation, MeshData.Size());

        chunk.MeshDataBuffer.SetData(chunk.MeshData);
        chunk.ArgsBuffer.SetData(args);

        chunk.Material.SetBuffer("_Properties", chunk.MeshDataBuffer);
    }

    void OnDisable()
    {
        ReleaseChunkBuffers();
    }

    private void OnDestroy()
    {
        ReleaseChunk();
    }

    private void LateUpdate()
    {
        RenderChunks();
    }

    private void RenderChunks()
    {
        Camera cam = null;

        if(!Application.isPlaying)
            cam = SceneView.lastActiveSceneView.camera;
        else if(Application.isPlaying)
            cam = Camera.main;

        if (cam == null)
            return;

        for (int i = 0; i < Chunks.Count; i++)
        {
            // Debug.Log(Chunks[i].Mesh);
            // Debug.Log(Chunks[i].Material);
            // Debug.Log(Chunks[i].Bounds);
            // Debug.Log(Chunks[i].ArgsBuffer);

            float dist = Vector3.Distance(cam.transform.position, Chunks[i].Bounds.center);

            if(dist < _renderDistance)
                Graphics.DrawMeshInstancedIndirect(Chunks[i].Mesh, 0, Chunks[i].Material, Chunks[i].Bounds, Chunks[i].ArgsBuffer); // , 0, null, _castShadows, _receiveShadows
        }
    }

    private void ReleaseChunkBuffers()
    {
        for (int i = 0; i < Chunks.Count; ++i)
        {
            ReleaseBuffers(Chunks[i]);
        }
    }

    void ReleaseBuffers(Chunk chunk)
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

    void ReleaseChunk()
    {
        foreach (var chunk in Chunks)
        {
            ReleaseChunkData(chunk);
        }

        // Clear Chunks
        _ChunkCache.Clear();
        Chunks = null;
    }

    void ReleaseChunkData(Chunk chunk)
    {
        if (chunk.MeshData != null)
        {
            chunk.MeshData.Clear();
            chunk.MeshData = null;
        }

        if (chunk.Material != null)
        {
            DestroyImmediate(chunk.Material);
            chunk.Material = null;
        }

        if (chunk.Mesh != null)
        {
            DestroyImmediate(chunk.Mesh);
            chunk.Mesh = null;
        }
    }

    public void UpdateChunkDataIndirect(Vector3 position, Quaternion rotation, Vector3 scale, Vector4 color)
    {
        Grid grid = GetComponent<Grid>();

        Vector3Int cellPosition = grid.WorldToCell(position);
        Vector3 center = grid.GetCellCenterWorld(cellPosition);
        Chunk chunk = GetChunk(cellPosition, center);

        // Get offset
        Vector3 worldPosition = position - center;

        // MeshData
        MeshData mesh = new MeshData();
        mesh.Matrix = Matrix4x4.TRS(worldPosition, rotation, scale);
        mesh.Color = color;
        chunk.MeshData.Add(mesh);

        chunk.UpdateBuffers();
        // RenderChunks();
    }

    private Chunk GetChunk(Vector3Int cellPosition, Vector3 center)
    {
        string id = GetCellID(cellPosition);

        Chunk chunk = GetChunkDictionary(id, center);
        return chunk;
    }

    public string GetCellID(Vector3Int position)
    {
        int x = position.x;
        int z = position.z;

        return $"Cell: {x}, {z}";
    }

    private void OnDrawGizmos()
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
    }
}
