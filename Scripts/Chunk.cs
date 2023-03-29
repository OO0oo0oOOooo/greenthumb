using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Chunk
{
    public string ID;
    public Mesh Mesh;
    public Bounds Bounds;
    public Material Material;

    public List<MeshData> MeshData;
    // public int Population;

    public ComputeBuffer ArgsBuffer;
    public ComputeBuffer MeshDataBuffer;
    
    public int MaxChunkPopulation = 1000;

    // Constructor
    public Chunk(string id, Mesh mesh, Bounds bounds, Material material) //  List<MeshData> meshData, uint population,  ComputeBuffer argsBuffer, ComputeBuffer meshDataBuffer,
    {
        // Mesh newMesh = new Mesh();
        // newMesh.vertices = mesh.vertices;
        // newMesh.triangles = mesh.triangles;
        // newMesh.normals = mesh.normals;
        // newMesh.uv = mesh.uv;

        // this.Mesh = newMesh;
        this.ID = id;
        this.Mesh = mesh;
        this.Material = new Material(material);
        this.Bounds = bounds;

        this.MeshData = new List<MeshData>();
        // this.Population = 0;

        uint[] args = InitArgs();

        // Create Buffers
        this.ArgsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        this.MeshDataBuffer = new ComputeBuffer(MaxChunkPopulation, global::MeshData.Size());
        // , ComputeBufferType.Append this.meshDataBuffer.SetCounterValue(0);
    }

    private uint[] InitArgs()
    {
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)this.Mesh.GetIndexCount(0);
        args[1] = (uint)MeshData.Count;
        args[2] = (uint)this.Mesh.GetIndexStart(0);
        args[3] = (uint)this.Mesh.GetBaseVertex(0);

        return args;
    }

    public void UpdateBuffers()
    {
        MeshDataBuffer.SetData(MeshData);
        ArgsBuffer.SetData(InitArgs());

        Material.SetBuffer("_Properties", MeshDataBuffer);
    }
}