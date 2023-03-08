using UnityEngine;
using System;

[Serializable]
public class BrushSettings
{
    // [Header("Brush Parameters")]
    [SerializeField] [Range(0, 100)] private float _brushSize = 5f;
    [SerializeField] [Range(0, 10)] private float _brushDensity = 2;

    // [Header("Normal Parameters")]
    [Header("")]
    [SerializeField] [Range(0, 1)] private float _brushNormalLimit = 0;
    [SerializeField] [Range(0, 1)] private float _brushNormalWeight = 1;

    public float BrushSize 
    {
        get { return _brushSize; }
        set { _brushSize = value; }
    }
    public float BrushDensity
    {
        get { return _brushDensity; }
        set { _brushDensity = value; }
    }
    public float BrushNormalLimit
    {
        get { return _brushNormalLimit; }
        set { _brushNormalLimit = value; }
    }
    public float BrushNormalWeight
    {
        get { return _brushNormalWeight; }
        set { _brushNormalWeight = value; }
    }
}
