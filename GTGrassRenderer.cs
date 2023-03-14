using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GTGrassRenderer : MonoBehaviour
{
    [Header("Chunk Parameters")]
    // [SerializeField] private Chunk[] _chunks;
    [SerializeField] private List<Chunk> _chunks = new List<Chunk>();
    [SerializeField] private Vector2Int _chunkSize = new Vector2Int(25, 25); // this is also inside the EditorWindow Script and that is dumb and bad
    [SerializeField] private float _renderDistanceInChunks = 4;
    private float _renderDistance  => _renderDistanceInChunks * _chunkSize.x;
    [SerializeField] private bool _debugChunks = false;


    [Header("Shadows")]
    private ShadowCastingMode _castShadows = ShadowCastingMode.Off;
    private bool _receiveShadows = true;

    public void ThxBro(Chunk[] chunks)
    {
        foreach (Chunk chunk in chunks)
        {
            AddChunk(chunk);
        }
    }

    public void AddChunk(Chunk chunk)
    {
        _chunks.Add(chunk);
    }

    private void OnDestroy()
    {
        ReleaseChunks();
    }

    private void Update()
    {
        RenderChunks();
    }

    private void RenderChunks()
    {
        if(Camera.main == null)
            return;

        for (int i = 0; i < _chunks.Count; i++)
        {
            float dist = Vector3.Distance(Camera.main.transform.position, _chunks[i].bounds.center);

            if(dist < _renderDistance)
                Graphics.DrawMeshInstancedIndirect(_chunks[i].mesh, 0, _chunks[i].material, _chunks[i].bounds, _chunks[i].argsBuffer, 0, null, _castShadows, _receiveShadows);
        }
    }

    void ReleaseChunks()
    {
        // OnDisable
        for (int i = 0; i < _chunks.Count; ++i) 
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

        Gizmos.color = Color.green;
        if (_chunks != null)
        {
            for (int i = 0; i < _chunks.Count; ++i)
            {
                Gizmos.DrawWireCube(_chunks[i].bounds.center, _chunks[i].bounds.size);
            }
        }

        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
