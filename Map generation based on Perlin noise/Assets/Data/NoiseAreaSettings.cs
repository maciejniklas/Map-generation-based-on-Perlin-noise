using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseAreaSettings : UpdatableAsset
{
    public NoiseSettings noiseSettings;

    public float heightMultiplier;
    public AnimationCurve curve;
    public bool useFalloff = false;

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        noiseSettings.Validate();

        base.OnValidate();
    }

#endif

    public float minHeight
    {
        get
        {
            return heightMultiplier * curve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * curve.Evaluate(1);
        }
    }
}
