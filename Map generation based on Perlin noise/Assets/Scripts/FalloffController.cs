using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffController
{
    public static float[,] GenerateFalloffArea(int resolution)
    {
        float[,] falloffArea = new float[resolution, resolution];

        for(int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex++)
            {
                Vector2 point = new Vector2(xIndex, yIndex);
                point /= (float)resolution;
                point *= 2;
                point -= Vector2.one;

                float value = Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y));
                falloffArea[xIndex, yIndex] = Convert(value);
            }
        }

        return falloffArea;
    }

    private static float Convert(float value)
    {
        float paramA = 3;
        float paramB = 2.2f;

        return Mathf.Pow(value, paramA) / (Mathf.Pow(value, paramA) + Mathf.Pow((paramB - paramB * value), paramA));
    }
}
