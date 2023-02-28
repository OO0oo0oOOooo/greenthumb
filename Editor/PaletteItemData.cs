using UnityEngine;

[CreateAssetMenu]
public class PaletteItemData : ScriptableObject
{
    public GameObject Obj;
    public string ParentName;

    public Vector3[] Scale;
    public int[] weights;
}
