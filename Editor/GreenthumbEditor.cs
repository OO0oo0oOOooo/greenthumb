using UnityEngine;
using UnityEditor;
using System;

// TODO:
// Palette
// // _treePreset

// Undo

// Saving and loading
// // Parent, Palette, Layers

// All of grass stuff

public class GreenthumbEditor : EditorWindow
{
    private GUIContent[] _tabs;
    private int _tabSelected = -1;

    private enum Tabs
    {
        Tree, Grass, Settings
    }

    ScaleOption _scaleOption;
    private enum ScaleOption
    {
        Slider, RandomWeighted
    }

    // Brush Settings

    [Range(0, 100)]
    private float _brushSize = 5f;

    [Range(0, 10)]
    private float _brushDensity = 2;

    // Normal Settomgs
    [Range(0, 1)]
    private float _brushNormalLimit = 0;

    [Range(0, 1)] // 0 Normal has no effect, 1 Fully faces normal
    private float _brushNormalWeight = 1;


    // Object Settings
    private float _width = 0.75f;
    private float _height = 1;


    [SerializeField] private GreenthumbData _data;
    private PaletteItemData _treePreset;
    private PaletteItemData[] _paletteItems;
    private GameObject _objParent;

    // layer
    private LayerMask _defaultlayer;
    private LayerMask _backupLayer = 4;
    private string _defaultLayerName = "Greenthumb";


    // Input Event Parameters
    private bool _isMouseDown = false;
    private bool _isButtonHeld = false;
    
    private float _mouseDownTime;

    void OnEnable()
    {
        LoadData();
        
        // TODO: Look into this because i am saving the layer now
        _defaultlayer = GreenthumbUtils.CreateLayer(_defaultLayerName, _backupLayer);

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
            Debug.Log("Creating new GreenthumbData");
        }

        _paletteItems = _data.PaletteItems;
        _defaultlayer = _data.DefaultLayer;
        _backupLayer = _data.BackupLayer;
        _objParent = _data.ObjParent;
    }

    private void SaveData()
    {
        _data.PaletteItems = _paletteItems;
        _data.DefaultLayer = _defaultlayer;
        _data.BackupLayer = _backupLayer;
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
        GUILayout.Label("Brush Settings");
        using ( new GUILayout.HorizontalScope() ) 
        {
            GUILayout.Label("Brush Size");
            _brushSize = EditorGUILayout.Slider(_brushSize, 0, 100);
        }

        using ( new GUILayout.HorizontalScope() ) 
        {
            GUILayout.Label("Density");
            _brushDensity = EditorGUILayout.Slider(_brushDensity, 0, 10);
        }

        GUILayout.Space(20);

        GUILayout.Label("Normal Settings");
        using ( new GUILayout.HorizontalScope() )
        {
            GUILayout.Label("Normal Limit");
            _brushNormalLimit = EditorGUILayout.Slider(_brushNormalLimit, 0, 1);
        }
        using ( new GUILayout.HorizontalScope() )
        {
            GUILayout.Label("Normal Weight");
            _brushNormalWeight = EditorGUILayout.Slider(_brushNormalWeight, 0, 1);
        }

        GUILayout.Space(20);

        GUILayout.Label("Object Size");
        _scaleOption = (ScaleOption)EditorGUILayout.EnumPopup(_scaleOption);

        if(_scaleOption == ScaleOption.Slider)
        {
            using ( new GUILayout.HorizontalScope() )
            {
                GUILayout.Label("Width");
                _width = EditorGUILayout.FloatField(_width);
            }

            using ( new GUILayout.HorizontalScope() )
            {
                GUILayout.Label("Height");
                _height = EditorGUILayout.FloatField(_height);
            }
        }
    }

    private void TreeGUI()
    {
        // Content Pallette
        _treePreset = EditorGUILayout.ObjectField("", _treePreset, typeof(PaletteItemData), true) as PaletteItemData;

        GUILayout.Space(20);

        // Default Brush GUI
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
        EditorGUILayout.LayerField(_defaultlayer);
        EditorGUI.EndDisabledGroup();

        GUILayout.Label("Backup Layer");
        _backupLayer = EditorGUILayout.LayerField(_backupLayer);
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
                Handles.DrawWireDisc(hit.point, hit.normal, _brushSize);
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

    private void Place(int tab, int palletteIndex, RaycastHit hit)
    {
        InstatiatePrefab(hit);
    }

    private void Paint(int tab, int palletteIndex, RaycastHit hit)
    {
        Brush(hit, _brushSize, _brushDensity, 1, _defaultlayer);
    }

    private void Erase(int tab, int palletteIndex, RaycastHit hit)
    {
        RemovePrefab(hit);
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
            InstatiatePrefab(hitInfo);
        }
        
    }

    private void InstatiatePrefab(RaycastHit hit)
    {
        Quaternion rotNormal = Quaternion.identity.WeightedNormal(hit.normal, _brushNormalWeight).RandomizeAxisRotation(new Vector3(0, 180, 0));
        Quaternion rot = rotNormal.RandomizeAxisRotation(new Vector3(0, 180, 0));
        float dot = Quaternion.Dot(Quaternion.identity.WeightedNormal(hit.normal), Quaternion.identity);

        if(dot <= _brushNormalLimit)
        {
            return;
        }

        GameObject newObject = Instantiate(_treePreset.Obj, hit.point, rot, _objParent.transform);

        GreenthumbUtils.SetLayerRecursively(newObject, _defaultlayer);

        if(_scaleOption == ScaleOption.Slider)
        {
            newObject.transform.localScale = new Vector3(_width, _height, _width);
        }
        if(_scaleOption == ScaleOption.RandomWeighted)
        {
            newObject.transform.localScale = _treePreset.Scale[GreenthumbUtils.WeightedRandom(_treePreset.weights)];
        }
    }

    private void RemovePrefab(RaycastHit hit)
    {
        int layerMask = 1 << _defaultlayer;
        Collider[] hitColliders = Physics.OverlapSphere(hit.point, _brushSize, layerMask);
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


    [MenuItem("Tools/Green thumb")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GreenthumbEditor>("Green thumb");
    }
}