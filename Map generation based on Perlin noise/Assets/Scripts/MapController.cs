using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapController : MonoBehaviour
{
    public enum DisplayMode { Noise, Color, Mesh };

    public const int resolution = 241;

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

    public float heightMultiplier;
    public AnimationCurve curve;
    [Range(0, 6)] public int previevLOD = 1;

    [Space(10)]

    public bool autoUpdate = true;

    [Space(10)]

    public Region[] regions;

    private Queue<MapThread<MapDetails>> mapThreadCollection = new Queue<MapThread<MapDetails>>();
    private Queue<MapThread<MeshDetails>> meshThreadCollection = new Queue<MapThread<MeshDetails>>();

    private void Update()
    {
        if(mapThreadCollection.Count > 0)
        {
            for(int index = 0; index < mapThreadCollection.Count; index++)
            {
                MapThread<MapDetails> threadDetails = mapThreadCollection.Dequeue();
                threadDetails.callback(threadDetails.variable);
            }
        }

        if(meshThreadCollection.Count > 0)
        {
            for(int index = 0; index < meshThreadCollection.Count; index++)
            {
                MapThread<MeshDetails> threadDetails = meshThreadCollection.Dequeue();
                threadDetails.callback(threadDetails.variable);
            }
        }
    }

    private MapDetails BuildMapDetails(Vector2 center)
    {
        float[,] noiseArea = Noise.GenerateNoiseArea(resolution, scale, type, dimension, octaves, perisstance, lacunarity, seed, new Vector3(center.x, center.y) + offset);
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

        return new MapDetails(noiseArea, mapColors);
    }

    public void DisplayInEditor()
    {
        MapHandler handler = FindObjectOfType<MapHandler>();
        MapDetails mapDetails = BuildMapDetails(Vector2.zero);

        if (displayMode == DisplayMode.Noise)
        {
            handler.DisplayMap(TextureController.GenerateFromNoise(mapDetails.noiseArea));
        }
        else if (displayMode == DisplayMode.Color)
        {
            handler.DisplayMap(TextureController.GenerateFromColors(mapDetails.mapColors, resolution));
        }
        else if (displayMode == DisplayMode.Mesh)
        {
            handler.DisplayMesh(MeshController.GenerateMesh(mapDetails.noiseArea, heightMultiplier, curve, previevLOD), TextureController.GenerateFromColors(mapDetails.mapColors, resolution));
        }
    }

    private void MapDetailsThread(Action<MapDetails> callback, Vector2 center)
    {
        MapDetails mapDetails = BuildMapDetails(center);

        lock(mapThreadCollection)
        {
            mapThreadCollection.Enqueue(new MapThread<MapDetails>(callback, mapDetails));
        }
    }

    private void MeshDetailsThread(Action<MeshDetails> callback, MapDetails mapDetails, int lod)
    {
        MeshDetails meshDetails = MeshController.GenerateMesh(mapDetails.noiseArea, heightMultiplier, curve, lod);

        lock(meshThreadCollection)
        {
            meshThreadCollection.Enqueue(new MapThread<MeshDetails>(callback, meshDetails));
        }
    }

    public void RequestMapDetails(Action<MapDetails> callback, Vector2 center)
    {
        ThreadStart threadStart = delegate
        {
            MapDetailsThread(callback, center);
        };

        new Thread(threadStart).Start();
    }

    public void RequestMeshDetails(Action<MeshDetails> callback, MapDetails mapDetails, int lod)
    {
        ThreadStart threadStart = delegate
        {
            MeshDetailsThread(callback, mapDetails, lod);
        };

        new Thread(threadStart).Start();
    }

    private struct MapThread<T>
    {
        public readonly Action<T> callback;
        public readonly T variable;

        public MapThread(Action<T> callback, T variable)
        {
            this.callback = callback;
            this.variable = variable;
        }
    };
}

public struct MapDetails
{
    public readonly float[,] noiseArea;
    public readonly Color[] mapColors;

    public MapDetails(float[,] noiseArea, Color[] mapColors)
    {
        this.noiseArea = noiseArea;
        this.mapColors = mapColors;
    }
};

[System.Serializable]
public struct Region
{
    public float height;
    public Color color;
    public string label;
};
