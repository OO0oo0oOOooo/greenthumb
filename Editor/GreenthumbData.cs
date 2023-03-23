using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GreenthumbData : ScriptableObject
{
    public List<GTPalette> PaletteList = new List<GTPalette>();
    public BrushSettings BrushSettings;

    [HideInInspector] public string Layer;
    [HideInInspector] public string BackupLayer = "Water";

    // private string _objName = "Greenthumb Group";
    // public GameObject _obj;
    // public GameObject Obj
    // {
    //     get
    //     {
    //         if(_obj == null && !GameObject.Find(_objName))
    //         {
    //             GameObject newObj = new GameObject();
    //             newObj.name = _objName;

    //             newObj.AddComponent(typeof(GTGrassRenderer));
    //             newObj.AddComponent<Grid>().cellSize = new Vector3(25, 25, 25);
                
    //             return newObj;
    //         }

    //         return _obj;
    //     }
    //     set { _obj = value; }
    // }
}