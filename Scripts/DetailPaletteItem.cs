using System;
using UnityEngine;

[Serializable]
public class DetailPaletteItem
{
    public PrefabScaleMode ActivePrefabScaleMode;

    public Mesh ItemMesh;
    public Material ItemMaterial;

    public float YOffset = 0;

    public float Width = 1;
    public float Height = 1;

    public Vector3 Scale
    {
        get { return new Vector3(Width, Height, Width); }
    }
}

