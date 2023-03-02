using UnityEngine;

[CreateAssetMenu]
public class PaletteItemData : ScriptableObject
{
    public PaletteItem PaletteItem;

    public Vector3[] ScaleWeighted;
    public int[] weights;
}