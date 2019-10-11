using UnityEngine;

[CreateAssetMenu()]
public class AreaDetails : UpdatableAsset
{
    public const int availableLODs = 5;
    public const int availableResolutionsAmount = 9;
    public const int availableFlatshadedResolutionsAmount = 3;
    public static readonly int[] availableResolutions = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float scale = 2.5f;
    public bool useFlatshading = false;
    [Range(0, availableResolutionsAmount - 1)] public int areaResolutionIndex;
    [Range(0, availableFlatshadedResolutionsAmount - 1)] public int areaFlatshadedResolutionIndex;

    // +1 because of the0 vertex, +4 because of generating triangles in the boundaries of map area to avoid mismatch of areas with different LODs
    public int verticesPerLine
    {
        get
        {
            return availableResolutions[useFlatshading ? areaFlatshadedResolutionIndex : areaResolutionIndex] + 5;
        }
    }

    // Includes 2 vertices that we use to calculate normals
    public float resolution
    {
        get
        {
            return (verticesPerLine - 3) * scale;
        }
    }
}
