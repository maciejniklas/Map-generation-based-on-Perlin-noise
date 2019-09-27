using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AreaAsset : UpdatableAsset
{
    public float scale = 1f;
    public float heightMultiplier;
    public AnimationCurve curve;

    public bool useFalloff = false;
    public bool useFlatshading = false;
}
