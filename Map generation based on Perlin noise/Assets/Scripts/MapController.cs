using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapController : MonoBehaviour
{
    public enum DisplayMode { Noise, Mesh, Falloff };

    public NoiseAsset noiseAsset;
    public AreaAsset areaAsset;
    public TextureAsset textureAsset;
    public DisplayMode displayMode;
    public Material areaMaterial;

    [Space(10)]

    [Range(0, 4)] public int previevLOD = 1;

    [Space(10)]

    public bool autoUpdate = true;

    private Queue<MapThread<MapDetails>> mapThreadCollection = new Queue<MapThread<MapDetails>>();
    private Queue<MapThread<MeshDetails>> meshThreadCollection = new Queue<MapThread<MeshDetails>>();
    private float[,] falloffArea;

    public int resolution
    {
        get
        {
            if(areaAsset.useFlatshading)
            {
                return 95;
            }
            else
            {
                return 239;
            }
        }
    }

    private void OnValidate()
    {
        if(areaAsset != null)
        {
            areaAsset.onDataUpdate -= OnDataUpdate;
            areaAsset.onDataUpdate += OnDataUpdate;
        }

        if(noiseAsset != null)
        {
            noiseAsset.onDataUpdate -= OnDataUpdate;
            noiseAsset.onDataUpdate += OnDataUpdate;
        }

        if(textureAsset != null)
        {
            textureAsset.onDataUpdate -= OnTextureUpdate;
            textureAsset.onDataUpdate += OnTextureUpdate;
        }
    }

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
        float[,] noiseArea = Noise.GenerateNoiseArea(resolution + 2, noiseAsset.scale, noiseAsset.type, noiseAsset.dimension, noiseAsset.octaves, noiseAsset.perisstance, noiseAsset.lacunarity, noiseAsset.seed, new Vector3(center.x, center.y) + noiseAsset.offset, noiseAsset.heightMode);

        if(areaAsset.useFalloff)
        {
            if(falloffArea == null)
            {
                falloffArea = FalloffController.GenerateFalloffArea(resolution + 2);
            }

            for (int yIndex = 0; yIndex < resolution + 2; yIndex++)
            {
                for (int xIndex = 0; xIndex < resolution + 2; xIndex++)
                {
                    if (areaAsset.useFalloff)
                    {
                        noiseArea[xIndex, yIndex] = Mathf.Clamp01(noiseArea[xIndex, yIndex] - falloffArea[xIndex, yIndex]);
                    }
                }
            }
        }

        return new MapDetails(noiseArea);
    }

    public void DisplayInEditor()
    {
        MapHandler handler = FindObjectOfType<MapHandler>();
        MapDetails mapDetails = BuildMapDetails(Vector2.zero);

        if (displayMode == DisplayMode.Noise)
        {
            handler.DisplayMap(TextureController.GenerateFromNoise(mapDetails.noiseArea));
        }
        else if (displayMode == DisplayMode.Mesh)
        {
            handler.DisplayMesh(MeshController.GenerateMesh(mapDetails.noiseArea, areaAsset.heightMultiplier, areaAsset.curve, previevLOD, areaAsset.useFlatshading));
        }
        else if(displayMode == DisplayMode.Falloff)
        {
            handler.DisplayMap(TextureController.GenerateFromNoise(falloffArea));
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
        MeshDetails meshDetails = MeshController.GenerateMesh(mapDetails.noiseArea, areaAsset.heightMultiplier, areaAsset.curve, lod, areaAsset.useFlatshading);

        lock(meshThreadCollection)
        {
            meshThreadCollection.Enqueue(new MapThread<MeshDetails>(callback, meshDetails));
        }
    }

    private void OnDataUpdate()
    {
        if(!Application.isPlaying)
        {
            DisplayInEditor();
        }
    }

    private void OnTextureUpdate()
    {
        textureAsset.AttachToMaterial(areaMaterial);
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

    public MapDetails(float[,] noiseArea)
    {
        this.noiseArea = noiseArea;
    }
};
