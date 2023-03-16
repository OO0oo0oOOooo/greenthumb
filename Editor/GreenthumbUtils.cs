using UnityEngine;
using UnityEditor;

public static class GreenthumbUtils
{

    public static int CreateLayer(LayerMask layer, LayerMask backupLayer, string nullLayerInitName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty propLayersArr = tagManager.FindProperty("layers");

        string layerName = LayerMask.LayerToName(layer);

        if(string.IsNullOrEmpty(layerName))
        {
            layerName = nullLayerInitName;
        }

        int layerIndex = -1;
        for (int i = 8; i < propLayersArr.arraySize; i++)
        {
            SerializedProperty propLayer = propLayersArr.GetArrayElementAtIndex(i);
            if (propLayer.stringValue == layerName)
            {
                // The layer already exists
                // Debug.Log("Layer exists: " + propLayer.stringValue + " ID: " + i);
                return i;
            }
            if (propLayer.stringValue == "")
            {
                // Empty layer found
                layerIndex = i;
                break;
            }
        }

        if (layerIndex == -1)
        {
            // No unused layer indices found
            Debug.Log("No available layers. Defaulting to: " + LayerMask.LayerToName(backupLayer));
            return backupLayer;
        }

        SerializedProperty newLayer = propLayersArr.GetArrayElementAtIndex(layerIndex);
        newLayer.stringValue = layerName;
        tagManager.ApplyModifiedProperties();

        // Debug.Log("Created new layer: " + newLayer.stringValue + " ID: " + layerIndex);
        return layerIndex;
    }

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        // Set the layer of the current object
        obj.layer = newLayer;

        // Recursively set the layer of all child objects
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public static GameObject FindRootByParent(GameObject obj, GameObject desiredParent)
    {
        if (obj == null)
        {
            return null;
        }

        while(obj.transform.parent != null && obj.transform.parent.gameObject != desiredParent)
        {
            obj = obj.transform.parent.gameObject;
        }

        return obj;
    }

    public static GameObject FindRootByLayer(GameObject obj, int layer)
    {
        if (obj == null) return null;

        while(obj.transform.parent != null && obj.transform.parent.gameObject.layer == layer)
        {
            obj = obj.transform.parent.gameObject;
        }

        return obj;
    }

    public static int WeightedRandom(int[] weights)
    {
        int weightTotal = 0;
        foreach ( int w in weights ) {
            weightTotal += w;
        }

        int result = 0, total = 0;
        int randVal = Random.Range( 0, weightTotal );
        for ( result = 0; result < weights.Length; result++ ) {
            total += weights[result];
            if ( total > randVal ) break;
        }

        return result;
    }
}