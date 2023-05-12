using System.IO;
using UnityEngine;
using UnityEditor;

public partial class GTEditor
{
    // [SerializeField] private PaletteData<PrefabPalette> _palettePrefab;
    // [SerializeField] private PaletteData<DetailPalette> _paletteDetail;

    [SerializeField] private PPaletteData _palettePrefab;
    [SerializeField] private DPaletteData _paletteDetail;


    // Palette Display
    private int _columns = 2;
    private int _padding = 2;
    private float _spacing => _padding*2;
    private int _paddingOffset => _padding / 2;
    private Color _bgColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
    private Color _selectionColor = new Color(0, 0, 1, 0.1f);
    private Vector2 _scrollPosition = Vector2.zero;

    private GUIContent _paletteItemsLabel = new GUIContent("Palette Item");
    private GUIContent _detailItemsLabel = new GUIContent("Detail Item");
    private bool _showPaletteItems = false;


    // This is not DRY but i am not good enough with Genarics
    #region Prefab Palette GUI 
    private void PrefabPaletteSelectionGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical("toolbar");
        using ( new GUILayout.HorizontalScope() )
        {
            _palettePrefab.SelectedPaletteIndex = EditorGUILayout.Popup(_palettePrefab.SelectedPaletteIndex, _palettePrefab.PaletteNameList.ToArray());

            if(GUILayout.Button("New"))
            {
                // Create New Palette
                PrefabPalette palette = CreateInstance<PrefabPalette>();

                string filePath = "Assets/Greenthumb/Resources/";
                string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath + "PrefabPalette.asset");
                string fileName = Path.GetFileNameWithoutExtension(uniqueFileName);

                AssetDatabase.CreateAsset(palette, uniqueFileName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                _palettePrefab.PaletteNameList.Add(fileName);
                _palettePrefab.PaletteList.Add(palette);

                Debug.Log("Creating new Palette.");
            }

            if(GUILayout.Button("Del"))
            {
                if(_palettePrefab.PaletteList.Count <= 0) return;
                
                string paletteName = _palettePrefab.PaletteNameList[_palettePrefab.SelectedPaletteIndex];
                string filePath = $"Assets/Greenthumb/Resources/{paletteName}.asset";

                if (AssetDatabase.LoadAssetAtPath<PrefabPalette>(filePath) != null)
                {
                    // Asset exists
                    AssetDatabase.DeleteAsset(filePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    _palettePrefab.PaletteNameList.Remove(paletteName);
                    _palettePrefab.PaletteList.RemoveAt(_palettePrefab.SelectedPaletteIndex);

                    _palettePrefab.SelectedPaletteIndex = _palettePrefab.PaletteList.Count > 0 ? _palettePrefab.PaletteNameList.Count - 1 : 0;
                }

                else Debug.Log("Asset doesnt exist.");
            }
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical("HelpBox");

        using ( new GUILayout.HorizontalScope() )
        {
            if(GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25)))
            {
                PaletteAddItem();
            }

            if(GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(25)))
            {
                if(_palettePrefab.GetSelectedPalette.Palette.Contains(_palettePrefab.SelectedPaletteItem))
                    _palettePrefab.GetSelectedPalette.Palette.Remove(_palettePrefab.SelectedPaletteItem);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
    
    private void PrefabAddRemoveItemGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical("HelpBox");

        using ( new GUILayout.HorizontalScope() )
        {
            if(GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25)))
            {
                PaletteAddItem();
            }

            if(GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(25)))
            {
                if(_palettePrefab.GetSelectedPalette.Palette.Contains(_palettePrefab.SelectedPaletteItem))
                    _palettePrefab.GetSelectedPalette.Palette.Remove(_palettePrefab.SelectedPaletteItem);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void PaletteAddItem(UnityEngine.Object draggedObject = null)
    {
        PrefabPaletteItem item = new PrefabPaletteItem();
        item.Prefab = (GameObject)draggedObject;
        item.Width = 1;
        item.Height = 1;

        _palettePrefab.GetSelectedPalette.Palette.Add(item);
    }

    private void PrefabSelectedGUI()
    {
        if(_palettePrefab.GetSelectedPalette == null || _palettePrefab.GetSelectedPalette.Palette.Count <= 0) return;
        _target._selectedPaletteItem = _palettePrefab.SelectedPaletteItem;
        
        SerializedProperty prop = _so.FindProperty("_selectedPaletteItem");

        if(_showPaletteItems = EditorGUILayout.Foldout(_showPaletteItems, "Palette Item"))
        {
            // EditorGUI.BeginChangeCheck();
            Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));
            EditorGUI.BeginProperty(rect, _paletteItemsLabel, prop);

            // Draw the enum popup
            SerializedProperty enumProp = prop.FindPropertyRelative("ActivePrefabScaleMode");
            enumProp.enumValueIndex = EditorGUILayout.Popup("Prefab Scale Mode", enumProp.enumValueIndex, enumProp.enumNames);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("Prefab"));
            GUILayout.Space(10);
        
            // Draw the appropriate fields based on the enum selection
            switch ((PrefabScaleMode)enumProp.enumValueIndex)
            {
                case PrefabScaleMode.Fixed:
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Width"));
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Height"));
                    break;

                case PrefabScaleMode.Weighted:
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("ScaleWeighted"), true);
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Weights"), true);
                    break;
            }
            EditorGUI.EndProperty();
            // if (EditorGUI.EndChangeCheck())
            // {
            //     // Undo.RecordObject(_target, "Changed Item data");
            //     Debug.Log("EndChangeCheck");
            // }
        }

        _so.ApplyModifiedProperties();
    }

    private void PrefabPaletteDisplayGUI()
    {
        if(_palettePrefab.GetSelectedPalette == null) return;
        if(_palettePrefab.GetSelectedPalette.Palette.Count == 1) _palettePrefab.SelectedPaletteItem = _palettePrefab.GetSelectedPalette.Palette[0];

        // PrefabAddRemoveItemGUI();
        
        float itemSize = Screen.width/_columns-_spacing-_paddingOffset;

        int result = _palettePrefab.GetSelectedPalette.Palette.Count % _columns == 0 ? _palettePrefab.GetSelectedPalette.Palette.Count : _palettePrefab.GetSelectedPalette.Palette.Count + 1;
        float totalHeight = (int)(result/2) * itemSize;
        totalHeight = totalHeight < itemSize ? itemSize : totalHeight;

        // rect is here to tell the inspector that the space is being used and to grab the y for the area rect.
        Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(totalHeight));
        Rect area = new Rect(0, rect.y, Screen.width, totalHeight);

        Event evt = Event.current;
        PrefabDropArea(area, evt);

        Rect viewRect = new Rect(area.x, area.y, area.width - GUI.skin.verticalScrollbar.fixedWidth, totalHeight + _padding);
        _scrollPosition = GUI.BeginScrollView(area, _scrollPosition, viewRect);

        for (int i = 0; i < _palettePrefab.GetSelectedPalette.Palette.Count; i++)
        {
            int row = i / _columns;
            int column = i % _columns;

            Rect cellRect = new Rect(area.x + column * itemSize, area.y + row * itemSize, itemSize, itemSize);
            cellRect.x += _paddingOffset;
            cellRect.y += _paddingOffset;
            cellRect.width -= _padding;
            cellRect.height -= _padding;

            PrefabPaletteItem item = _palettePrefab.GetSelectedPalette.Palette[i];
            GameObject go = item.Prefab;
            Texture2D preview = AssetPreview.GetAssetPreview(go);
            EditorGUI.DrawRect(cellRect, _bgColor);

            if(preview != null)
                EditorGUI.DrawPreviewTexture(cellRect, preview, null, ScaleMode.ScaleToFit, 0f);

            if (evt.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition) && evt.button == 0)
            {
                _palettePrefab.SelectedPaletteItem = item;
                Repaint();
            }

            if (item == _palettePrefab.SelectedPaletteItem)
            {
                EditorGUI.DrawRect(cellRect, _selectionColor);
            }
        }
        GUI.EndScrollView();
    }

    private void PrefabDropArea(Rect area, Event evt)
    {
        GUI.Box(area, string.Empty);
        // GUI.Box(area, string.Empty, "HelpBox"); 
        if(_palettePrefab.GetSelectedPalette.Palette.Count == 0)
            GUI.Label(area, "Drop", EditorStyles.centeredGreyMiniLabel);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if(draggedObject is GameObject)
                        {
                            PaletteAddItem(draggedObject);
                        }   
                    }
                }
                break;
        }
    }
    #endregion


    #region  Detail Palette GUI
    private void DetailPaletteSelectionGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical("toolbar");
        using ( new GUILayout.HorizontalScope())
        {
            _paletteDetail.SelectedPaletteIndex = EditorGUILayout.Popup(_paletteDetail.SelectedPaletteIndex, _paletteDetail.PaletteNameList.ToArray());

            if(GUILayout.Button("New")) //, GUILayout.Height(25) // The lid for the plastic caps that handle the size adjustment is broken
            {
                // Create New Palette
                DetailPalette palette = CreateInstance<DetailPalette>();

                string filePath = "Assets/Greenthumb/Resources/";
                string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath + "DetailPalette.asset");
                string fileName = Path.GetFileNameWithoutExtension(uniqueFileName);

                AssetDatabase.CreateAsset(palette, uniqueFileName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                _paletteDetail.PaletteNameList.Add(fileName);
                _paletteDetail.PaletteList.Add(palette);

                Debug.Log("Creating new Palette.");
            }

            if(GUILayout.Button("Del")) //, GUILayout.Height(25)
            {
                if(_paletteDetail.PaletteList.Count <= 0) return;
                
                string paletteName = _paletteDetail.PaletteNameList[_paletteDetail.SelectedPaletteIndex];
                string filePath = $"Assets/Greenthumb/Resources/{paletteName}.asset";

                if (AssetDatabase.LoadAssetAtPath<DetailPalette>(filePath) != null)
                {
                    // Asset exists
                    AssetDatabase.DeleteAsset(filePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    _paletteDetail.PaletteNameList.Remove(paletteName);
                    _paletteDetail.PaletteList.RemoveAt(_paletteDetail.SelectedPaletteIndex);

                    _paletteDetail.SelectedPaletteIndex = _paletteDetail.PaletteList.Count > 0 ? _paletteDetail.PaletteNameList.Count - 1 : 0;
                }

                else Debug.Log("Asset doesnt exist.");
            }
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical("HelpBox");

        using ( new GUILayout.HorizontalScope() )
        {
            if(GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25))) // "miniButtonLeft",
            {
                DetailAddItem();
            }

            if(GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(25))) // "miniButtonRight",
            {
                if(_paletteDetail.GetSelectedPalette.Palette.Contains(_paletteDetail.SelectedPaletteItem))
                {
                    _paletteDetail.GetSelectedPalette.Palette.Remove(_paletteDetail.SelectedPaletteItem);
                    _target.ReleaseChunksWithThisMesh(_target.SelectedMesh);
                }
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    // TODO: Add the GUI for highlighted Palette Items
    private void DetailSelectedGUI()
    {
        if(_paletteDetail.GetSelectedPalette == null || _paletteDetail.GetSelectedPalette.Palette.Count <= 0) return;
        _target._selectedDetailItem = _paletteDetail.SelectedPaletteItem;
        
        SerializedProperty prop = _so.FindProperty("_selectedDetailItem");

        if(_showPaletteItems = EditorGUILayout.Foldout(_showPaletteItems, "Detail Items"))
        {
            // EditorGUI.BeginChangeCheck();
            Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));
            EditorGUI.BeginProperty(rect, _detailItemsLabel, prop);

            _propMesh = prop.FindPropertyRelative("ItemMesh");
            EditorGUILayout.PropertyField(_propMesh);
            _target.SelectedMesh = _propMesh.objectReferenceValue as Mesh;

            // _propMaterial = prop.FindPropertyRelative("ItemMaterial");
            // EditorGUILayout.PropertyField(_propMaterial);
            // _target.SelectedMaterial = _propMaterial.objectReferenceValue as Material;

            GUILayout.Space(10);

            // Draw the enum popup
            SerializedProperty enumProp = prop.FindPropertyRelative("ActivePrefabScaleMode");
            enumProp.enumValueIndex = EditorGUILayout.Popup("Prefab Scale Mode", enumProp.enumValueIndex, enumProp.enumNames);

            // Draw the appropriate fields based on the enum selection
            switch ((PrefabScaleMode)enumProp.enumValueIndex)
            {
                case PrefabScaleMode.Fixed:
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Width"));
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Height"));
                    break;

                case PrefabScaleMode.Weighted:
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("ScaleWeighted"), true);
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Weights"), true);
                    break;
            }
            EditorGUI.EndProperty();
            // if (EditorGUI.EndChangeCheck())
            // {
            //     // Undo.RecordObject(_target, "Changed Item data");
            //     Debug.Log("EndChangeCheck");
            // }
        }

        _so.ApplyModifiedProperties();
    }

    private void DetailAddRemoveItemGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical("HelpBox");
        using ( new GUILayout.HorizontalScope() )
        {
            if(GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25)))
            {
                DetailAddItem();
            }

            if(GUILayout.Button("-", GUILayout.Width(25), GUILayout.Height(25)))
            {
                if(_paletteDetail.GetSelectedPalette.Palette.Contains(_paletteDetail.SelectedPaletteItem))
                {
                    _paletteDetail.GetSelectedPalette.Palette.Remove(_paletteDetail.SelectedPaletteItem);
                    _target.ReleaseChunksWithThisMesh(_target.SelectedMesh);
                }
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void DetailAddItem(Mesh mesh = null)
    {
        DetailPaletteItem item = new DetailPaletteItem();
        item.ItemMesh = mesh;
        item.Width = 1;
        item.Height = 1;

        _paletteDetail.GetSelectedPalette.Palette.Add(item);
    }

    private void DetailPaletteDisplayGUI()
    {
        if(_paletteDetail.GetSelectedPalette == null) return;
        if(_paletteDetail.GetSelectedPalette.Palette.Count == 1) _paletteDetail.SelectedPaletteItem = _paletteDetail.GetSelectedPalette.Palette[0];

        // DetailAddRemoveItemGUI();
        
        float itemSize = Screen.width/_columns-_spacing-_paddingOffset;

        int result = _paletteDetail.GetSelectedPalette.Palette.Count % _columns == 0 ? _paletteDetail.GetSelectedPalette.Palette.Count : _paletteDetail.GetSelectedPalette.Palette.Count + 1;
        float totalHeight = (int)(result/2) * itemSize;
        totalHeight = totalHeight < itemSize ? itemSize : totalHeight;

        // rect is here to tell the inspector that the space is being used and to grab the y for the area rect.
        Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(totalHeight));
        Rect area = new Rect(0, rect.y, Screen.width, totalHeight);

        Event evt = Event.current;
        DetailDropArea(area, evt);

        Rect viewRect = new Rect(area.x, area.y, area.width - GUI.skin.verticalScrollbar.fixedWidth, totalHeight + _padding);
        _scrollPosition = GUI.BeginScrollView(area, _scrollPosition, viewRect);

        for (int i = 0; i < _paletteDetail.GetSelectedPalette.Palette.Count; i++)
        {
            int row = i / _columns;
            int column = i % _columns;

            Rect cellRect = new Rect(area.x + column * itemSize, area.y + row * itemSize, itemSize, itemSize);
            cellRect.x += _paddingOffset;
            cellRect.y += _paddingOffset;
            cellRect.width -= _padding;
            cellRect.height -= _padding;

            DetailPaletteItem item = _paletteDetail.GetSelectedPalette.Palette[i];
            Mesh go = item.ItemMesh;
            Texture2D preview = AssetPreview.GetAssetPreview(go);
            EditorGUI.DrawRect(cellRect, _bgColor);

            if(preview != null)
                EditorGUI.DrawPreviewTexture(cellRect, preview, null, ScaleMode.ScaleToFit, 0f);

            if (evt.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition) && evt.button == 0)
            {
                _paletteDetail.SelectedPaletteItem = item;
                Repaint();
            }

            if (item == _paletteDetail.SelectedPaletteItem)
            {
                EditorGUI.DrawRect(cellRect, _selectionColor);
            }
        }
        GUI.EndScrollView();
    }

    private void DetailDropArea(Rect area, Event evt)
    {
        GUI.Box(area, string.Empty);
        // GUI.Box(area, string.Empty, "HelpBox"); 
        if(_paletteDetail.GetSelectedPalette.Palette.Count == 0)
            GUI.Label(area, "Drop", EditorStyles.centeredGreyMiniLabel);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if(draggedObject is Mesh)
                        {
                            DetailAddItem((Mesh)draggedObject);
                        }
                    }
                }
                break;
        }
    }
    #endregion
}