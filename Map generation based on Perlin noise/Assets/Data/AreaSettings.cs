using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AreaSettings : UpdatableAsset
{
    public const int availableLODS = 5;
    public const int availableResolutionsAmount = 9;
    public const int availableFlatshadedResolutionsAmount = 3;
    public static readonly int[] availableResolutions = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float scale = 2.5f;
    public bool useFlatshading = false;
    [Range(0, availableResolutionsAmount - 1)] public int areaResolutionIndex;
    [Range(0, availableFlatshadedResolutionsAmount - 1)] public int areaFlatshadedResolutionIndex;

    // Includes 2 vertices that we use to calculate normals
    public int verticesPerLine
    {
        get
        {
            return availableResolutions[useFlatshading ? areaFlatshadedResolutionIndex : areaResolutionIndex] + 1;
        }
    }

    public float resolution
    {
        get
        {
            return (verticesPerLine - 3) * scale;
        }
    }
}
