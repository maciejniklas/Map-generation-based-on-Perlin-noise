﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    [Range(2, 512)] public int resolution = 256;

    public NoiseType type;
    public float frequency = 10f;
    [Range(1, 2)] public int dimension = 2;

    private Texture2D texture;

    private void OnEnable()
    {
        if(texture == null)
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
            texture.name = "Procedural texture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 9;

            GetComponent<MeshRenderer>().material.mainTexture = texture;
        }

        FillTexture();
    }

    private void Update()
    {
        if(transform.hasChanged)
        {
            transform.hasChanged = false;
            FillTexture();
        }
    }

    public void FillTexture()
    {
        if(texture.width != resolution)
        {
            texture.Resize(resolution, resolution);
        }

        Vector3 corner00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 corner10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 corner01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 corner11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));

        NoiseGenerator generator = Noise.noiseType[(int)type][dimension - 1];

        float stepSize = 1f / resolution;

        // To compute the colors at the center of the pixels I have to add a half one to achieve this
        for (int yIndex = 0; yIndex < resolution; yIndex++)
        {
            Vector3 interpolation0 = Vector3.Lerp(corner00, corner01, (yIndex + 0.5f) * stepSize);
            Vector3 interpolation1 = Vector3.Lerp(corner10, corner11, (yIndex + 0.5f) * stepSize);
            for (int xIndex = 0; xIndex < resolution; xIndex++)
            {
                Vector3 point = Vector3.Lerp(interpolation0, interpolation1, (xIndex + 0.5f) * stepSize);

                float value = generator(point, frequency);
                value = type == NoiseType.Perlin ? value * 0.5f + 0.5f : value;

                texture.SetPixel(xIndex, yIndex, Color.white * value);
            }
        }

        texture.Apply();
    }
}
