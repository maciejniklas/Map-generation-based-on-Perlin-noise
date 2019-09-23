using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureGenerator))]
public class TextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        if(EditorGUI.EndChangeCheck() && Application.isPlaying)
        {
            (target as TextureGenerator).FillTexture();
        }
    }
}
