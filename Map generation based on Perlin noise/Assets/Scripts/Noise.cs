using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate float NoiseGenerator(Vector3 point);

public enum NoiseType { Value, Perlin }

public static class Noise
{
    public enum HeightNormalizeMode { Local, Global}

    public static NoiseGenerator[] perlinNoise = { Perlin1D, Perlin2D };
    public static NoiseGenerator[] valueNoise = { Value1D, Value2D };
    public static NoiseGenerator[][] noiseType = { valueNoise, perlinNoise };

    private static float[] gradients1D = { 1f, -1f };
    private static int gradients1DMask = gradients1D.Length - 1;

    private static Vector2[] gradients2D = {
        new Vector2( 1f, 0f),
        new Vector2(-1f, 0f),
        new Vector2( 0f, 1f),
        new Vector2( 0f,-1f),
        new Vector2( 1f, 1f).normalized,
        new Vector2(-1f, 1f).normalized,
        new Vector2( 1f,-1f).normalized,
        new Vector2(-1f,-1f).normalized
    };
    private static int gradients2DMask = gradients2D.Length - 1;

    private static int[] permutations = {
        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
         57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
         74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
         60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
         65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
         52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
         81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
    };
    private static int permutationsMask = permutations.Length - 1;

    private static float DotProduct(Vector2 gradient, float x, float y)
    {
        return gradient.x * x + gradient.y * y;
    }

    public static float[,] GenerateNoiseArea(int resolution, float scale, NoiseType type, int dimension, int octaves, float persistance, float lacunarity, int seed, Vector3 offset, HeightNormalizeMode heightMode)
    {
        float[,] noiseArea = new float[resolution, resolution];
        NoiseGenerator generator = noiseType[(int)type][dimension - 1];
        System.Random random = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[octaves];

        offset.y *= -1;

        float globalMaxHeight = 0;
        float amplitude = 1f;
        float frequency = 1f;

        for (int octaveIndex = 0; octaveIndex < octaves; octaveIndex++)
        {
            octaveOffsets[octaveIndex] = new Vector3(random.Next(-1000000, 100000), random.Next(-1000000, 100000)) + offset;

            globalMaxHeight += amplitude;
            amplitude *= persistance;
        }

        scale = scale <= 0 ? 0.001f : scale;

        float localMinHeight = float.MaxValue;
        float localMaxHeight = float.MinValue;

        for(int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex++)
            {
                amplitude = 1f;
                frequency = 1f;
                float height = 0f;

                for(int octaveIndex = 0; octaveIndex < octaves; octaveIndex++)
                {
                    Vector3 point = new Vector3(xIndex, yIndex);
                    point.x -= resolution / 2;
                    point.y -= resolution / 2;
                    point += octaveOffsets[octaveIndex];
                    point /= scale;
                    point *= frequency;

                    float value = generator(point);

                    height += value * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(height > localMaxHeight)
                {
                    localMaxHeight = height;
                }
                else if (height < localMinHeight)
                {
                    localMinHeight = height;
                }

                noiseArea[xIndex, yIndex] = height;
            }
        }

        for (int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for (int xIndex = 0; xIndex < resolution; xIndex++)
            {
                if(heightMode == HeightNormalizeMode.Local)
                {
                    noiseArea[xIndex, yIndex] = Mathf.InverseLerp(localMinHeight, localMaxHeight, noiseArea[xIndex, yIndex]);
                }
                else if(heightMode == HeightNormalizeMode.Global)
                {
                    float unificatedHeight = (noiseArea[xIndex, yIndex] + 1) / globalMaxHeight;
                    noiseArea[xIndex, yIndex] = Mathf.Clamp(unificatedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseArea;
    }

    public static float Perlin1D(Vector3 point)
    {
        int leftIndex = Mathf.FloorToInt(point.x);

        float leftFractional = point.x - leftIndex;
        float rightFractional = leftFractional - 1;

        leftIndex &= permutationsMask;

        int rightIndex = leftIndex + 1;
        rightIndex &= permutationsMask;

        float leftGradient = gradients1D[permutations[leftIndex] & gradients1DMask];
        float rightGradient = gradients1D[permutations[rightIndex] & gradients1DMask];

        float leftGradientValue = leftGradient * leftFractional;
        float rightGradientValue = rightGradient * rightFractional;

        float fractional = Smooth(leftFractional);

        // If we want interpolate two gradients in opposite directions the maximum value is 0.5, so I have to multiply the result by 2
        float value = Mathf.Lerp(leftGradientValue, rightGradientValue, fractional) * 2f; ;

        return value;
    }

    public static float Perlin2D(Vector3 point)
    {
        int leftIndexX = Mathf.FloorToInt(point.x);
        int leftIndexY = Mathf.FloorToInt(point.y);

        float leftFractionalX = point.x - leftIndexX;
        float leftFractionalY = point.y - leftIndexY;

        float rightFractionalX = leftFractionalX - 1;
        float rightFractionalY = leftFractionalY - 1;

        leftIndexX &= permutationsMask;
        leftIndexY &= permutationsMask;

        int rightIndexX = leftIndexX + 1;
        int rightIndexY = leftIndexY + 1;

        rightIndexX &= permutationsMask;
        rightIndexY &= permutationsMask;

        int leftHash = permutations[leftIndexX];
        int rightHash = permutations[rightIndexX];
        
        Vector2 bottomLeftGradient = gradients2D[permutations[(leftHash + leftIndexY) & permutationsMask] & gradients2DMask];
        Vector2 bottomRightGradient = gradients2D[permutations[(rightHash + leftIndexY) & permutationsMask] & gradients2DMask];
        Vector2 topLeftGradient = gradients2D[permutations[(leftHash + rightIndexY) & permutationsMask] & gradients2DMask];
        Vector2 topRightGradient = gradients2D[permutations[(rightHash + rightIndexY) & permutationsMask] & gradients2DMask];
        
        float bottomLeftGradientValue = DotProduct(bottomLeftGradient, leftFractionalX, leftFractionalY);
        float bottomRightGradientValue = DotProduct(bottomRightGradient, rightFractionalX, leftFractionalY);
        float topLeftGradientValue = DotProduct(topLeftGradient, leftFractionalX, rightFractionalY);
        float topRightGradientValue = DotProduct(topRightGradient, rightFractionalX, rightFractionalY);

        float fractionalX = Smooth(leftFractionalX);
        float fractionalY = Smooth(leftFractionalY);
        
        // In this case maximum value is no 1 but sqrt(0.5) because of four diagonal gradients pointing at center of the cell
        float value = Mathf.Lerp(
            Mathf.Lerp(bottomLeftGradientValue, bottomRightGradientValue, fractionalX),
            Mathf.Lerp(topLeftGradientValue, topRightGradientValue, fractionalX),
            fractionalY
            ) * Mathf.Sqrt(2);

        return value;
    }

    private static float Smooth(float value)
    {
        return value * value * value * (value * (value * 6f - 15f) + 10f);
    }

    public static float Value1D(Vector3 point)
    {
        // To interpolate noise values I have to compute lattice coordinates to the left and right of our sample point
        int leftIndex = Mathf.FloorToInt(point.x);
        float fractional = point.x - leftIndex;
        leftIndex &= permutationsMask;

        int rightIndex = leftIndex + 1;
        rightIndex &= permutationsMask;

        int leftHash = permutations[leftIndex];
        int rightHash = permutations[rightIndex];

        fractional = Smooth(fractional);

        float value = Mathf.Lerp(leftHash, rightHash, fractional) / (float)permutationsMask;

        return value;
    }

    public static float Value2D(Vector3 point)
    {
        int leftIndexX = Mathf.FloorToInt(point.x);
        int leftIndexY = Mathf.FloorToInt(point.y);

        float fractionalX = point.x - leftIndexX;
        float fractionalY = point.y - leftIndexY;

        leftIndexX &= permutationsMask;
        leftIndexY &= permutationsMask;

        int rightIndexX = leftIndexX + 1;
        int rightIndexY = leftIndexY + 1;

        rightIndexX &= permutationsMask;
        rightIndexY &= permutationsMask;

        int leftHash = permutations[leftIndexX];
        int rightHash = permutations[rightIndexX];

        int bottomLeftHash = permutations[(leftHash + leftIndexY) & permutationsMask];
        int bottomRightHash = permutations[(rightHash + leftIndexY) & permutationsMask];
        int topLeftHash = permutations[(leftHash + rightIndexY) & permutationsMask];
        int topRightHash = permutations[(rightHash + rightIndexY) & permutationsMask];

        fractionalX = Smooth(fractionalX);
        fractionalY = Smooth(fractionalY);

        float value = Mathf.Lerp(
            Mathf.Lerp(bottomLeftHash, bottomRightHash, fractionalX),
            Mathf.Lerp(topLeftHash, topRightHash, fractionalX),
            fractionalY
            ) / (float)permutationsMask;

        return value;
    }
}
