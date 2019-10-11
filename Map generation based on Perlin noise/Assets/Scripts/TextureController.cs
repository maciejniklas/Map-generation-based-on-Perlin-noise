using UnityEngine;

public static class TextureController
{
    public static Texture2D GenerateFromColors(Color[] colors, int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }

    public static Texture2D GenerateFromNoise(AreaNoise noiseArea)
    {
        int resolution = noiseArea.area.GetLength(0);

        Color[] colors = new Color[resolution * resolution];

        for (int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for (int xIndex = 0; xIndex < resolution; xIndex++)
            {
                colors[yIndex * resolution + xIndex] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(noiseArea.minHeight, noiseArea.maxHeight, noiseArea.area[xIndex, yIndex]));
            }
        }

        return GenerateFromColors(colors, resolution);
    }
}
