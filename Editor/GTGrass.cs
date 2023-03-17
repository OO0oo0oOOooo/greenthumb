using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;

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
    private int _maxChunkPopulation = 1000;

    // private float _renderDistance { get => _renderDistanceInChunks * _chunkSize.x; }
    // private int _chunkCount { get => _chunks.Length; }
    // private int _chunkPopulation { get => _population / _chunkCount; }
    // private Chunk[] _chunks;

    private uint[] _args;

    private Dictionary<int, Chunk> _ChunkCache = new Dictionary<int, Chunk>();
    private Chunk GetChunkDictionary(Vector3 center, int xIndex, int zIndex)
    {
        int i = xIndex + zIndex;

        if (!_ChunkCache.ContainsKey(i))
        {
            _ChunkCache[i] = InitChunk(center, i);
        }

        return _ChunkCache[i];
    }

    private void LoadChunksFromRenderer(Chunk[] arr)
    {
        if(_obj == null) return;
        _ChunkCache.Clear();

        foreach (var item in _obj.GetComponent<GTGrassRenderer>().Chunks)
        {
            _ChunkCache[item.id] = item;
        }
    }
    // s
    private void ClearChunkCache()
    {
        foreach (KeyValuePair<int, Chunk> kvp in _ChunkCache)
        {
            // Im 90% sure this is overkill but that last 10% keeps me up at night
            // if(kvp.Value.meshDataBuffer != null)
            // {
            //     kvp.Value.meshDataBuffer.Release();
            // }

            // if(kvp.Value.argsBuffer != null)
            // {
            //     kvp.Value.argsBuffer.Release();
            // }

            // if(kvp.Value.meshData != null)
            // {
            //     kvp.Value.meshData.Clear();
            // }

            // Release and set buffers to null
            Chunk chunk = kvp.Value;

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

            if(chunk.meshData != null)
            {
                chunk.meshData.Clear();
                chunk.meshData = null;
            }

            _ChunkCache[kvp.Key] = chunk;
        }
        _ChunkCache.Clear();
    }

    private void InitArgs()
    {
        _args = new uint[5] { 0, 0, 0, 0, 0 };
        _args[0] = (uint)_mesh.GetIndexCount(0);
        _args[1] = (uint)_maxChunkPopulation;
        _args[2] = (uint)_mesh.GetIndexStart(0);
        _args[3] = (uint)_mesh.GetBaseVertex(0);
    }

    Chunk InitChunk(Vector3 center, int id)
    {
        if(_args == null) InitArgs();

        Chunk chunk = new Chunk();

        chunk.id = id;
        chunk.mesh = _mesh;
        chunk.population = 0;
        chunk.meshData = new List<MeshData>();
        chunk.bounds = new Bounds(center, new Vector3(_chunkSize, 100, _chunkSize));

        // Create Buffers
        chunk.argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        chunk.meshDataBuffer = new ComputeBuffer(_maxChunkPopulation, MeshData.Size(), ComputeBufferType.Append);
        chunk.meshDataBuffer.SetCounterValue(0);

        // Set Buffer Data
        chunk.argsBuffer.SetData(_args);

        // Create Material
        chunk.material = new Material(_material);

        return chunk;
    }

    private void UpdateBuffers(Chunk chunk)
    {
        chunk.population++;
        // if(chunk.population > _maxChunkPopulation)
        // {
        //     chunk.population--;
        //     return;
        // }

        // if(chunk.population < 0) return;
        // // Compare Y of new item and bounds and reset bounds.
        // if(chunk.meshData[(int)chunk.population-1].mat.GetPosition().y > chunk.bounds.size.y)
        // {
        //     chunk.bounds.size = new Vector3(chunk.bounds.size.x, chunk.meshData[(int)chunk.population].mat.GetPosition().y, chunk.bounds.size.z);
        // }
        // else if(chunk.meshData[(int)chunk.population].mat.GetPosition().y < chunk.bounds.center.y)
        // {
        //     chunk.bounds.size = new Vector3(chunk.bounds.center.x, chunk.meshData[(int)chunk.population].mat.GetPosition().y, chunk.bounds.center.z);
        // }

        // I think this would add all the items in chunk.meshdata not just the new entrys.
        // chunk.meshDataBuffer.SetCounterValue(chunk.population);
        // chunk.meshDataBuffer.SetData(chunk.meshData);

        // TODO: Add the new items to the buffer only. chunk.meshDataBuffer.SetData(chunk.meshData, chunk.population, chunk.population, chunk.meshData.Length);
        chunk.meshDataBuffer.SetData(chunk.meshData);

        chunk.material.SetBuffer("_Properties", chunk.meshDataBuffer);

        // TODO: Set _chunks in GTGrassRenderer
        if(_obj != null)
        {
            Chunk[] chunkArray = _ChunkCache.Values.ToArray();
            _obj.GetComponent<GTGrassRenderer>().SetChunks(chunkArray);
        }
    }

    private float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / _chunkSize * scale + offset, (position.y + 0.1f) / _chunkSize * scale + offset);
    }

    private void PlaceGrass(RaycastHit hit)
    {
        // Pos
        // Vector3 position = hit.point;
        Vector3 position = Vector3.zero;

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

        Chunk chunk = GetChunk(position);
        chunk.meshData.Add(mesh);

        UpdateBuffers(chunk);
    }

    private void RemoveGrass(RaycastHit hit)
    {
        // Debug.Log("Remove Grass");
        // Check chunk matrix and find positions in range
    }

    private Chunk GetChunk(Vector3 point)
    {
        point = GetChunkCenterFromPoint(point);

        int xIndex, zIndex;
        GetIndicesFromPoint(point, out xIndex, out zIndex);

        Chunk chunk = GetChunkDictionary(point, xIndex, zIndex);

        return chunk;
    }

    private void GetIndicesFromPoint(Vector3 point, out int xIndex, out int zIndex)
    {
        // Get Chunk index from position
        xIndex = Mathf.FloorToInt(point.x / _chunkSize);
        zIndex = Mathf.FloorToInt(point.z / _chunkSize);
    }

    private Vector3 GetChunkCenterFromPoint(Vector3 point)
    {
        float halfChunkSize = (_chunkSize / 2f);
        
        float x = Mathf.RoundToInt(point.x);
        float z = Mathf.RoundToInt(point.z);

    

        float cx = Mathf.RoundToInt((point.x / _chunkSize) * _chunkSize) + halfChunkSize;
        float cz = Mathf.RoundToInt((point.z / _chunkSize) * _chunkSize) + halfChunkSize;
        
        return new Vector3(cx, -10, cz);
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
