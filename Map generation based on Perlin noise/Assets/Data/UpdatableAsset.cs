using UnityEngine;

public class UpdatableAsset : ScriptableObject
{
    public event System.Action onDataUpdate;
    public bool autoUpdate;

#if UNITY_EDITOR

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
        onDataUpdate();
    }

#endif
}
