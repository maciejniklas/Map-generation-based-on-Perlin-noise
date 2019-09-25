using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapController))]
public class MapControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapController controller = (MapController)target;

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
