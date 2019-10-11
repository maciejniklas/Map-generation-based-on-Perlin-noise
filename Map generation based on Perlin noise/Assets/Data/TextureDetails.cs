using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TextureDetails : UpdatableAsset
{
    private const int textureResolution = 512;
    private const TextureFormat textureFormat = TextureFormat.RGB565;
    public Region[] regions;

    private float minHeight;
    private float maxHeight;

    public void AttachToMaterial(Material material)
    {
        material.SetInt("regionsAmount", regions.Length);
        material.SetColorArray("colors", regions.Select(item => item.color).ToArray());
        material.SetFloatArray("heights", regions.Select(item => item.height).ToArray());
        material.SetFloatArray("mixture", regions.Select(item => item.mixture).ToArray());
        material.SetFloatArray("impacts", regions.Select(item => item.impact).ToArray());
        material.SetFloatArray("textureScales", regions.Select(item => item.textureScale).ToArray());

        Texture2DArray texture2DArray = ConvertToTextureArray(regions.Select(item => item.texture).ToArray());

        material.SetTexture("regionsTexture", texture2DArray);

        RefreshHeights(material, minHeight, maxHeight);
    }

    private Texture2DArray ConvertToTextureArray(Texture2D[] textures)
    {
        Texture2DArray texture2DArray = new Texture2DArray(textureResolution, textureResolution, textures.Length, textureFormat, true);

        for(int index = 0; index < textures.Length; index++)
        {
            texture2DArray.SetPixels(textures[index].GetPixels(), index);
        }

        texture2DArray.Apply();

        return texture2DArray;
    }

    public void RefreshHeights(Material material, float minHeight, float maxHeight)
    {
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;

        material.SetFloat("minimalHeight", minHeight);
        material.SetFloat("maximumHeight", maxHeight);
    }

    [System.Serializable]
    public class Region
    {
        public Texture2D texture;
        public Color color;
        [Range(0, 1)] public float impact;
        [Range(0, 1)] public float height;
        [Range(0, 1)] public float mixture;
        public float textureScale;
    }
}
