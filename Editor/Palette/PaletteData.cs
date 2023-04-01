using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PPaletteData//<T> where T : GTPalette
{
    public PrefabPalette SelectedPalette;
    public PrefabPalette GetSelectedPalette
    {
        get
        {
            if(PaletteList.Count > 0)
                return PaletteList[SelectedPaletteIndex];

            return null;
        }
    }

    public PrefabPaletteItem SelectedPaletteItem;

    // Palette
    public List<PrefabPalette> PaletteList = new List<PrefabPalette>();
    public List<string> PaletteNameList = new List<string>();
    public int SelectedPaletteIndex = 0;

    public void InitPaletteNameList()
    {
        foreach (var pal in PaletteList)
        {
            PaletteNameList.Add(pal.name);
        }
    }
}

[Serializable]
public class DPaletteData//<T> where T : GTPalette
{
    public DetailPalette SelectedPalette;
    public DetailPalette GetSelectedPalette
    {
        get
        {
            if(PaletteList.Count > 0)
                return PaletteList[SelectedPaletteIndex];

            return null;
        }
    }

    public DetailPaletteItem SelectedPaletteItem;

    // Palette
    public List<DetailPalette> PaletteList = new List<DetailPalette>();
    public List<string> PaletteNameList = new List<string>();
    public int SelectedPaletteIndex = 0;

    public void InitPaletteNameList()
    {
        foreach (var pal in PaletteList)
        {
            PaletteNameList.Add(pal.name);
        }
    }
}
