using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public enum DisplayMode { Noise, Color };

    [Range(2, 512)]   public int resolution;
    public float scale;
    public NoiseType type;
    [Range(1, 2)] public int dimension = 2;
    public DisplayMode displayMode;

    [Space(10)]

    [Range(1, 6)] public int octaves;
    [Range(0, 1)] public float perisstance;
    public float lacunarity;
    public int seed;
    public Vector3 offset;

    [Space(10)]

    public bool autoUpdate = true;

    [Space(10)]

    public Region[] regions;

    public void BuildMap()
    {
        float[,] noiseArea = Noise.GenerateNoiseArea(resolution, scale, type, dimension, octaves, perisstance, lacunarity, seed, offset);
        Color[] mapColors = new Color[resolution * resolution];

        for(int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex++)
            {
                float height = noiseArea[xIndex, yIndex];

                for(int regionIndex = 0; regionIndex < regions.Length; regionIndex++)
                {
                    if(height <= regions[regionIndex].height)
                    {
                        mapColors[yIndex * resolution + xIndex] = regions[regionIndex].color;
                        break;
                    }
                }
            }
        }

        MapHandler handler = FindObjectOfType<MapHandler>();

        if(displayMode == DisplayMode.Noise)
        {
            handler.DisplayMap(TextureController.GenerateFromNoise(noiseArea));
        }
        else if(displayMode == DisplayMode.Color)
        {
            handler.DisplayMap(TextureController.GenerateFromColors(mapColors, resolution));
        }
    }
}

[System.Serializable]
public struct Region
{
    public float height;
    public Color color;
    public string label;
};
