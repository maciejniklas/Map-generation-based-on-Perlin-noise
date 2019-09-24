using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureController
{
    public static Texture2D GenerateFromColors(Color[] mapColors, int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(mapColors);
        texture.Apply();

        return texture;
    }

    public static Texture2D GenerateFromNoise(float[,] noiseArea)
    {
        int resolution = noiseArea.GetLength(0);
        Texture2D texture = new Texture2D(resolution, resolution);

        Color[] mapColors = new Color[resolution * resolution];

        for (int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for (int xIndex = 0; xIndex < resolution; xIndex++)
            {
                mapColors[yIndex * resolution + xIndex] = Color.Lerp(Color.black, Color.white, noiseArea[xIndex, yIndex]);
            }
        }

        return GenerateFromColors(mapColors, resolution);
    }
}
