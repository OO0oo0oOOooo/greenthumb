using System;
using UnityEngine;

[Serializable]
public struct MeshData
{
    public Matrix4x4 Matrix;
    public Color Color;

    public static int Size() {
        return
            sizeof(float) * 4 * 4 +
            sizeof(float) * 4;
    }
}