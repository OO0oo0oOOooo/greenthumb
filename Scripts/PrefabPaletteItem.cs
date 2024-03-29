using System;
using UnityEngine;

[Serializable]
public class PrefabPaletteItem
{
    public PrefabScaleMode ActivePrefabScaleMode;

    public GameObject Prefab;

    public float Width = 1;
    public float Height = 1;

    public Vector3[] ScaleWeighted;
    public int[] Weights;

    public Vector3 Scale
    {
        get { return new Vector3(Width, Height, Width); }
    }
}

