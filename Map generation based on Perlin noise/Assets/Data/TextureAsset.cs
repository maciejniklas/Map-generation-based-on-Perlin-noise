using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureAsset : UpdatableAsset
{
    public Color[] regionColors;
    public float[] regionHeights;

    private float minHeight;
    private float maxHeight;
    public void AttachToMaterial(Material material)
    {
        material.SetInt("colorsAmount", regionColors.Length);
        material.SetColorArray("regionColors", regionColors);
        material.SetFloatArray("regionHeights", regionHeights);

        RefreshHeights(material, minHeight, maxHeight);
    }

    public void RefreshHeights(Material material, float minHeight, float maxHeight)
    {
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;

        material.SetFloat("minimalHeight", minHeight);
        material.SetFloat("maximumHeight", maxHeight);
    }
}
