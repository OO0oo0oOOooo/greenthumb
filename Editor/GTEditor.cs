using UnityEngine;
using UnityEditor;

//TODO:
// // Move Saving to target script
// // Think about how i should save _data maybe inside target script

// // Undo/Redo

// // Adjust chunk y size

[CustomEditor(typeof(Greenthumb))]
public partial class GTEditor
{
    Greenthumb _target;
    SerializedObject _so;

    SerializedProperty _propMesh;
    SerializedProperty _propMaterial;

    [SerializeField] private GreenthumbData _data;
    [SerializeField] private BrushSettings _brushSettings;
    
    private GUIContent _brushSettingsLabel = new GUIContent("Brush Settings");
    private bool _showBrushSettings = false;


    // Tab Stuff
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

        _so = serializedObject;
        _propMesh = _so.FindProperty("SelectedMesh");
        _propMaterial = _so.FindProperty("SelectedMaterial");

        // Get/Create layer
        _layer = GreenthumbUtils.CreateLayer(_layer, _backupLayer, "Greenthumb");

        // Set up Toolbar GUI
        Texture treeIcon = (Texture)Resources.Load("Icons/pine-tree", typeof(Texture));
        Texture grassIcon = (Texture)Resources.Load("Icons/grass", typeof(Texture));
        Texture cogIcon = (Texture)Resources.Load("Icons/cog", typeof(Texture));
        _tabs = new GUIContent[] { new GUIContent(treeIcon), new GUIContent(grassIcon), new GUIContent(cogIcon) };

        Undo.undoRedoPerformed += UndoCallback;
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoCallback;
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

            // Set these so they dont try to overwrite "" and "Default". 
            _data.Layer = "Greenthumb";
            _data.BackupLayer = "Water";
        }

        _palettePrefab = _data.PaletteDataPrefab;
        _paletteDetail = _data.PaletteDataDetails;

        InitPaletteData();

        _brushSettings = _data.BrushSettings;
        _layer = LayerMask.NameToLayer(_data.Layer);
        _backupLayer = LayerMask.NameToLayer(_data.BackupLayer);
    }

    private void SaveData()
    {
        _data.PaletteDataPrefab = _palettePrefab;
        _data.PaletteDataDetails = _paletteDetail;

        _data.BrushSettings = _brushSettings;
        _data.Layer = LayerMask.LayerToName(_layer);
        _data.BackupLayer = LayerMask.LayerToName(_backupLayer);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void InitPaletteData()
    {
        if(_palettePrefab.PaletteList.Count > 0)
        {
            _palettePrefab.SelectedPalette = _palettePrefab.PaletteList[0];
            _palettePrefab.SelectedPaletteIndex = 0;
            _palettePrefab.SelectedPaletteItem = _palettePrefab.GetSelectedPalette.Palette[0];
        }

        if(_paletteDetail.PaletteList.Count > 0)
        {
            _paletteDetail.SelectedPalette = _paletteDetail.PaletteList[0];
            _paletteDetail.SelectedPaletteIndex = 0;
            _paletteDetail.SelectedPaletteItem = _paletteDetail.GetSelectedPalette.Palette[0];
        }
    }

    public override void OnInspectorGUI()
    {
        _target = target as Greenthumb;
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
        PrefabSelectedGUI();
        PrefabPaletteSelectionGUI();
        PrefabPaletteDisplayGUI();
    }

    private void GrassGUI()
    {
        BrushGUI();
        DetailSelectedGUI();
        DetailPaletteSelectionGUI();
        DetailPaletteDisplayGUI();
    }

    private void SettingsGUI()
    {
        GUILayout.Label("Primary Layer");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LayerField(_layer);
        EditorGUI.EndDisabledGroup();

        GUILayout.Label("Backup Layer");
        _backupLayer = EditorGUILayout.LayerField(_backupLayer);
    }

    private void BrushGUI()
    {
        if(_showBrushSettings = EditorGUILayout.Foldout(_showBrushSettings, "Brush Settings"))
        {
            _brushSettings.BrushSize = EditorGUILayout.Slider("Brush Size", _brushSettings.BrushSize, 0f, 100f);
            _brushSettings.BrushDensity = EditorGUILayout.Slider("Brush Density", _brushSettings.BrushDensity, 0f, 10f);
            GUILayout.Space(10);
            _brushSettings.BrushNormalLimit = EditorGUILayout.Slider("Brush Normal Limit", _brushSettings.BrushNormalLimit, 0f, 1f);
            _brushSettings.BrushNormalWeight = EditorGUILayout.Slider("Brush Normal Weight", _brushSettings.BrushNormalWeight, 0f, 1f);
        }
    }

    private void OnSceneGUI ()
    {
        _target = target as Greenthumb;
        // _target = target as GTGrassRenderer;

        if(_tabSelected == (int)Tabs.Settings || _tabSelected == -1)
            return;

        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(worldRay, out RaycastHit hit, 10000))
        {
            if (hit.collider.gameObject != null)
            {
                DrawBrush(hit);
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
                Place(hit);
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
                Erase(hit);
            }
            else
            {
                Paint(hit);
            }
        }
    }

    private void Place(RaycastHit hit)
    {
        switch (_tabSelected)
        {
            case (int)Tabs.Tree:
                InstatiatePrefab(hit,  _target.gameObject);
                break;

            case (int)Tabs.Grass:
                PlaceGrass(hit);
                break;
        }
    }

    private void Paint(RaycastHit hit)
    {
        switch (_tabSelected)
        {
            case (int)Tabs.Tree:
                BrushPrefab(hit, 1, _layer);
                break;

            case (int)Tabs.Grass:
                BrushGrass(hit, 1, _layer);
                break;
        }
    }

    private void Erase(RaycastHit hit)
    {
        switch (_tabSelected)
        {
            case (int)Tabs.Tree:
                RemovePrefab(hit, _brushSettings.BrushSize);
                break;

            case (int)Tabs.Grass:
                RemoveGrass(hit);
                break;
        }
    }

    void DrawBrush(RaycastHit hit)
    {
        float dot = Quaternion.Dot(Quaternion.identity.WeightedNormal(hit.normal), Quaternion.identity);
        Handles.color = dot <= _brushSettings.BrushNormalLimit ? Color.red : Color.green;

        Handles.DrawLine(hit.point, hit.point + hit.normal);
        Handles.DrawWireDisc(hit.point, hit.normal, _brushSettings.BrushSize);
    }

    private void UndoCallback()
    {
        Debug.Log("Undo");
    }

    [MenuItem("GameObject/3D Object/Green Thumb")]
    static void CreateCustomObject()
    {
        // Create a new GameObject
        GameObject customObject = new GameObject("Green Thumb");
        customObject.AddComponent<Grid>().cellSize = new Vector3(25, 25, 25);
        customObject.AddComponent<Greenthumb>();
        // customObject.AddComponent<GTGrassRenderer>();

        // Set its position to be at the focus point
        // if (SceneView.lastActiveSceneView != null)
            // customObject.transform.position = SceneView.lastActiveSceneView.pivot;

        // Register undo operation
        Undo.RegisterCreatedObjectUndo(customObject, "Create " + customObject.name);

        // Select newly created object
        Selection.activeObject = customObject;
    }
}
