using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    [Range(2, 512)] public int resolution = 256;

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

    public void FillTexture()
    {
        if(texture.width != resolution)
        {
            texture.Resize(resolution, resolution);
        }

        float stepSize = 1f / resolution;

        for(int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex++)
            {
                // To compute the colors at the center of the pixels I have to add a half one to achieve this
                texture.SetPixel(xIndex, yIndex, new Color((xIndex + .5f) * stepSize, (yIndex + .5f) * stepSize, 0f));
            }
        }

        texture.Apply();
    }
}
