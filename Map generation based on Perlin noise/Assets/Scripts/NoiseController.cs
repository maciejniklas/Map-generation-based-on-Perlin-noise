using UnityEngine;

public static class NoiseController
{
    public static AreaNoise BuildNoiseArea(int size, AreaNoiseDetails areaNoiseDetails, Vector2 center)
    {
        float[,] area = Noise.GenerateAreaValues(size, areaNoiseDetails.noiseDetails, center);

        // Curve copy for every thread to avoid accessing it in same time
        AnimationCurve threadCurve = new AnimationCurve(areaNoiseDetails.curve.keys);

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for(int yIndex = 0; yIndex < size; yIndex++)
        {
            for(int xIndex = 0; xIndex < size; xIndex++)
            {
                area[xIndex, yIndex] *= threadCurve.Evaluate(area[xIndex, yIndex]) * areaNoiseDetails.heightMultiplier;

                if(area[xIndex,yIndex] < minHeight)
                {
                    minHeight = area[xIndex, yIndex];
                }
                if(area[xIndex,yIndex] > maxHeight)
                {
                    maxHeight = area[xIndex, yIndex];
                }
            }
        }

        return new AreaNoise(area, minHeight, maxHeight);
    }
}

public struct AreaNoise
{
    public readonly float[,] area;
    public readonly float minHeight;
    public readonly float maxHeight;

    public AreaNoise(float[,] values, float minHeight, float maxHeight)
    {
        this.area = values;
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;
    }
};
