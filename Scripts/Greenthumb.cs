using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Known Issues: In edit mode if the project is saved or any files in the project are Moved/Created/Deleted 
// all meshses rendered with Graphics.DrawMeshInstancedIndirect will vanish until the this script is Disabled->Enabled

[ExecuteAlways]
public class Greenthumb : MonoBehaviour
{
    // TODO: See if you can remove this
    public PrefabPaletteItem _selectedPaletteItem;
    public DetailPaletteItem _selectedDetailItem;

    // Grass Rendering
    [Header("Parameters")]
    public Mesh SelectedMesh;
    public Material SelectedMaterial;

    [Header("Chunk Parameters")]
    [SerializeField] private List<Chunk> Chunks = new List<Chunk>();

    [SerializeField] private int _chunkSize = 25;
    [SerializeField] private float _renderDistanceInChunks = 4;
    private float _renderDistance  => _renderDistanceInChunks * _chunkSize;
    [SerializeField] private bool _debugChunks = false;

    [Header("Shadows")]
    private ShadowCastingMode _castShadows = ShadowCastingMode.Off;
    private bool _receiveShadows = true;

    private Dictionary<string, Dictionary<string, Chunk>> _chunkCache = new Dictionary<string, Dictionary<string, Chunk>>();
    // private Dictionary<string, Chunk> _ChunkCache = new Dictionary<string, Chunk>();
    private Chunk GetChunkDictionary(string id, Vector3 center)
    {
        if (!_chunkCache.ContainsKey(SelectedMesh.name))
        {
            _chunkCache[SelectedMesh.name] = new Dictionary<string, Chunk>();
        }
        
        Dictionary<string, Chunk> chunkDict = _chunkCache[SelectedMesh.name];
        if (!chunkDict.ContainsKey(id))
        {
            Bounds bounds = new Bounds(center, new Vector3(_chunkSize, _chunkSize, _chunkSize));
            Chunk chunk = new Chunk(id, SelectedMesh, bounds, SelectedMaterial);

            chunkDict[id] = chunk;
            Chunks.Add(chunk);
        }

        return chunkDict[id];
    }

    private bool _canUpdate = false;

    private Camera _cam;

    private void OnEnable()
    {
        _selectedPaletteItem = null;

        // I dont know why this works and i dont want to know why. For some reason using 
        // the code raw from OnEnable() doesnt set up the chunk right.
        StartCoroutine(DelayedOnEnable());
        
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += SetCamera;
    }

    private IEnumerator DelayedOnEnable()
    {
        yield return new WaitForSeconds(0);

        foreach (var chunk in Chunks)
        {
            InitializeBuffers(chunk);
        }

        // Stop Update from running before this completes
        _canUpdate = true;
    }

    void SetCamera(UnityEngine.Rendering.ScriptableRenderContext context, Camera camera)
    {
        // Camera cam = null;

        // if(!Application.isPlaying)
        //     cam = SceneView.lastActiveSceneView.camera;
        // else if(Application.isPlaying)
        //     cam = Camera.main;

        // if (_cam == null)
        //     return;

        _cam = camera;       
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
        _canUpdate = false;
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= SetCamera;
    }

    private void OnDestroy()
    {
        ReleaseChunk();
    }

    private void LateUpdate()
    {
        if(_canUpdate)
            RenderChunks();
    }

    private void RenderChunks()
    {
        if (_cam == null)
            return;

        for (int i = 0; i < Chunks.Count; i++)
        {
            float dist = Vector3.Distance(_cam.transform.position, Chunks[i].Bounds.center);

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
        _chunkCache.Clear();
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
