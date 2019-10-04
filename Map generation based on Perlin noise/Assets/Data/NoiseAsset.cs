using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseAsset : UpdatableAsset
{
    public NoiseType type;
    public Noise.HeightNormalizeMode heightMode;

    public float scale;
    [Range(1, 2)] public int dimension = 2;

    [Range(1, 6)] public int octaves;
    [Range(0, 1)] public float perisstance;
    public float lacunarity;

    public int seed;
    public Vector3 offset;

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        base.OnValidate();
    }

#endif
}
