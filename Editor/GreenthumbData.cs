using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GreenthumbData : ScriptableObject
{
    public List<GTPalette> PaletteList = new List<GTPalette>();
    public BrushSettings BrushSettings;

    [HideInInspector] public string Layer;
    [HideInInspector] public string BackupLayer = "Water";
}