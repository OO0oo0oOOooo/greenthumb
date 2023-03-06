using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GreenthumbData : ScriptableObject
{
    // public List<PaletteItemData> PaletteItems = new List<PaletteItemData>();
    public BrushSettings BrushSettings;

    public string Layer;
    public string BackupLayer = "Water";

    [HideInInspector] [SerializeReference] private GameObject _objParent;
    public GameObject ObjParent
    {
        get
        {
            if(_objParent == null)
            {
                GameObject obj = new GameObject();
                obj.name = "Greenthumb Group";
                
                return obj;
            }

            return _objParent;
        }
        set { _objParent = value; }
    }
}
