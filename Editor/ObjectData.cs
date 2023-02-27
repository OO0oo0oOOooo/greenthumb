using UnityEngine;

[CreateAssetMenu]
public class ObjectData : ScriptableObject
{
    public GameObject Obj;
    public string ParentName;

    public Vector3[] Scale;
    public int[] weights;
}
