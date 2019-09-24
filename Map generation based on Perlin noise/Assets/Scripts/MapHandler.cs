using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public Renderer renderer;

    public void DisplayMap(float[,] noiseArea)
    {
        int resolution = noiseArea.GetLength(0);
        Texture2D texture = new Texture2D(resolution, resolution);

        Color[] mapColors = new Color[resolution * resolution];

        for(int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex++)
            {
                mapColors[yIndex * resolution + xIndex] = Color.Lerp(Color.black, Color.white, noiseArea[xIndex, yIndex]);
            }
        }

        texture.SetPixels(mapColors);
        texture.Apply();

        // To avoid clicking Play button all the time I could apply the texture to the shared material to see effect in the editor
        renderer.sharedMaterial.mainTexture = texture;
        renderer.transform.localScale = new Vector3(resolution, 1, resolution);
    }
}
