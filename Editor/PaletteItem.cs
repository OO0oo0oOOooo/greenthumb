using System;
using UnityEngine;

[Serializable]
public class PaletteItem
{
    [SerializeField] private GameObject _prefab;

    [SerializeField] private PrefabScaleMode _prefabScaleMode;
    [SerializeField] private Vector3 _scale = Vector3.one;

    [SerializeField] private float _width = 1;
    [SerializeField] private float _height = 1;

    public GameObject Prefab
    {
        get { return _prefab; }
        set { _prefab = value; }
    }

    public PrefabScaleMode ActiveScaleMode
    {
        get{ return _prefabScaleMode; }
        set{ _prefabScaleMode = value; }
    }

    public Vector3 Scale
    {
        get { return _scale; }
        set { _scale = value; }
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
