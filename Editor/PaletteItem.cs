using System;
using UnityEngine;

[Serializable]
public class PaletteItem
{
    [SerializeField] private GameObject _prefab;

    public PrefabScaleMode PrefabScaleMode;

    [SerializeField] private float _width = 1;
    [SerializeField] private float _height = 1;

    public Vector3[] ScaleWeighted;
    public int[] Weights;

    public GameObject Prefab
    {
        get { return _prefab; }
        set { _prefab = value; }
    }

    public Vector3 Scale
    {
        get { return new Vector3(_width, _height, _width); }
    }

    public float Width
    {
        get { return _width; }
        set { _width = value; }
    }

    public float Height
    {
        get { return _height; }
        set { _height = value; }
    }
}
