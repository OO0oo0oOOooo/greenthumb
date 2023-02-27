using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GreenThumbUtils
{
    public static int CreateLayer(string layerName, int backupLayerIndex)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        // Find the first unused layer index
        int layerIndex = -1;
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (layer.stringValue == layerName)
            {
                // The layer already exists
                // Debug.Log("Layer exists: " + i + ", " + layer.stringValue);
                return i;
            }
            if (layer.stringValue == "")
            {
                layerIndex = i;
                break;
            }
        }

        if (layerIndex == -1)
        {
            // No unused layer indices found
            Debug.Log("No available layers. Defaulting to: " + LayerMask.LayerToName(backupLayerIndex));
            return backupLayerIndex;
        }

        // Set the layer name and save the changes
        SerializedProperty layerProp = layers.GetArrayElementAtIndex(layerIndex);
        layerProp.stringValue = layerName;
        tagManager.ApplyModifiedProperties();

        Debug.Log("Created new layer: " + layerProp.stringValue + ", " + layerIndex);
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

    public static GameObject FindDesiredRoot(GameObject obj, GameObject desiredParent)
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