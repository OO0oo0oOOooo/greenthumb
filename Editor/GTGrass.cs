using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

// Plan
// Paint positions


// GRASS
    

public partial class GreenthumbEditor
{
    private Mesh _mesh;
    private Material _material;

    // [Header("Shadows")]
    // private ShadowCastingMode castShadows = ShadowCastingMode.Off;
    // private bool receiveShadows = true;

    // [Header("Chunk Parameters")]
    private int chunksXZ = 5; // Automatically calculate based on the bounds of the paint
    // private int chunksInZ = 5;
    // private float renderDistanceInChunks = 4;
    // private bool debugChunks = false;

    private Vector2 chunkSize = new Vector2Int(100, 100);
    private int population = 0;
    private int _maxChunkPopulation = 1000;

    // private float renderDistance { get => renderDistanceInChunks*chunkSize.x; }
    // private int chunkCount { get => chunksInX*chunksInZ; }
    // // private int chunkPopulation { get => population / chunkCount; }
    private Chunk[] chunks;

    private uint[] args;
    // private Bounds terrainBounds;

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
        args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)_mesh.GetIndexCount(0);
        args[1] = (uint)100;
        args[2] = (uint)_mesh.GetIndexStart(0);
        args[3] = (uint)_mesh.GetBaseVertex(0);

        // InitChunk(0, 0);
    }

    Chunk InitChunk(int xOffset, int zOffset)
    {
        Chunk chunk = new Chunk();

        Vector3 center = Vector3.zero;
        float halfChunkSize = (chunkSize.x * 0.5f);

        center.x = -halfChunkSize;
        center.z = -halfChunkSize;
        center.y = 0;

        Vector3 worldspacePosition = center;
        chunk.bounds = new Bounds(center, new Vector3(chunkSize.x, 100, chunkSize.y));

        MeshData[] meshData = new MeshData[_maxChunkPopulation];
        // for (int i = 0; i < chunkPopulation; i++)
        // {
        //     MeshData mesh = new MeshData();
        //     Vector3 position = Vector3.zero;
        //     Quaternion rotation = Quaternion.identity;
        //     Vector3 scale = Vector3.one;
        //     mesh.mat = Matrix4x4.TRS(position, rotation, scale);
        //     meshData[i] = mesh;
        // }
        int i = 0;

        MeshData mesh = new MeshData();
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        Vector3 scale = Vector3.one;
        mesh.mat = Matrix4x4.TRS(position, rotation, scale);
        meshData[i] = mesh;

        chunk.argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        chunk.meshDataBuffer = new ComputeBuffer(population, MeshData.Size());

        chunk.argsBuffer.SetData(args);
        chunk.meshDataBuffer.SetData(meshData);

        chunk.material = new Material(_material);
        chunk.material.SetBuffer("_Properties", chunk.meshDataBuffer);

        return chunk;
    }

    // void Update()
    // {
    //     if(Camera.main == null)
    //         return;
        
    //     for (int i = 0; i < chunkCount; i++)
    //     {
    //         float dist = Vector3.Distance(Camera.main.transform.position, chunks[i].bounds.center);

    //         if(dist < renderDistance)
    //             Graphics.DrawMeshInstancedIndirect(mesh, 0, chunks[i].material, chunks[i].bounds, chunks[i].argsBuffer, 0, null, castShadows, receiveShadows);
    //     }
    // }

    // void OnDisable()
    // {
    //     for (int i = 0; i < chunkCount; ++i) 
    //     {
    //         ReleaseChunk(chunks[i]);
    //     }

    //     chunks = null;
    // }

    // void ReleaseChunk(Chunk chunk)
    // {
    //     if (chunk.meshDataBuffer != null)
    //     {
    //         chunk.meshDataBuffer.Release();
    //         chunk.meshDataBuffer = null;
    //     }

    //     if (chunk.argsBuffer != null) 
    //     {
    //         chunk.argsBuffer.Release();
    //         chunk.argsBuffer = null;
    //     }
    // }

    // void OnDrawGizmos()
    // {
    //     if(!debugChunks)
    //         return;
    //     Gizmos.color = Color.yellow;
    //     if (chunks != null) 
    //     {
    //         for (int i = 0; i < chunkCount; ++i)
    //         {
    //             Gizmos.DrawWireCube(chunks[i].bounds.center, chunks[i].bounds.size);
    //         }
    //     }

    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireCube(terrainBounds.center, terrainBounds.size);
    // }

    // GUI
    private bool _grassFoldout = false;

    public void SelectedGrassGUI()
    {
        EditorGUILayout.BeginVertical("HelpBox");
        if(_grassFoldout = EditorGUILayout.Foldout(_grassFoldout, "Grass Settings"))
        {
            _mesh = EditorGUILayout.ObjectField(_mesh, typeof(Mesh), false) as Mesh;
            _material = EditorGUILayout.ObjectField(_material, typeof(Material), false) as Material;
        }
        EditorGUILayout.EndVertical();
    }

    private void PlaceGrass(RaycastHit hit)
    {
        Debug.Log("Place Grass");
        population++;

        

    }

    private void RemoveGrass(RaycastHit hit)
    {
        Debug.Log("Remove Grass");
    }
}
