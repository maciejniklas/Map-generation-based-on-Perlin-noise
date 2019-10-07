using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseController
{
    public static NoiseArea BuildNoiseArea(int resolution, NoiseAreaSettings settings, Vector3 center)
    {
        float[,] values = Noise.GenerateNoiseArea(resolution, settings.noiseSettings, center);

        AnimationCurve threadCurve = new AnimationCurve(settings.curve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for(int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex++)
            {
                values[xIndex, yIndex] *= threadCurve.Evaluate(values[xIndex, yIndex]) * settings.heightMultiplier;

                if(values[xIndex,yIndex] < minValue)
                {
                    minValue = values[xIndex, yIndex];
                }
                if(values[xIndex,yIndex] > maxValue)
                {
                    maxValue = values[xIndex, yIndex];
                }
            }
        }

        return new NoiseArea(values, minValue, maxValue);
    }
}

public struct NoiseArea
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public NoiseArea(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
};
