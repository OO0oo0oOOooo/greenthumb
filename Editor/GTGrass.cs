using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public partial class GreenthumbEditor
{
    private Mesh _mesh;
    private Material _material;

    // GUI
    private bool _grassFoldout = false;

    [Header("Shadows")]
    private ShadowCastingMode _castShadows = ShadowCastingMode.Off;
    private bool _receiveShadows = true;

    [Header("Chunk Parameters")]
    private int _chunksXZ = 5; // Automatically calculate based on the bounds of the paint
    private Vector2Int _chunkSize = new Vector2Int(25, 25);
    private int _population = 0;
    private int _maxChunkPopulation = 1000;
    private float _renderDistanceInChunks = 4;
    private bool _debugChunks = false;


    private float _renderDistance { get => _renderDistanceInChunks * _chunkSize.x; }
    private int _chunkCount { get => _chunksXZ * _chunksXZ; }
    private int _chunkPopulation { get => _population / _chunkCount; }
    private Chunk[] _chunks;

    private uint[] _args;
    // private Bounds bounds;

    private Dictionary<int, Chunk> _ChunkCache = new Dictionary<int, Chunk>();
    private Chunk GetChunkDictionary(int i, int xIndex, int zIndex)
    {
        if (!_ChunkCache.ContainsKey(i))
        {
            _ChunkCache[i] = InitChunk(xIndex, zIndex);
        }

        return _ChunkCache[i];
    }

    private struct Chunk 
    {
        public ComputeBuffer argsBuffer;
        public ComputeBuffer meshDataBuffer;
        public Bounds bounds;
        public Material material;
    }

    private struct MeshData
    {
        public Matrix4x4 mat;

        public static int Size()
        {
            return
                sizeof(float) * 4 * 4;
        }
    }

    private void InitArgsArr()
    {
        _args = new uint[5] { 0, 0, 0, 0, 0 };
        _args[0] = (uint)_mesh.GetIndexCount(0);
        _args[1] = (uint)_maxChunkPopulation;
        _args[2] = (uint)_mesh.GetIndexStart(0);
        _args[3] = (uint)_mesh.GetBaseVertex(0);
    }

    // void InitializeChunks()
    // {
    //     _chunks = new Chunk[_chunkCount];

    //     for (int x = 0; x < _chunksXZ; ++x) {
    //         for (int y = 0; y < _chunksXZ; ++y)
    //         {
    //             _chunks[x * (_chunksXZ) + y] = InitChunk(x, y);
    //         }
    //     }
    // }

    Chunk InitChunk(int xOffset, int zOffset)
    {
        Chunk chunk = new Chunk();

        Vector3 center = Vector3.zero;
        float halfChunkSize = (_chunkSize.x * 0.5f);

        center.x = (-(halfChunkSize * _chunksXZ) + _chunkSize.x * xOffset) + halfChunkSize;
        center.z = (-(halfChunkSize * _chunksXZ) + _chunkSize.y * zOffset) + halfChunkSize;
        center.y = 0;

        // Center probably needs to be reevaluated

        chunk.bounds = new Bounds(center, new Vector3(_chunkSize.x, 100, _chunkSize.y));

        // MeshData[] meshData = new MeshData[_chunkPopulation];

        // for (int i = 0; i < _chunkPopulation; i++)
        // {
        //     MeshData mesh = new MeshData();

        //     Vector3 position = Vector3.zero;
        //     Quaternion rotation = Quaternion.identity;
        //     Vector3 scale = Vector3.one;

        //     mesh.mat = Matrix4x4.TRS(position, rotation, scale);
        //     meshData[i] = mesh;
        // }

        // Create Buffers
        chunk.argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        chunk.meshDataBuffer = new ComputeBuffer(_maxChunkPopulation, MeshData.Size());

        // Set Buffer Data
        chunk.argsBuffer.SetData(_args);

        // Create Material
        chunk.material = new Material(_material);

        return chunk;
    }

    private void UpdateBuffers(Chunk chunk, MeshData[] meshData)
    {
        // Can update bounds if i want, mostly useful for the pos.y of any grass
        // Vector3 size = new Vector3(chunk.bounds.size.x, 0, chunk.bounds.size.z)
        // chunk.bounds = new Bounds(chunk.bounds.center, size);

        // I dont know if i should Dispose and recreate the MeshDataBuffer Every time i update the amount of items in the chunk.

        // Create Buffers
        // chunk.argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        // chunk.meshDataBuffer = new ComputeBuffer(_population, MeshData.Size());

        // Set Buffer Data
        // chunk.argsBuffer.SetData(_args);
        chunk.meshDataBuffer.SetData(meshData);

        // Set Material Data
        // chunk.material = new Material(_material); // also should be done in InitChunk
        chunk.material.SetBuffer("_Properties", chunk.meshDataBuffer);
    }

    // void Update()
    // {
    //     RenderChunks();
    // }

    private void RenderChunks()
    {
        if(Camera.main == null)
            return;

        for (int i = 0; i < _chunkCount; i++)
        {
            float dist = Vector3.Distance(Camera.main.transform.position, _chunks[i].bounds.center);

            if(dist < _renderDistance)
                Graphics.DrawMeshInstancedIndirect(_mesh, 0, _chunks[i].material, _chunks[i].bounds, _chunks[i].argsBuffer, 0, null, _castShadows, _receiveShadows);
        }
    }

    void ReleaseChunks()
    {
        // OnDisable
        for (int i = 0; i < _chunkCount; ++i) 
        {
            ReleaseChunk(_chunks[i]);
        }

        _chunks = null;
    }

    void ReleaseChunk(Chunk chunk)
    {
        if (chunk.meshDataBuffer != null)
        {
            chunk.meshDataBuffer.Release();
            chunk.meshDataBuffer = null;
        }

        if (chunk.argsBuffer != null) 
        {
            chunk.argsBuffer.Release();
            chunk.argsBuffer = null;
        }
    }

    void OnDrawGizmos()
    {
        if(!_debugChunks)
            return;

        Gizmos.color = Color.yellow;
        if (_chunks != null)
        {
            for (int i = 0; i < _chunkCount; ++i)
            {
                Gizmos.DrawWireCube(_chunks[i].bounds.center, _chunks[i].bounds.size);
            }
        }

        // Gizmos.color = Color.green;
        // Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private void PlaceGrass(RaycastHit hit)
    {
        Debug.Log("Place Grass");
        _population++;

        // Position, Rotation, Scale
        Vector3 position = hit.point;
        Quaternion rotation = Quaternion.identity;
        Vector3 scale = Vector3.one;

        // GetChunk Or create new Chunk
        Chunk chunk = GetChunk(position);

        // Fill in chunk data
        // Draw to chunk meshdata[]
        // Update Buffers
        MeshData[] meshData = new MeshData[_population]; // maxChunkPop or pop?
        UpdateBuffers(chunk, meshData);
    }

    private Chunk GetChunk(Vector3 point)
    {
        // Get Chunk index from position
        int xIndex = (int)point.x / _chunkSize.x;
        int yIndex = (int)point.z / _chunkSize.y;
        int index = xIndex + yIndex;

        Chunk chunk = GetChunkDictionary(index, xIndex, yIndex);

        return chunk;
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
            _mesh = EditorGUILayout.ObjectField(_mesh, typeof(Mesh), false) as Mesh;
            _material = EditorGUILayout.ObjectField(_material, typeof(Material), false) as Material;
            // if(GUILayout.Button("Initialized Chunks"))
            //     InitializeChunks();
        }
        EditorGUILayout.EndVertical();
    }

}
