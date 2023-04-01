using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public partial class GTEditor
{
//     [SerializeField] private PrefabPalette _selectedPalette;
//     public PrefabPalette GetSelectedPalette
//     {
//         get
//         {
//             if(_palettes.Count > 0)
//                 return _palettes[_selectedPaletteIndex];

//             return null;
//         }
//     }

//     [SerializeField] private PrefabPaletteItem _selectedPaletteItem;

//     // Palette
//     [SerializeField] private List<PrefabPalette> _palettes = new List<PrefabPalette>();
//     private List<string> _paletteNames = new List<string>();
//     private int _selectedPaletteIndex = 0;

//     private GUIContent _paletteItemsLabel = new GUIContent("Palette Item");
//     private bool _showPaletteItems = false;
// // Palette Display
//     private int _columns = 2;
//     private int _padding = 2;
//     private float _spacing => _padding*2;
//     private int _paddingOffset => _padding / 2;
//     private Color _selectionColor = new Color(0, 0, 1, 0.05f);
//     private Vector2 _scrollPosition = Vector2.zero;

    private LayerMask _layer;
    private LayerMask _backupLayer;

    // // (╯°□°）╯︵ ┻━┻ This doesnt work until the project is recompiled for some reason but using AssetPreview.GetAssetPreview(go); raw is fine...
    // private Dictionary<GameObject, Texture2D> _assetPreviewCache = new Dictionary<GameObject, Texture2D>();
    // private Texture2D GetAssetPreview(GameObject go) 
    // {
    //     if (!_assetPreviewCache.ContainsKey(go))
    //     {
    //         _assetPreviewCache[go] = AssetPreview.GetAssetPreview(go);
    //     }
    //     return _assetPreviewCache[go];
    // }

    // GUI
    // private void SelectedPrefabGUI()
    // {
    //     if(GetSelectedPalette == null || GetSelectedPalette.Palette.Count <= 0) return;
    //     _target._selectedItem = _selectedPaletteItem;
        
    //     SerializedProperty prop = _so.FindProperty("_selectedItem");

    //     if(_showPaletteItems = EditorGUILayout.Foldout(_showPaletteItems, "Palette Item"))
    //     {
    //         // EditorGUI.BeginChangeCheck();
    //         Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));
    //         EditorGUI.BeginProperty(rect, _paletteItemsLabel, prop);

    //         // Draw the enum popup
    //         SerializedProperty enumProp = prop.FindPropertyRelative("ActivePrefabScaleMode");
    //         enumProp.enumValueIndex = EditorGUILayout.Popup("Prefab Scale Mode", enumProp.enumValueIndex, enumProp.enumNames);
    //         EditorGUILayout.PropertyField(prop.FindPropertyRelative("Prefab"));
    //         GUILayout.Space(10);
        
    //         // Draw the appropriate fields based on the enum selection
    //         switch ((PrefabScaleMode)enumProp.enumValueIndex)
    //         {
    //             case PrefabScaleMode.Fixed:
    //                 EditorGUILayout.PropertyField(prop.FindPropertyRelative("Width"));
    //                 EditorGUILayout.PropertyField(prop.FindPropertyRelative("Height"));
    //                 break;

    //             case PrefabScaleMode.Weighted:
    //                 EditorGUILayout.PropertyField(prop.FindPropertyRelative("ScaleWeighted"), true);
    //                 EditorGUILayout.PropertyField(prop.FindPropertyRelative("Weights"), true);
    //                 break;
    //         }
    //         EditorGUI.EndProperty();
    //         // if (EditorGUI.EndChangeCheck())
    //         // {
    //         //     // Undo.RecordObject(_target, "Changed Item data");
    //         //     Debug.Log("EndChangeCheck");
    //         // }

    //         if(GUILayout.Button("Delete"))
    //         {
    //             if(GetSelectedPalette.Palette.Contains(_selectedPaletteItem))
    //                 GetSelectedPalette.Palette.Remove(_selectedPaletteItem);
    //         }
    //     }

    //     _so.ApplyModifiedProperties();
    // }

    // private void PaletteSelectionGUI()
    // {
    //     using ( new GUILayout.HorizontalScope() )
    //     {
    //         _selectedPaletteIndex = EditorGUILayout.Popup(_selectedPaletteIndex, _paletteNames.ToArray()); // "ToolbarPopup"

    //         if(GUILayout.Button("New")) //, "ToolbarButton"
    //         {
    //             // Create New Palette
    //             PrefabPalette palette = CreateInstance<PrefabPalette>();

    //             string filePath = "Assets/Greenthumb/Resources/";
    //             string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath + "Palette.asset");
    //             string fileName = Path.GetFileNameWithoutExtension(uniqueFileName);

    //             AssetDatabase.CreateAsset(palette, uniqueFileName);
    //             AssetDatabase.SaveAssets();
    //             AssetDatabase.Refresh();

    //             _paletteNames.Add(fileName);
    //             _palettes.Add(palette);

    //             Debug.Log("Creating new Palette.");
    //         }

    //         if(GUILayout.Button("Del")) //, "ToolbarButton"
    //         {
    //             if(_palettes.Count <= 0) return;
                
    //             string paletteName = _paletteNames[_selectedPaletteIndex];
    //             string filePath = $"Assets/Greenthumb/Resources/{paletteName}.asset";

    //             if (AssetDatabase.LoadAssetAtPath<PrefabPalette>(filePath) != null)
    //             {
    //                 // Asset exists
    //                 AssetDatabase.DeleteAsset(filePath);
    //                 AssetDatabase.SaveAssets();
    //                 AssetDatabase.Refresh();

    //                 _paletteNames.Remove(paletteName);
    //                 _palettes.RemoveAt(_selectedPaletteIndex);

    //                 _selectedPaletteIndex = _palettes.Count > 0 ? _palettes.Count - 1 : 0;
    //             }
    //             else Debug.Log("Asset doesnt exist.");
    //         }
    //     }
    // }

    // private void PaletteDisplayGUI()
    // {
    //     if(GetSelectedPalette == null) return;
    //     if(GetSelectedPalette.Palette.Count == 1) _selectedPaletteItem = GetSelectedPalette.Palette[0];
        
    //     float itemSize = Screen.width/_columns-_spacing-_paddingOffset;

    //     int result = GetSelectedPalette.Palette.Count % _columns == 0 ? GetSelectedPalette.Palette.Count : GetSelectedPalette.Palette.Count + 1;
    //     float totalHeight = (int)(result/2) * itemSize;
    //     totalHeight = totalHeight < itemSize ? itemSize : totalHeight;

    //     // rect is here to tell the inspector that the space is being used and to grab the y for the area rect.
    //     Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(totalHeight));
    //     Rect area = new Rect(0, rect.y, Screen.width, totalHeight);

    //     Event evt = Event.current;
    //     DropArea(area, evt);

    //     Rect viewRect = new Rect(area.x, area.y, area.width - GUI.skin.verticalScrollbar.fixedWidth, totalHeight + _padding);
    //     _scrollPosition = GUI.BeginScrollView(area, _scrollPosition, viewRect);

    //     for (int i = 0; i < GetSelectedPalette.Palette.Count; i++)
    //     {
    //         int row = i / _columns;
    //         int column = i % _columns;

    //         Rect cellRect = new Rect(area.x + column * itemSize, area.y + row * itemSize, itemSize, itemSize);
    //         cellRect.x += _paddingOffset;
    //         cellRect.y += _paddingOffset;
    //         cellRect.width -= _padding;
    //         cellRect.height -= _padding;

    //         PrefabPaletteItem item =  GetSelectedPalette.Palette[i];
    //         GameObject go = item.Prefab;
    //         Texture2D preview = AssetPreview.GetAssetPreview(go);

    //         if(preview != null)
    //             EditorGUI.DrawPreviewTexture(cellRect, preview, null, ScaleMode.ScaleToFit, 0f);

    //         if (evt.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition) && evt.button == 0)
    //         {
    //             _selectedPaletteItem = item;
    //             Repaint();
    //         }

    //         if (item == _selectedPaletteItem)
    //         {
    //             EditorGUI.DrawRect(cellRect, _selectionColor);
    //         }
    //     }
    //     GUI.EndScrollView();
    // }

    // private void DropArea(Rect area, Event evt)
    // {
    //     GUI.Box(area, string.Empty);
    //     // GUI.Box(area, string.Empty, "HelpBox"); 
    //     if(GetSelectedPalette.Palette.Count == 0)
    //         GUI.Label(area, "Drop", EditorStyles.centeredGreyMiniLabel);

    //     switch (evt.type)
    //     {
    //         case EventType.DragUpdated:
    //         case EventType.DragPerform:
    //             if (!area.Contains(evt.mousePosition))
    //                 return;

    //             DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

    //             if (evt.type == EventType.DragPerform)
    //             {
    //                 DragAndDrop.AcceptDrag();

    //                 foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
    //                 {
    //                     if(draggedObject is GameObject)
    //                     {
    //                         PrefabPaletteItem item = new PrefabPaletteItem();
    //                         item.Prefab = (GameObject)draggedObject;
    //                         item.Width = 1;
    //                         item.Height = 1;

    //                         GetSelectedPalette.Palette.Add(item);
    //                     }   
    //                 }
    //             }
    //             break;
    //     }
    // }

    private void BrushPrefab(RaycastHit hit, float distance, int layerID)
    {
        // // Calculate the minimum distance between each object based on the desired density
        float minDistance = Mathf.Sqrt(1f / (_brushSettings.BrushDensity * Mathf.PI)) * _brushSettings.BrushSize;
        int maxObjects = Mathf.FloorToInt(Mathf.PI * _brushSettings.BrushSize * _brushSettings.BrushSize * 0.25f * _brushSettings.BrushDensity);

        Vector3 randomPosition = hit.point + UnityEngine.Random.insideUnitSphere * _brushSettings.BrushSize;

        int layerMask = 1 << layerID;
        Collider[] hitColliders = Physics.OverlapSphere(randomPosition, _brushSettings.BrushSize, layerMask);

        //Return if it is too dense
        if(hitColliders.Length >= maxObjects) // maxObjects or (int)density
        {
            return;
        }

        // Get exact postion of new object before checking distance
        if (Physics.Raycast(randomPosition + hit.normal, -hit.normal, out RaycastHit hitInfo, distance))
        {
            // Check if the new object overlaps with any existing objects in the layer
            foreach (Collider hitCollider in hitColliders)
            {
                if (Vector3.Distance(hitInfo.point, hitCollider.transform.position) < minDistance)
                {
                    return;
                }
            }
            
            // Instatiate
            // InstatiatePrefab(hitInfo, _selectedPaletteItem.Prefab, _target.gameObject, _selectedPaletteItem.Scale);
            InstatiatePrefab(hitInfo, _target.gameObject);
        }
        
    }

    private void InstatiatePrefab(RaycastHit hit, GameObject parent)
    {
        Quaternion rot = Quaternion.identity.WeightedNormal(hit.normal, _brushSettings.BrushNormalWeight).RandomizeAxisRotation(new Vector3(0, 180, 0));
        // Quaternion rot = rotNormal.RandomizeAxisRotation(new Vector3(0, 180, 0));

        // recalculates normal with the weight set to 1 so the normalLimit threshold can be calculated correctly
        float dot = Quaternion.Dot(Quaternion.identity.WeightedNormal(hit.normal), Quaternion.identity);
        if(dot <= _brushSettings.BrushNormalLimit) return;

        // This might not neet to take in a scale
        Vector3 scale = SetScaleMode();

        GameObject newObject = Instantiate(_palettePrefab.SelectedPaletteItem.Prefab, hit.point, rot, parent.transform);

        GreenthumbUtils.SetLayerRecursively(newObject, _layer);
        newObject.transform.localScale = scale;
    }

    private void RemovePrefab(RaycastHit hit, float size)
    {
        int layerMask = 1 << _layer;
        Collider[] hitColliders = Physics.OverlapSphere(hit.point, size, layerMask);
        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider.transform.root.gameObject != _target.gameObject)
                continue;
            
            GameObject obj = GreenthumbUtils.FindRootByParent(hitCollider.gameObject, _target.gameObject);

            DestroyImmediate(obj);
        }
    }

    private Vector3 SetScaleMode()
    {
        Vector3 scale = Vector3.zero;

        if(_palettePrefab.SelectedPaletteItem.ActivePrefabScaleMode == PrefabScaleMode.Fixed)
            scale = _palettePrefab.SelectedPaletteItem.Scale;
        else if(_palettePrefab.SelectedPaletteItem.ActivePrefabScaleMode == PrefabScaleMode.Weighted)
            scale = _palettePrefab.SelectedPaletteItem.ScaleWeighted[GreenthumbUtils.WeightedRandom(_palettePrefab.SelectedPaletteItem.Weights)];

        return scale;
    }
}
