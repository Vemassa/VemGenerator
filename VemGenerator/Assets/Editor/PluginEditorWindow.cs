using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PluginEditorWindow : EditorWindow
{

    [MenuItem("VemGeneration/Tools")]
    public static void ShowWindow()
    {
        GetWindow<PluginEditorWindow>(false, "VemGeneration", true);
    }

    public void OnGUI()
    {
        var centerGeoPoint = EditorGUILayout.Vector2Field("Center point coordinates", new Vector2(0, 0));
    }
}
