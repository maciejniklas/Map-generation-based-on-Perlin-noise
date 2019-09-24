using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Range(2, 512)]   public int resolution;
    public float scale;
    public NoiseType type;
    [Range(1, 2)] public int dimension = 2;
    [Range(1, 6)] public int octaves;
    [Range(0, 1)] public float perisstance;
    public float lacunarity;
    public int seed;
    public Vector3 offset;

    public bool autoUpdate = true;

    public void BuildMap()
    {
        float[,] noiseArea = Noise.GenerateNoiseArea(resolution, scale, type, dimension, octaves, perisstance, lacunarity, seed, offset);

        MapHandler handler = FindObjectOfType<MapHandler>();
        handler.DisplayMap(noiseArea);
    }
}
