using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GreenThumbUtils
{
    public static void GenerateNewLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        // Find an unused layer index
        int unusedLayer = -1;
        for (int i = 0; i < layersProp.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layersProp.GetArrayElementAtIndex(i).stringValue))
            {
                unusedLayer = i;
                break;
            }
        }

        // If all layers are used, log an error message and return
        if (unusedLayer == -1)
        {
            Debug.LogError("Could not create a new layer. All layer indices are already used.");
            return;
        }

        // Set the name and index of the new layer
        layersProp.GetArrayElementAtIndex(unusedLayer).stringValue = layerName;
        layersProp.serializedObject.ApplyModifiedProperties();
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