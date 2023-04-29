using UnityEngine;
using UnityEditor;

public partial class GTEditor : Editor
{
    private Mesh _mesh;
    private Material _material;

    private bool _grassFoldout = false;

    private float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / 25 * scale + offset, (position.y + 0.1f) / 25 * scale + offset);
    }

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
        float n = Get2DPerlin(position, 0, 1);
        Vector3 scaleN = new Vector3(0, n, 0);
        Vector3 scale = Vector3.one + scaleN;
        // Debug.Log(scale.y);

        Vector4 color = Color.green;

        _target.UpdateChunkDataIndirect(position, rotation, scale, color);
    }

    private void RemoveGrass(RaycastHit hit)
    {
        _target.RemoveChunkDataAtPos(hit.point, _brushSettings.BrushSize);
    }

    private void BrushGrass(RaycastHit hit, float distance, int layerID)
    {
        Vector3 randomPosition = hit.point + UnityEngine.Random.insideUnitSphere * _brushSettings.BrushSize;

        // Get exact postion of new object before checking distance
        if (Physics.Raycast(randomPosition + hit.normal, -hit.normal, out RaycastHit hitInfo, distance))
        {
            PlaceGrass(hitInfo);
        }
    }
    
    private void SelectedGrassGUI()
    {
        

        if(_grassFoldout = EditorGUILayout.Foldout(_grassFoldout, "Grass Settings"))
        {
            EditorGUILayout.BeginVertical("HelpBox");
            Mesh newMesh = EditorGUILayout.ObjectField(_propMesh.objectReferenceValue, typeof(Mesh), false) as Mesh;
            if (newMesh != _propMesh.objectReferenceValue)
            {
                _propMesh.objectReferenceValue = newMesh;
                _target.SelectedMesh = newMesh;
            }

            Material newMaterial = EditorGUILayout.ObjectField(_propMaterial.objectReferenceValue, typeof(Material), false) as Material;
            if (newMaterial != _propMaterial.objectReferenceValue)
            {
                _propMaterial.objectReferenceValue = newMaterial;
                _target.SelectedMaterial = newMaterial;
            }
            EditorGUILayout.EndVertical();
        }
    }
}