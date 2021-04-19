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
        var myString = EditorGUILayout.TextField("Text Field", "flflfl");
    }
}
