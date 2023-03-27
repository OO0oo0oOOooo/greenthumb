using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class Greenthumb : MonoBehaviour
{
    // This has to be here so i can use it as a property. When i was creating a SO of the editor i wasnt
    // allowed to modify the propertys for some reason.
    public PaletteData _selectedItem;

    // Grass Rendering
    [Header("Parameters")]
    public Mesh _mesh;
    public Material _material;

    [Header("Chunk Parameters")]
    public List<Chunk> Chunks = new List<Chunk>();

    [SerializeField] private int _chunkSize = 25;
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

    private bool canUpdate = false;

    private void OnEnable()
    {
        _selectedItem = null;

        // I dont know why this works and i dont want to know why. For some reason using 
        // the code raw from OnEnable() doesnt set up the chunk right but if i do it 
        // async with a delay of 0 seconds it works.
        StartCoroutine(DelayedOnEnable());
    }

    private IEnumerator DelayedOnEnable()
    {
        yield return new WaitForSeconds(0);

        foreach (var chunk in Chunks)
        {
            InitializeBuffers(chunk);
        }

        // Stop Update from running before this completes
        canUpdate = true;
    }

    private void InitializeBuffers(Chunk chunk)
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

    private void OnDisable()
    {
        ReleaseChunkBuffers();
        canUpdate = false;
    }

    private void OnDestroy()
    {
        ReleaseChunk();
    }

    private void LateUpdate()
    {
        if(canUpdate)
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
            float dist = Vector3.Distance(cam.transform.position, Chunks[i].Bounds.center);

            if(dist < _renderDistance)
                Graphics.DrawMeshInstancedIndirect(Chunks[i].Mesh, 0, Chunks[i].Material, Chunks[i].Bounds, Chunks[i].ArgsBuffer, 0, null, _castShadows, _receiveShadows);
        }
    }

    private void ReleaseChunkBuffers()
    {
        for (int i = 0; i < Chunks.Count; ++i)
        {
            ReleaseBuffers(Chunks[i]);
        }
    }

    private void ReleaseBuffers(Chunk chunk)
    {
        // Debug.Log("Chunk " + chunk.ID + " releasing buffers");
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

    private void ReleaseChunk()
    {
        foreach (var chunk in Chunks)
        {
            ReleaseChunkData(chunk);
        }

        // Clear Chunks
        _ChunkCache.Clear();
        Chunks = null;
    }

    private void ReleaseChunkData(Chunk chunk)
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

        // if (chunk.Mesh != null)
        // {
        //     DestroyImmediate(chunk.Mesh);
        //     chunk.Mesh = null;
        // }
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

    public void RemoveChunkDataAtPos(Vector3 position, float brushSize)
    {
        Grid grid = GetComponent<Grid>();

        Vector3Int cellPosition = grid.WorldToCell(position);
        Vector3 center = grid.GetCellCenterWorld(cellPosition);
        Chunk chunk = GetChunk(cellPosition, center);

        Vector3 worldPosition = position - center;

        for (int i = 0; i < chunk.MeshData.Count; i++)
        {
            if(Vector3.Distance(chunk.MeshData[i].Matrix.GetPosition(), worldPosition) < brushSize)
            {
                chunk.MeshData.RemoveAt(i);
            }
        }

        chunk.UpdateBuffers();
    }

    private Chunk GetChunk(Vector3Int cellPosition, Vector3 center)
    {
        string id = GetCellID(cellPosition);

        Chunk chunk = GetChunkDictionary(id, center);
        return chunk;
    }

    private string GetCellID(Vector3Int position)
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

//     public GTGrassRenderer _renderer;

//     void OnEnable()
//     {
//         if(_renderer == null)
//             _renderer = gameObject.AddComponent<GTGrassRenderer>();
//     }
