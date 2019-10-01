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
            UnityEditor.EditorApplication.update += Notify;
        }
    }

    public void Notify()
    {
        UnityEditor.EditorApplication.update -= Notify;
        if (onDataUpdate != null)
        {
            onDataUpdate();
        }
    }
}
