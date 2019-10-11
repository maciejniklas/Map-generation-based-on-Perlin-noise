using UnityEngine;

public static class FalloffController
{
    public static float[,] GenerateFalloffArea(int size)
    {
        float[,] falloffArea = new float[size, size];

        for(int yIndex = 0; yIndex < size; yIndex++)
        {
            for(int xIndex = 0; xIndex < size; xIndex++)
            {
                Vector2 point = new Vector2(xIndex, yIndex);
                point /= size;
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
