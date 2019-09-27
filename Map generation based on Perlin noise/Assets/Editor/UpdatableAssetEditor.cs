using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableAsset), true)]
public class UpdatableAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdatableAsset asset = (UpdatableAsset)target;

        if(GUILayout.Button("Update"))
        {
            asset.Notify();
        }
    }
}
