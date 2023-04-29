using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GreenthumbData : ScriptableObject
{
    // public List<PrefabPalette> PaletteList = new List<PrefabPalette>();

    public PPaletteData PaletteDataPrefab;
    public DPaletteData PaletteDataDetails;

    public BrushSettings BrushSettings;

    [HideInInspector] public string Layer;
    [HideInInspector] public string BackupLayer = "Water";
}