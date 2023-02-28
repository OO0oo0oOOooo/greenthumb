using UnityEngine;

[CreateAssetMenu]
public class GreenthumbData : ScriptableObject
{
    public PaletteItemData[] PaletteItems;

    public LayerMask DefaultLayer;
    public LayerMask BackupLayer = 4;

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
