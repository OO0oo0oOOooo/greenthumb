using System;
using UnityEngine;

[Serializable]
public struct MeshData
{
    public Matrix4x4 mat;
    public Color color;

    public static int Size()
    {
        return
            sizeof(float) * 4 * 4 +
            sizeof(float) * 4;
    }
}
