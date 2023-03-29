using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GTPalette : ScriptableObject
{
    public List<PaletteData> Palette = new List<PaletteData>();
}