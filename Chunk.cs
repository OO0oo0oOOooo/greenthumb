using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Chunk 
{
    public Mesh mesh;
    public List<MeshData> meshData;
    public uint population;

    public ComputeBuffer argsBuffer;
    public ComputeBuffer meshDataBuffer;
    public Bounds bounds;
    public Material material;
}
