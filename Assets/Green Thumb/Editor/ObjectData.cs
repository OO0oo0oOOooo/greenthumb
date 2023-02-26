using UnityEngine;

[CreateAssetMenu]
public class ObjectData : ScriptableObject
{
    public GameObject Obj;
    public GameObject ObjParent;

    public Vector3[] Scale;
    public int[] weights;
}
