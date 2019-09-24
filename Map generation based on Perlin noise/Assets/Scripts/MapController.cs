using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Range(2, 512)] public int resolution;
    public float scale;
    public NoiseType type;
    [Range(1, 2)] public int dimension = 2;

    public bool autoUpdate = true;

    public void BuildMap()
    {
        float[,] noiseArea = Noise.GenerateNoiseArea(resolution, scale, type, dimension);

        MapHandler handler = FindObjectOfType<MapHandler>();
        handler.DisplayMap(noiseArea);
    }
}
