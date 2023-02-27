using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// TODO:
// Palette
// Undo
// Serialization
// // Parent, Palette, 

// All of grass stuff

public class GreenThumbEditor : EditorWindow
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
    [Range(0, 1)] // -1 any normal, 0 only halfway, 1 only perpendicular
    private float _brushNormalLimit = 1;

    [Range(0, 1)] // 0 Hit Normal has no effect, 1 Fully faces normal
    private float _brushNormalWeight = 0;


    // Object Settings
    private float _width = 0.75f;
    private float _height = 1;

    private string _defaultParent = "GreenThumb Group";

    private GameObject _objectGroup;
    private ObjectData _treePreset;

    // layer
    private LayerMask _layerMask;
    private LayerMask _backupLayer = 4;
    private string _defaultLayer = "GreenThumb";


    // Input Event Parameters
    private bool _isMouseDown = false;
    private bool _isButtonHeld = false;
    
    private float _mouseDownTime;


    [MenuItem("Tools/Green Thumb")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GreenThumbEditor>("Green Thumb");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += this.OnSceneGUI;

        Texture _treeIcon = (Texture)Resources.Load("Icons/pine-tree", typeof(Texture));
        Texture _grassIcon = (Texture)Resources.Load("Icons/grass", typeof(Texture));
        Texture _cogIcon = (Texture)Resources.Load("Icons/cog", typeof(Texture));

        _tabs = new GUIContent[] {new GUIContent(_treeIcon), new GUIContent(_grassIcon), new GUIContent(_cogIcon)};

        _layerMask = GreenThumbUtils.CreateLayer(_defaultLayer, _backupLayer);

        if(_objectGroup == null)
        {
            _objectGroup = new GameObject(_defaultParent); // _treePreset.ParentName
        }
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
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
        _treePreset = EditorGUILayout.ObjectField("", _treePreset, typeof(ObjectData), true) as ObjectData;

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
            _objectGroup = EditorGUILayout.ObjectField("Object Group", _objectGroup, typeof(GameObject), true) as GameObject;
        }
        
        
        GUILayout.Space(20);
        
        // Should be ok to enable if data serialized. right now i think it would recreate the "Green Thumb" Layer every OnEnable() if it was removed
        GUILayout.Label("Primary Layer");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LayerField(_layerMask);
        EditorGUI.EndDisabledGroup();

        GUILayout.Label("Backup Layer");
        _backupLayer = EditorGUILayout.LayerField(_backupLayer);

        GUILayout.Label("Settings");
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
        // Debug.Log("Frame");
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
        Brush(hit, _brushSize, _brushDensity, 1, _layerMask);
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

        GameObject newObject = Instantiate(_treePreset.Obj, hit.point, rot, _objectGroup.transform);

        GreenThumbUtils.SetLayerRecursively(newObject, _layerMask);

        if(_scaleOption == ScaleOption.Slider)
        {
            newObject.transform.localScale = new Vector3(_width, _height, _width);
        }
        if(_scaleOption == ScaleOption.RandomWeighted)
        {
            newObject.transform.localScale = _treePreset.Scale[GreenThumbUtils.WeightedRandom(_treePreset.weights)];
        }
    }

    private void RemovePrefab(RaycastHit hit)
    {
        int layerMask = 1 << _layerMask;
        Collider[] hitColliders = Physics.OverlapSphere(hit.point, _brushSize, layerMask);
        foreach (var hitCollider in hitColliders)
        {
            // Double check incase of stray Objects in this layer
            if(hitCollider.transform.root.gameObject != _objectGroup)
                continue;

            GameObject obj = GreenThumbUtils.FindDesiredRoot(hitCollider.gameObject, _objectGroup);

            // Double protection in case i remove one of these while working on stuff
            if(hitCollider.transform.root.gameObject == _objectGroup)
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
}