using UnityEngine;

[CreateAssetMenu()]
public class AreaNoiseDetails : UpdatableAsset
{
    public NoiseDetials noiseDetails;

    public float heightMultiplier;
    public AnimationCurve curve;
    public bool useFalloff = false;

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        noiseDetails.Validate();

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
