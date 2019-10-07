using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapHandler))]
public class MapHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapHandler controller = (MapHandler)target;

        if(DrawDefaultInspector() && controller.autoUpdate)
        {
            controller.DisplayInEditor();
        }

        if (GUILayout.Button("Generate"))
        {
            controller.DisplayInEditor();
        }
    }
}
