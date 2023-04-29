using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PrefabPalette : ScriptableObject
{
    public List<PrefabPaletteItem> Palette = new List<PrefabPaletteItem>();
}