using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// TODO:
// Palette
// // Palette Item
// // Palette Item list
// // Palette Item scalemode

// Undo

// Saving and loading
// // Palette

// SetScaleMode runs every time an item is placed I couldnt get OnValidate to work

// All of grass stuff

public class GreenthumbEditor : EditorWindow
{
    [SerializeField] private GreenthumbData _data;
    [SerializeField] private BrushSettings _brushSettings;

    SerializedObject _so;
    SerializedProperty _propPaletteItems;
    SerializedProperty _propBrushSettings;
    SerializedProperty _propPaletteItem;

    [SerializeField] private List<PaletteItemData> _paletteItems = new List<PaletteItemData>();
    [SerializeField] private PaletteItemData _selectedPaletteItem;
    [SerializeField] private PaletteItem _paletteItem;

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
        
        if(_paletteItems.Count != 0)
        {
            _selectedPaletteItem = _paletteItems[0];
            _paletteItem = _selectedPaletteItem.PaletteItem;
        }

        _so = new SerializedObject(this);
        _propPaletteItems = _so.FindProperty("_paletteItems");
        _propBrushSettings = _so.FindProperty("_brushSettings");
        _propPaletteItem = _so.FindProperty("_paletteItem");

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
        _data = AssetDatabase.LoadAssetAtPath<GreenthumbData>("Assets/Greenthumb/Resources/GreenthumbData.asset");

        if (_data == null)
        {
            _data = CreateInstance<GreenthumbData>();
            AssetDatabase.CreateAsset(_data, "Assets/Greenthumb/Resources/GreenthumbData.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Remove this after getting it to work with packages
            Debug.Log("Creating new data object.");
        }

        _paletteItems = _data.PaletteItems;
        _brushSettings = _data.BrushSettings;

        _layer = LayerMask.NameToLayer(_data.Layer);
        _backupLayer = LayerMask.NameToLayer(_data.BackupLayer);
        _objParent = _data.ObjParent;
    }

    private void SaveData()
    {
        _so.ApplyModifiedProperties();

        _data.PaletteItems = _paletteItems;
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
    }

    private void BrushGUI()
    {
        EditorGUILayout.PropertyField(_propBrushSettings);

        GUILayout.Space(20);

        // GUILayout.Label("Object Size");
        // _scaleOption = (ScaleOption)EditorGUILayout.EnumPopup(_scaleOption);

        // if(_selectedPaletteItem != null)
        // {
        //     if(_scaleOption == ScaleOption.Slider)
        //     {
        //         using ( new GUILayout.HorizontalScope() )
        //         {
        //             GUILayout.Label("Width");
        //             _selectedPaletteItem.Scale.x = EditorGUILayout.FloatField(_selectedPaletteItem.Scale.x);
        //         }

        //         using ( new GUILayout.HorizontalScope() )
        //         {
        //             GUILayout.Label("Height");
        //             _selectedPaletteItem.Scale.y = EditorGUILayout.FloatField(_selectedPaletteItem.Scale.y);
        //         }
        //     }
        // }
    }

    private void TreeGUI()
    {
        PaletteDisplay();

        GUILayout.Space(20);

        BrushGUI();
    }

    private void GrassGUI()
    {
        GUILayout.Label("Grass");
    }

    private void SettingsGUI()
    {
        GUILayout.Space(20);

        using ( new GUILayout.HorizontalScope() ) 
        {
            _objParent = EditorGUILayout.ObjectField("", _objParent, typeof(GameObject), true) as GameObject;
        }
        
        GUILayout.Space(20);
        
        // Should be ok to enable if data serialized. right now i think it would recreate the "Greenthumb" Layer every OnEnable() if it was removed
        GUILayout.Label("Primary Layer");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LayerField(_layer);
        EditorGUI.EndDisabledGroup();

        GUILayout.Label("Backup Layer");
        _backupLayer = EditorGUILayout.LayerField(_backupLayer);
    }

    private void PaletteDisplay()
    {
        // Content Pallette
        EditorGUILayout.PropertyField(_propPaletteItems);
        _selectedPaletteItem = EditorGUILayout.ObjectField("", _selectedPaletteItem, typeof(PaletteItemData), true) as PaletteItemData;

        EditorGUILayout.PropertyField(_propPaletteItem);

        // private GUIContent[] _tabs;

        //  = new GUIContent[] { new GUIContent(_treeIcon), new GUIContent(_grassIcon), new GUIContent(_cogIcon) };

        // GUILayout.SelectionGrid(0, _tabs, 2);

        // Rect border = new Rect(50, 50, 50, 50);

        // GameObject go = _selectedPaletteItem.Prefab;
        // Texture2D preview = AssetPreview.GetAssetPreview(go);

        // EditorGUI.DrawPreviewTexture(border, preview, null, ScaleMode.ScaleToFit, 0f);
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
        InstatiatePrefab(hit, _paletteItem.Prefab, _objParent, _paletteItem.Scale, _brushSettings.BrushNormalLimit, _brushSettings.BrushNormalWeight, _layer);
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
            InstatiatePrefab(hitInfo, _paletteItem.Prefab, _objParent, _paletteItem.Scale, _brushSettings.BrushNormalLimit, _brushSettings.BrushNormalWeight, _layer);
        }
        
    }

    private void InstatiatePrefab(RaycastHit hit, GameObject prefab, GameObject parent, Vector3 scale, float normalLimit, float normalWeight, int layer)
    {
        Quaternion rotNormal = Quaternion.identity.WeightedNormal(hit.normal, normalWeight).RandomizeAxisRotation(new Vector3(0, 180, 0));
        Quaternion rot = rotNormal.RandomizeAxisRotation(new Vector3(0, 180, 0));
        float dot = Quaternion.Dot(Quaternion.identity.WeightedNormal(hit.normal), Quaternion.identity);

        if(dot <= normalLimit) { return; }

        scale = SetScaleMode(scale);

        GameObject newObject = Instantiate(prefab, hit.point, rot, parent.transform);
        GreenthumbUtils.SetLayerRecursively(newObject, layer);
        newObject.transform.localScale = scale;
    }

    private void RemovePrefab(RaycastHit hit, float size)
    {
        int layerMask = 1 << _layer;
        Collider[] hitColliders = Physics.OverlapSphere(hit.point, size, layerMask);
        foreach (var hitCollider in hitColliders)
        {
            // Double check incase of stray Objects in this layer
            if(hitCollider.transform.root.gameObject != _objParent)
                continue;

            GameObject obj = GreenthumbUtils.FindDesiredRoot(hitCollider.gameObject, _objParent);

            // Double protection in case i remove one of these while working on stuff
            if(hitCollider.transform.root.gameObject == _objParent)
            {
                // Debug.Log(obj.name);
                DestroyImmediate(obj);
            }
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
        if(_paletteItem.ActiveScaleMode == PrefabScaleMode.Normal)
            scale = _paletteItem.Scale;
        else if(_paletteItem.ActiveScaleMode == PrefabScaleMode.RandomWeighted)
            scale = _selectedPaletteItem.ScaleWeighted[GreenthumbUtils.WeightedRandom(_selectedPaletteItem.weights)];

        return scale;
    }

    [MenuItem("Tools/Green thumb")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GreenthumbEditor>("Green thumb");
    }
}