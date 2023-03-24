using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public partial class GTEditor : Editor
{
    
    private Mesh _mesh;
    private Material _material;

    // GUI
    private bool _grassFoldout = false;

    // [Header("Chunk Parameters")]
    // private int _chunkSize = 25;

    // private float Get2DPerlin(Vector2 position, float offset, float scale)
    // {
    //     return Mathf.PerlinNoise((position.x + 0.1f) / _chunkSize * scale + offset, (position.y + 0.1f) / _chunkSize * scale + offset);
    // }

    private void PlaceGrass(RaycastHit hit)
    {
        Vector3 position = hit.point;

        // Rotation
        Quaternion rotation = Quaternion.identity.WeightedNormal(hit.normal, _brushSettings.BrushNormalWeight).RandomizeAxisRotation(new Vector3(0, 180, 0));
        // Quaternion rotation = Quaternion.identity;

        // Recalculates normal with the weight set to 1 so the normalLimit threshold can be calculated correctly
        float dot = Quaternion.Dot(Quaternion.identity.WeightedNormal(hit.normal), Quaternion.identity);
        if(dot <= _brushSettings.BrushNormalLimit) return;

        // Scale
        // float n = Get2DPerlin(position, 0, 1);
        // Vector3 scale = new Vector3(0, n, 0); // Set Scale from noise
        Vector3 scale = Vector3.one;

        Vector4 color = Color.green;

        _target.UpdateChunkData(position, rotation, scale, color);
    }

    private void RemoveGrass(RaycastHit hit)
    {
        Debug.Log("Remove Grass");
        // Check chunk meshData matrix pos and find positions in range
    }

    public void SelectedGrassGUI()
    {
        if(_grassFoldout = EditorGUILayout.Foldout(_grassFoldout, "Grass Settings"))
        {
            EditorGUILayout.BeginVertical("HelpBox");
            Mesh newMesh = EditorGUILayout.ObjectField(_mesh, typeof(Mesh), false) as Mesh;
            if (newMesh != _mesh)
            {
                _mesh = newMesh;
                _target.GrassRenderer._mesh = newMesh;
            }

            Material newMaterial = EditorGUILayout.ObjectField(_material, typeof(Material), false) as Material;
            if (newMaterial != _material)
            {
                _material = newMaterial;
                _target.GrassRenderer._material = newMaterial;
            }
            EditorGUILayout.EndVertical();
        }
    }
}