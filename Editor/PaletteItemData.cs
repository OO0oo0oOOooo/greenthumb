using UnityEngine;

[CreateAssetMenu]
public class PaletteItemData : ScriptableObject
{
    public GameObject Prefab;
    public Vector3 ScaleSlider = Vector3.one;

    public Vector3[] ScaleWeighted;
    public int[] weights;
}