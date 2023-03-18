using UnityEngine;


public static class ExtensionMethods
{   
    /// <summary>
    /// lerp towards normal by the weight parameter
    /// </summary>
    public static Quaternion WeightedNormal(this Quaternion rot, Vector3 normal, float weight = 1)
    {
        Vector3 xCross = Vector3.Cross(normal, Vector3.forward);
        Vector3 zCross = Vector3.Cross(xCross, normal);
        Quaternion normalRot = Quaternion.LookRotation(zCross, normal);
        
        return Quaternion.Lerp(rot, normalRot, weight);
    }

    /// <summary>
    /// rotation * Random.Range(-dimension, dimension);
    /// </summary>
    public static Quaternion RandomizeAxisRotation(this Quaternion rotation, Vector3 range )
    {
        Quaternion rot = rotation * Quaternion.Euler(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y), Random.Range(-range.z, range.z));
        return rot;
    }

    // public static Random WeightedRandom(this Random random)
    // {

    // }
}
