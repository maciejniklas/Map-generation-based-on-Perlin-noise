using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableAsset : ScriptableObject
{
    public event System.Action onDataUpdate;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if(autoUpdate)
        {
            Notify();
        }
    }

    public void Notify()
    {
        if(onDataUpdate != null)
        {
            onDataUpdate();
        }
    }
}
