using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
// using System;
using System.IO;

// TODO:
// Palette
// if none selected highlight 0

// Undo

// Saving and loading
// // Palette

// SetScaleMode runs every time an item is placed I couldnt get OnValidate to work

// All of grass stuff

public partial class GreenthumbEditor : EditorWindow
{
    [SerializeField] private GreenthumbData _data;

    // [SerializeField] private GTPalette _selectedPalette;
    public GTPalette GetSelectedPalette
    { 
        get 
        { 
            if(_palettes.Count > 0) 
                return _palettes[_selectedPaletteIndex];

            return null;
        }

    }

    [SerializeField] private PaletteData _selectedPaletteItem;

    [SerializeField] private BrushSettings _brushSettings;

    SerializedObject _so;
    SerializedProperty _propBrushSettings;

    // Palette 
    [SerializeField] List<GTPalette> _palettes = new List<GTPalette>();
    List<string> _paletteNames = new List<string>();
    int _selectedPaletteIndex = 0;
    
    private GUIContent _paletteItemsLabel = new GUIContent("Palette Item");
    private GUIContent _brushSettingsLabel = new GUIContent("Brush Settings");
    private bool _showPaletteItems = false;
    private bool _showBrushSettings = false;

    // Palette Display
    private int _columns = 2;
    private int _padding = 2;
    private float _spacing => _padding*2;
    private int _paddingOffset => _padding / 2;
    private Color _selectionColor = new Color(0, 0, 1, 0.05f);
    private Vector2 _scrollPosition = Vector2.zero;

    // (╯°□°）╯︵ ┻━┻ This doesnt work until the project is recompiled for some reason but using AssetPreview.GetAssetPreview(go); raw is fine...
    // private Dictionary<GameObject, Texture2D> _assetPreviewCache = new Dictionary<GameObject, Texture2D>();
    // private Texture2D GetAssetPreview(GameObject go) 
    // {
    //     if (!_assetPreviewCache.ContainsKey(go))
    //     {
    //         _assetPreviewCache[go] = AssetPreview.GetAssetPreview(go);
    //     }
    //     return _assetPreviewCache[go];
    // }

    private GameObject _objParent;

    private LayerMask _layer;
    private LayerMask _backupLayer;

    // Toolbar
    private enum Tabs
    {
        Tree, Grass, Settings
    }
    private GUIContent[] _tabs;
    private int _tabSelected = -1;

    // Input Event Parameters
    private bool _isMouseDown = false;
    private bool _isButtonHeld = false;
    private float _mouseDownTime;



    void OnEnable()
    {
        LoadData();

        _so = new SerializedObject(this);
        _propBrushSettings = _so.FindProperty("_brushSettings");

        _layer = GreenthumbUtils.CreateLayer(_layer, _backupLayer, "Greenthumb");
        
        Texture treeIcon = (Texture)Resources.Load("Icons/pine-tree", typeof(Texture));
        Texture grassIcon = (Texture)Resources.Load("Icons/grass", typeof(Texture));
        Texture cogIcon = (Texture)Resources.Load("Icons/cog", typeof(Texture));
        _tabs = new GUIContent[] { new GUIContent(treeIcon), new GUIContent(grassIcon), new GUIContent(cogIcon) };

        SceneView.duringSceneGui += this.OnSceneGUI;

        SaveData();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        
        SaveData();
    }
    
    // void OnDestroy()
    // {
    //     _paletteNames.Clear();
    //     _palettes.Clear();
    //     _assetPreviewCache.Clear();

    //     if (_tabs != null)
    //     {
    //         foreach (var tab in _tabs)
    //         {
    //             if (tab.image != null)
    //             {
    //                 Resources.UnloadAsset(tab.image);
    //             }
    //         }
    //     }
    // }

    private void LoadData()
    {
        // _data = AssetDatabase.LoadAssetAtPath<GreenthumbData>("Assets/Greenthumb/Resources/GreenthumbData.asset");
        _data = (GreenthumbData)Resources.Load("GreenThumbData", typeof(GreenthumbData));

        if (_data == null)
        {
            _data = CreateInstance<GreenthumbData>();
            AssetDatabase.CreateAsset(_data, "Assets/Greenthumb/Resources/GreenthumbData.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Remove this after getting it to work with packages
            Debug.Log("Creating new data object.");

            _data.Layer = "Greenthumb";
            _data.BackupLayer = "Water";
        }

        _palettes = _data.PaletteList;

        if(_palettes.Count > 0)
        {
            // _selectedPalette = _palettes[0];
            // _selectedPaletteIndex = 0;
            // _selectedPaletteItem = GetSelectedPalette.Palette[0];

            InitPaletteNames();
        }

        _brushSettings = _data.BrushSettings;

        _layer = LayerMask.NameToLayer(_data.Layer);
        _backupLayer = LayerMask.NameToLayer(_data.BackupLayer);
        _objParent = _data.ObjParent;
    }

    private void SaveData()
    {
        _so.ApplyModifiedProperties();

        _data.PaletteList = _palettes;

        _data.BrushSettings = _brushSettings;

        _data.Layer = LayerMask.LayerToName(_layer);
        _data.BackupLayer = LayerMask.LayerToName(_backupLayer);
        _data.ObjParent = _objParent;

        EditorUtility.SetDirty(_data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void InitPaletteNames()
    {
        foreach (GTPalette p in _palettes)
        {
            _paletteNames.Add(p.name);
        }
    }

    // EDITOR WINDOW INTERACTION
    void OnGUI()
    {
        _so.Update();
        
        using ( new GUILayout.VerticalScope() ) 
        {
            _tabSelected = GUILayout.Toolbar(_tabSelected, _tabs);
        }

        if(_tabSelected >= 0)
        {
            switch(_tabSelected)
            {
                case (int)Tabs.Tree:
                    TreeGUI();
                    break;

                case (int)Tabs.Grass:
                    GrassGUI();
                    break;

                case (int)Tabs.Settings:
                    SettingsGUI();
                    break;
            }
        }

        _so.ApplyModifiedProperties();
    }

    private void TreeGUI()
    {
        BrushGUI();
        SelectedPrefabGUI();
        
        PaletteSelectionGUI();
        PaletteDisplayGUI();
    }

    private void GrassGUI()
    {
        BrushGUI();
        SelectedGrassGUI();
        // GrassPaletteSelection();
        // GrassPaletteDisplay();
    }

    private void SettingsGUI()
    {
        using ( new GUILayout.HorizontalScope() )
        {
            GUILayout.Label("Parent");
            _objParent = EditorGUILayout.ObjectField(_objParent, typeof(GameObject), true) as GameObject;
        }
        GUILayout.Space(20);
        
        GUILayout.Label("Primary Layer");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LayerField(_layer);
        EditorGUI.EndDisabledGroup();

        GUILayout.Label("Backup Layer");
        _backupLayer = EditorGUILayout.LayerField(_backupLayer);
    }

    private void BrushGUI()
    {
        EditorGUILayout.BeginVertical("HelpBox");
        if(_showBrushSettings = EditorGUILayout.Foldout(_showBrushSettings, "Brush Settings"))
        {
            EditorGUI.BeginProperty(position, _brushSettingsLabel, _propBrushSettings);

            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushSize"));
            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushDensity"));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushNormalLimit"));
            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushNormalWeight"));
        }
        EditorGUILayout.EndVertical();
    }

    // Prefab
    private void SelectedPrefabGUI()
    {
        if(GetSelectedPalette == null || GetSelectedPalette.Palette.Count <= 0) return;

        SerializedProperty prop = _so.FindProperty("_selectedPaletteItem");

        EditorGUILayout.BeginVertical("HelpBox");
        if(_showPaletteItems = EditorGUILayout.Foldout(_showPaletteItems, "Palette Item"))
        {
            EditorGUI.BeginProperty(position, _paletteItemsLabel, prop);

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

            if(GUILayout.Button("Delete"))
            {
                if(GetSelectedPalette.Palette.Contains(_selectedPaletteItem))
                    GetSelectedPalette.Palette.Remove(_selectedPaletteItem);
            }
        }
        EditorGUILayout.EndVertical();

        _so.ApplyModifiedProperties();
    }

    private void PaletteSelectionGUI()
    {
        using ( new GUILayout.HorizontalScope() )
        {
            _selectedPaletteIndex = EditorGUILayout.Popup(_selectedPaletteIndex, _paletteNames.ToArray(), "ToolbarPopup");

            if(GUILayout.Button("New", "ToolbarButton"))
            {
                // Create New Palette
                GTPalette palette = CreateInstance<GTPalette>();

                string filePath = "Assets/Greenthumb/Resources/";
                string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath + "Palette.asset");
                string fileName = Path.GetFileNameWithoutExtension(uniqueFileName);

                AssetDatabase.CreateAsset(palette, uniqueFileName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                _paletteNames.Add(fileName);
                _palettes.Add(palette);

                Debug.Log("Creating new Palette.");
            }

            if(GUILayout.Button("Del", "ToolbarButton"))
            {
                if(_palettes.Count <= 0) return;
                
                string paletteName = _paletteNames[_selectedPaletteIndex];
                string filePath = $"Assets/Greenthumb/Resources/{paletteName}.asset";

                if (AssetDatabase.LoadAssetAtPath<GTPalette>(filePath) != null)
                {
                    // Asset exists
                    AssetDatabase.DeleteAsset(filePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    _paletteNames.Remove(paletteName);
                    _palettes.RemoveAt(_selectedPaletteIndex);

                    _selectedPaletteIndex = _palettes.Count > 0 ? _palettes.Count - 1 : 0;
                }
                else Debug.Log("Asset doesnt exist.");
            }
        }
    }

    private void PaletteDisplayGUI()
    {
        if(GetSelectedPalette == null) return; // || 
        if(GetSelectedPalette.Palette.Count == 1) _selectedPaletteItem = GetSelectedPalette.Palette[0]; // Still need to look at this

        Rect area = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        Event evt = Event.current;
        DropArea(area, evt);

        float windowWidth = EditorGUIUtility.currentViewWidth;
        float itemSize = windowWidth/_columns-_spacing;

        int result = GetSelectedPalette.Palette.Count % _columns == 0 ? GetSelectedPalette.Palette.Count : GetSelectedPalette.Palette.Count + 1;
        float totalHeight = (int)(result/2) * itemSize;
        Rect viewRect = new Rect(area.x, area.y, area.width - GUI.skin.verticalScrollbar.fixedWidth, totalHeight + _padding);
        _scrollPosition = GUI.BeginScrollView(area, _scrollPosition, viewRect);
        
        for (int i = 0; i < GetSelectedPalette.Palette.Count; i++)
        {
            int row = i / _columns;
            int column = i % _columns;

            Rect cellRect = new Rect(area.x + column * itemSize, area.y + row * itemSize, itemSize, itemSize);
            cellRect.x += _paddingOffset;
            cellRect.y += _paddingOffset;
            cellRect.width -= _padding;
            cellRect.height -= _padding;

            PaletteData item =  GetSelectedPalette.Palette[i];
            GameObject go = item.Prefab;
            Texture2D preview = AssetPreview.GetAssetPreview(go);

            if(preview != null)
                EditorGUI.DrawPreviewTexture(cellRect, preview, null, ScaleMode.ScaleToFit, 0f);

            if (evt.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition) && evt.button == 0)
            {
                _selectedPaletteItem = item;
                Repaint();
            }

            if (item == _selectedPaletteItem)
            {
                EditorGUI.DrawRect(cellRect, _selectionColor);
            }
        }
        GUI.EndScrollView();

        _so.ApplyModifiedProperties();
    }

    private void DropArea(Rect area, Event evt)
    {
        GUI.Box(area, string.Empty, "HelpBox");
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

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if(draggedObject is GameObject)
                        {
                            PaletteData item = new PaletteData();
                            item.Prefab = (GameObject)draggedObject;
                            item.Width = 1;
                            item.Height = 1;

                            GetSelectedPalette.Palette.Add(item);
                        }   
                    }
                }
                break;
        }
    }


    // SCENE INTERACTION
    void OnSceneGUI(SceneView sceneView)
    {
        if(_tabSelected == (int)Tabs.Settings || _tabSelected == -1)
            return;

        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(worldRay, out RaycastHit hit, 10000))
        {
            if (hit.collider.gameObject != null)
            {
                Handles.DrawLine(hit.point, hit.point + hit.normal);
                Handles.DrawWireDisc(hit.point, hit.normal, _brushSettings.BrushSize);
            }
        }

        HandleInput(_tabSelected, 0, hit);
    }

    private void HandleInput(int tab, int palletteIndex, RaycastHit hit)
    {
        Event e = Event.current;
        if (Event.current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            if(_isMouseDown && !_isButtonHeld)
            {
                Place(0, hit);
            }

            _isMouseDown = false;
            _isButtonHeld = false;

            e.Use();
        }
        else if (e.type == EventType.MouseDown && e.button == 0)
        {
            _isMouseDown = true;
            _mouseDownTime = Time.realtimeSinceStartup;

            e.Use();
        }

        if(_isMouseDown && Time.realtimeSinceStartup - _mouseDownTime > 0.2f)
        {
            _isButtonHeld = true;
            if(e.control)
            {
                Erase(0, hit);
            }
            else
            {
                Paint(0, hit);
            }
        }
    }


    private void Place(int tab, RaycastHit hit)
    {
        switch (_tabSelected)
        {
            case (int)Tabs.Tree:
                InstatiatePrefab(hit, _selectedPaletteItem.Prefab, _objParent, _selectedPaletteItem.Scale, _brushSettings.BrushNormalLimit, _brushSettings.BrushNormalWeight, _layer);
                break;

            case (int)Tabs.Grass:
                PlaceGrass(hit);
                break;
        }
    }

    private void Paint(int tab, RaycastHit hit)
    {
        switch (_tabSelected)
        {
            case (int)Tabs.Tree:
                Brush(hit, _brushSettings.BrushSize, _brushSettings.BrushDensity, 1, _layer);
                break;

            // case (int)Tabs.Grass:
            //     break;
        }
        
    }

    private void Erase(int tab, RaycastHit hit)
    {
        switch (_tabSelected)
        {
            case (int)Tabs.Tree:
                RemovePrefab(hit, _brushSettings.BrushSize);
                break;

            case (int)Tabs.Grass:
                PlaceGrass(hit);
                break;
        }
    }


    public void Brush(RaycastHit hit, float brushSize, float density, float distance, int layerID)
    {
        // Calculate the minimum distance between each object based on the desired density
        float minDistance = Mathf.Sqrt(1f / (density * Mathf.PI)) * brushSize;
        int maxObjects = Mathf.FloorToInt(Mathf.PI * brushSize * brushSize * 0.25f * density);

        Vector3 randomPosition = hit.point + UnityEngine.Random.insideUnitSphere * brushSize;

        int layerMask = 1 << layerID;
        Collider[] hitColliders = Physics.OverlapSphere(randomPosition, brushSize, layerMask);

        //Return if it is too dense
        if(hitColliders.Length >= maxObjects) // maxObjects or (int)density
        {
            // Debug.Log("MAX OBJ");
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
                    // Debug.Log("MIN DIST");
                    return;
                }
            }
            
            // Instatiate
            InstatiatePrefab(hitInfo, _selectedPaletteItem.Prefab, _objParent, _selectedPaletteItem.Scale, _brushSettings.BrushNormalLimit, _brushSettings.BrushNormalWeight, _layer);
        }
        
    }

    private void InstatiatePrefab(RaycastHit hit, GameObject prefab, GameObject parent, Vector3 scale, float normalLimit, float normalWeight, int layer)
    {
        Quaternion rotNormal = Quaternion.identity.WeightedNormal(hit.normal, normalWeight).RandomizeAxisRotation(new Vector3(0, 180, 0));
        Quaternion rot = rotNormal.RandomizeAxisRotation(new Vector3(0, 180, 0));
        float dot = Quaternion.Dot(Quaternion.identity.WeightedNormal(hit.normal), Quaternion.identity);

        if(dot <= normalLimit) { return; }

        scale = SetScaleMode(scale);

        GameObject newObject = _objParent != null ?
            Instantiate(prefab, hit.point, rot, parent.transform) : 
            Instantiate(prefab, hit.point, rot);

        GreenthumbUtils.SetLayerRecursively(newObject, layer);
        newObject.transform.localScale = scale;
    }

    private void RemovePrefab(RaycastHit hit, float size)
    {
        int layerMask = 1 << _layer;
        Collider[] hitColliders = Physics.OverlapSphere(hit.point, size, layerMask);
        foreach (var hitCollider in hitColliders)
        {
            GameObject obj;
            if(_objParent != null)
            {
                if(hitCollider.transform.root.gameObject != _objParent)
                    continue;
                
                obj = GreenthumbUtils.FindRootByParent(hitCollider.gameObject, _objParent);
            }
            else
            {
                obj = GreenthumbUtils.FindRootByLayer(hitCollider.gameObject, _layer.value);
            }

            DestroyImmediate(obj);
        }
    }

    public Vector3 SetScaleMode(Vector3 scale)
    {
        if(_selectedPaletteItem.ActivePrefabScaleMode == PrefabScaleMode.Fixed)
            scale = _selectedPaletteItem.Scale;
        else if(_selectedPaletteItem.ActivePrefabScaleMode == PrefabScaleMode.Weighted)
            scale = _selectedPaletteItem.ScaleWeighted[GreenthumbUtils.WeightedRandom(_selectedPaletteItem.Weights)];

        return scale;
    }

    [MenuItem("Tools/Green thumb")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GreenthumbEditor>("Green thumb");
    }
}