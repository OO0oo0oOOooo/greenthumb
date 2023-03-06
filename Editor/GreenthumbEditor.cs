using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

// TODO:
// Palette
// // Palette Display
// // PaletteItem -> _selectedPaletteItem

// Undo

// Saving and loading
// // Palette

// SetScaleMode runs every time an item is placed I couldnt get OnValidate to work

// All of grass stuff

public class GreenthumbEditor : EditorWindow
{
    [SerializeField] private GreenthumbData _data;
    [SerializeField] private GTPalette _selectedPalette;
    [SerializeField] private PaletteData _selectedPaletteItem;

    [SerializeField] private BrushSettings _brushSettings;

    SerializedObject _so;
    SerializedProperty _propSelectedPalette;
    SerializedProperty _propSelectedPaletteItem;
    SerializedProperty _propBrushSettings;

    private bool _showPaletteItems = false;
    private bool _showBrushSettings = false;
    private GUIContent _paletteItemsLabel = new GUIContent("Palette Item");
    private GUIContent _brushSettingsLabel = new GUIContent("Brush Settings");
    private Vector2 _scrollPosition = Vector2.zero;

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
        // _propSelectedPalette = _so.FindProperty("_selectedPalette");
        // _propSelectedPaletteItem = _so.FindProperty("_paletteItem");

        _layer = GreenthumbUtils.CreateLayer(_layer, _backupLayer, "Greenthumb");
        
        Texture _treeIcon = (Texture)Resources.Load("Icons/pine-tree", typeof(Texture));
        Texture _grassIcon = (Texture)Resources.Load("Icons/grass", typeof(Texture));
        Texture _cogIcon = (Texture)Resources.Load("Icons/cog", typeof(Texture));
        _tabs = new GUIContent[] { new GUIContent(_treeIcon), new GUIContent(_grassIcon), new GUIContent(_cogIcon) };

        SceneView.duringSceneGui += this.OnSceneGUI;

        SaveData();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        
        SaveData();
    }

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
        }

        // _selectedPalette = _data.SelectedPalette;
        _brushSettings = _data.BrushSettings;

        _layer = LayerMask.NameToLayer(_data.Layer);
        _backupLayer = LayerMask.NameToLayer(_data.BackupLayer);
        _objParent = _data.ObjParent;
    }

    private void SaveData()
    {
        _so.ApplyModifiedProperties();

        // _data.PaletteItems = _paletteItems;
        _data.BrushSettings = _brushSettings;

        _data.Layer = LayerMask.LayerToName(_layer);
        _data.BackupLayer = LayerMask.LayerToName(_backupLayer);
        _data.ObjParent = _objParent;

        EditorUtility.SetDirty(_data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
        if(_selectedPalette == null)
        {
            BrushGUI();
            PrefabSettingsGUI();
        }
        
        PaletteSelection();

        PaletteDisplay();
    }

    private void GrassGUI()
    {
        GUILayout.Label("Grass");
    }

    private void SettingsGUI()
    {
        using ( new GUILayout.HorizontalScope() )
        {
            GUILayout.Label("Palette");
            _selectedPalette = EditorGUILayout.ObjectField(_selectedPalette, typeof(GTPalette), true) as GTPalette;
        }
        GUILayout.Space(20);

        if(_selectedPalette != null)
        {
            _selectedPaletteItem = _selectedPalette.Palette[0];
        }

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
            // EditorGUILayout.PropertyField(_propBrushSettings);
            EditorGUI.BeginProperty(position, _paletteItemsLabel, _propBrushSettings);

            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushSize"));
            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushDensity"));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushNormalLimit"));
            EditorGUILayout.PropertyField(_propBrushSettings.FindPropertyRelative("_brushNormalWeight"));
        }
        EditorGUILayout.EndVertical();
    }

    private void PrefabSettingsGUI()
    {
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
        }
        EditorGUILayout.EndVertical();
    }

    int selectedPaletteIndex = 0;
    string[] paletteNames = new string[]
    {
        "Palette", "Palette 1", "Palette 2", "Palette 3", "Palette 4", "Palette 5", "Palette 6", "Palette 7", "Palette 8", "Palette 9"
    };

    private void PaletteSelection()
    {
        using ( new GUILayout.HorizontalScope() )
        {
            selectedPaletteIndex = EditorGUILayout.Popup(selectedPaletteIndex, paletteNames, "ToolbarPopup");

            if(GUILayout.Button("New", "ToolbarButton"))
            {
                // Create New Palette
                GTPalette palette = CreateInstance<GTPalette>();

                string filePath = "Assets/Greenthumb/Resources/";
                string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath + "Palette.asset");

                AssetDatabase.CreateAsset(palette, uniqueFileName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("Creating new Palette.");
            }

            if(GUILayout.Button("Del", "ToolbarButton"))
            {
                string paletteName = paletteNames[selectedPaletteIndex];
                string filePath = $"Assets/Greenthumb/Resources/{paletteName}.asset";

                if (AssetDatabase.LoadAssetAtPath<GTPalette>(filePath) != null)
                {
                    // Asset exists
                    AssetDatabase.DeleteAsset(filePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else Debug.Log("Asset doesnt exist.");
            }
        }
    }

    private void PaletteDisplay()
    {
        if(_selectedPalette != null)
        {
            _selectedPaletteItem = _selectedPalette.Palette[0];

            float windowWidth = EditorGUIUtility.currentViewWidth;
            float spacing = 12.5f;
            float itemWidth = windowWidth/2-spacing;
            float itemHeight = windowWidth/2-spacing;

            float maxWidth = Mathf.Clamp(itemWidth, 100, 200);
            float maxHeight = Mathf.Clamp(itemHeight, 100, 200);

            int itemsPerRow = Mathf.FloorToInt((windowWidth + spacing) / (maxWidth + spacing));

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            for (int i = 0; i < _selectedPalette.Palette.Count; i += itemsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < itemsPerRow && i + j < _selectedPalette.Palette.Count; j++)
                {
                    PaletteData item =  _selectedPalette.Palette[i + j];
                    GameObject go = item.Prefab;
                    Texture2D preview = AssetPreview.GetAssetPreview(go);
                    
                    Rect border = GUILayoutUtility.GetRect(maxWidth, maxHeight, GUI.skin.box); // GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)
                    border.x += 2;
                    border.y += 2;
                    border.width -= 4;
                    border.height -= 4;

                    EditorGUI.DrawPreviewTexture(border, preview, null, ScaleMode.ScaleToFit, 0f);
                    
                    // GUIContent content = new GUIContent(preview);
                    
                    // GUIStyle boxStyle = GUI.skin.box;
                    // boxStyle.alignment = TextAnchor.MiddleCenter;

                    // GUILayout.Box(content, boxStyle, GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
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
                Place(0, 0, hit);
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
                Erase(0, 0, hit);
            }
            else
            {
                Paint(0, 0, hit);
            }
        }
    }


    private void Place(int tab, int paletteIndex, RaycastHit hit)
    {
        InstatiatePrefab(hit, _selectedPaletteItem.Prefab, _objParent, _selectedPaletteItem.Scale, _brushSettings.BrushNormalLimit, _brushSettings.BrushNormalWeight, _layer);
    }

    private void Paint(int tab, int paletteIndex, RaycastHit hit)
    {
        Brush(hit, _brushSettings.BrushSize, _brushSettings.BrushDensity, 1, _layer);
    }

    private void Erase(int tab, int palletteIndex, RaycastHit hit)
    {
        RemovePrefab(hit, _brushSettings.BrushSize);
    }


    public void Brush(RaycastHit hit, float brushSize, float density, float distance, int layerID)
    {
        // Calculate the minimum distance between each object based on the desired density
        float minDistance = Mathf.Sqrt(1f / (density * Mathf.PI)) * brushSize;
        int maxObjects = Mathf.FloorToInt(Mathf.PI * brushSize * brushSize * 0.25f * density);

        Vector3 randomPosition = hit.point + UnityEngine.Random.insideUnitSphere * brushSize;

        int layerMask = 1 << layerID;
        Debug.Log(layerMask);
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

            // Debug.Log("Destroying: " + obj.name); // -------------------------------------------------
            DestroyImmediate(obj);
        }
    }

    private void PlaceGrass(RaycastHit hit)
    {
        Debug.Log("Place Grass");
    }

    private void RemoveGrass(RaycastHit hit)
    {
        Debug.Log("Remove Grass");
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