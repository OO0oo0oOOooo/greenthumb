using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// TODO:
// Pallette
// Layer stuff in OnEnable, RemoveTree, InstantiatePrefab
// Undo
// Normal Limit
// Normal Weight

// All of grass stuff

public class GreenThumbEditor : EditorWindow
{
    private GUIContent[] _tabs;
    private int _tabSelected = -1;

    private String[] _options = { "Add", "Remove" };
    private int _optionSelected = 0;

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
    [Range(-1, 1)] // -1 any normal, 0 only halfway, 1 only perpendicular 
    private float _brushNormalLimit = 1;

    [Range(0, 1)] // 0 Hit Normal has no effect, 1 Fully faces normal
    private float _brushNormalWeight = 0;


    // Object Settings
    private float _width = 0.75f;
    private float _height = 1;

    private GameObject _treeGroup;
    private ObjectData _treePreset;

    private LayerMask _layerMask = 4;


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
        // Texture _rockIcon = (Texture)Resources.Load("Icons/stone", typeof(Texture));

        _tabs = new GUIContent[] {new GUIContent(_treeIcon), new GUIContent(_grassIcon), new GUIContent(_cogIcon)};
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

    private void BrushOptions()
    {
        using ( new GUILayout.VerticalScope() ) 
        {
            _optionSelected = GUILayout.Toolbar(_optionSelected, _options);
        }

        GUILayout.Space(20);
    }

    private void AddGUI()
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
        // else if(_scaleOption == ScaleOption.RandomWeighted)
        // {
        //     EditorGUILayout.PropertyField(_so.FindProperty("_scaleField"));
        //     _so.ApplyModifiedProperties();
        // }
    }

    private void RemoveGUI()
    {
        GUILayout.Label("Brush Settings");
        using ( new GUILayout.HorizontalScope() ) 
        {
            GUILayout.Label("Brush Size");
            _brushSize = EditorGUILayout.Slider(_brushSize, 0, 100);
        }
    }

    private void TreeGUI()
    {
        BrushOptions();

        _treePreset = EditorGUILayout.ObjectField("", _treePreset, typeof(ObjectData), true) as ObjectData;
        GUILayout.Space(20);

        switch(_optionSelected)
        {
            case 0:
                AddGUI();
                break;

            case 1:
                RemoveGUI();
                break;
        }

        GUILayout.Label("TREE");
    }

    private void GrassGUI()
    {
        BrushOptions();
        GUILayout.Label("Grass");
    }

    private void SettingsGUI()
    {
        GUILayout.Space(20);

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

        HandleInput(_tabSelected, 0); // TODO: Prefab Pallette
    }

    private void HandleInput(int tab, int palletteIndex)
    {
        Event e = Event.current;
        if (Event.current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            Debug.Log("Mouse Up!");
            e.Use();
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && e.control)
        {
            Debug.Log("Ctrl Mouse Drag!");
            // Erase Brush
            e.Use();
        }
        else if (e.type == EventType.MouseDown && e.button == 0 && e.control)
        {
            Debug.Log("Ctrl Mouse Down!");
            // Erase Selected
            e.Use();
        }

        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            Debug.Log("Mouse Drag!");
            // Paint Brush
            e.Use();
        }
        else if (e.type == EventType.MouseDown && e.button == 0)
        {
            Debug.Log("Mouse Down!");
            // Place at location
            e.Use();
        }
    }

    private void Place(int tab, int palletteIndex)
    {
        // Single Place
    }

    private void Paint(int tab, int palletteIndex)
    {
        // Brush
    }

    private void Remove(int tab, int palletteIndex)
    {
        // Single remove
    }

    private void Erase(int tab, int palletteIndex)
    {
        // Brush
    }

    void OnClick(RaycastHit hit)
    {
        if(_optionSelected == 0)
        {
            switch(_tabSelected)
            {
                case (int)Tabs.Tree:
                    InstatiatePrefab(hit);
                    break;

                case (int)Tabs.Grass:
                    PlaceGrass(hit);
                    break;
            }
        }
        else if(_optionSelected == 1)
        {
            switch(_tabSelected)
            {
                case (int)Tabs.Tree:
                    RemoveTree(hit);
                    break;

                case (int)Tabs.Grass:
                    RemoveGrass(hit);
                    break;
            }
        }

        // Check normal threshold with the dot product

        // Instantiate Object at hit point, hit normal, random rotation and scale controlled by noise or random
        // If Tree add leaves to GPUInstancing
        // If Grass add to GPUInstanced Grass 
        // If Erase find and remove objects of that type
    }

    void OnDragClick(RaycastHit hit)
    {
        InstantiateBrush(hit, _brushSize, _brushDensity, 1, _layerMask);
    }

    public void InstantiateBrush(RaycastHit hit, float brushSize, float density, float maxDistance, int layerID)
    {
        // Calculate the minimum distance between each object based on the desired density
        float minDistance = Mathf.Sqrt(1f / (density * Mathf.PI)) * brushSize;

        // Instantiate objects within the brush size until we reach the desired density
        int maxObjects = Mathf.FloorToInt(Mathf.PI * brushSize * brushSize * 0.25f * density);

        // Calculate the minimum distance between each object based on the desired density
        // float sphereVolume = (4f / 3f) * Mathf.PI * Mathf.Pow(brushSize, 3f);
        // int maxObjects = Mathf.FloorToInt(sphereVolume * density);
        // float minDistance = Mathf.Pow((3f / (4f * Mathf.PI * maxObjects)), 1f / 3f);

        // Choose a random position within the brush size
        Vector3 randomPosition = hit.point + UnityEngine.Random.insideUnitSphere * brushSize;

        int layerMask = 1 << layerID;
        // Find all colliders within the brush size
        Collider[] hitColliders = Physics.OverlapSphere(randomPosition, brushSize, layerMask);

        // Debug.Log(hitColliders.Length);
        //Return if it is too dense
        if(hitColliders.Length >= maxObjects)
        {
            Debug.Log("MAX OBJ");
            return;
        }

        // if(hitColliders.Length > (int)density)
        // {
        //     return;
        // }

        // Get exact postion of new object before checking distance
        if (Physics.Raycast(randomPosition + hit.normal, -hit.normal, out RaycastHit hitInfo, maxDistance))
        {
            // Check if the new object overlaps with any existing objects
            foreach (Collider hitCollider in hitColliders)
            {
                if (Vector3.Distance(hitInfo.point, hitCollider.transform.position) < minDistance)
                {
                    Debug.Log("MIN DIST");
                    return;
                }
            }
            
            // Instatiate
            InstatiatePrefab(hitInfo);
        }
        
    }

    private void InstatiatePrefab(RaycastHit hit)
    {
        Vector3 xCross = Vector3.Cross(hit.normal, Vector3.forward);
        Vector3 zCross = Vector3.Cross(xCross, hit.normal);
        Quaternion rot = Quaternion.LookRotation(zCross, hit.normal) * Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0);

        if(_treeGroup == null)
        {
            _treeGroup = new GameObject("Tree Group");
        }

        GameObject newObject = Instantiate(_treePreset.Obj, hit.point, rot, _treeGroup.transform);
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

    private void PlaceGrass(RaycastHit hit)
    {
        Debug.Log("Place Grass");
    }

    private void RemoveTree(RaycastHit hit)
    {
        // If removing gets laggy add layermask to Physics.OverlapSphere()
        // use GenerateNewLayer Script

        Collider[] hitColliders = Physics.OverlapSphere(hit.point, _brushSize);
        foreach (var hitCollider in hitColliders)
        {
            //Check parent and raise until parent is _treeGroup
            if(hitCollider.transform.root.gameObject != _treeGroup)
                continue;

            GameObject go = hitCollider.gameObject;
            while(go.transform.parent.gameObject != _treeGroup)
            {
                go = go.transform.parent.gameObject;
            }
            
            // Double protection in case i remove one of these while working on stuff
            if(hitCollider.transform.root.gameObject == _treeGroup)
            {
                Debug.Log(go.name);
                DestroyImmediate(go);
            }
        }
    }

    private void RemoveGrass(RaycastHit hit)
    {
        Debug.Log("Remove Grass");
    }

    void EraseAllOfType()
    {
        // Erase all of that object Type
    }

    void Reset()
    {
        // Erase All objects
    }
}