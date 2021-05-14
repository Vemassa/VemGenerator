using UnityEditor;
using UnityEngine;

public class PluginEditorWindow : EditorWindow
{
    private const int MAX_RADIUS = 2000;

    [MenuItem("Tools/VemGeneration")]
    public static void ShowWindow()
    {
        GetWindow<PluginEditorWindow>(false, "VemGeneration", true);
    }

    public void OnGUI()
    {
        InputSettings();
        EditorGUILayout.Space();
        UnitySettings();
        EditorGUILayout.Space();

        #region Buttons
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create", GUILayout.MaxWidth(300)))
        {
            var root = GameObject.Find("BuildingsRoot");

            if (root != null)
            {
                DestroyImmediate(root);
            }

            Buildings.Instance.CreateEnvironment(new Coords(SessionState.GetFloat("latitude", 0), SessionState.GetFloat("longitude", 0)), SessionState.GetInt("radius", 0));
        }
        if (GUILayout.Button("Reset", GUILayout.MaxWidth(300)))
        {
            var root = GameObject.Find("BuildingsRoot");

            if (root != null)
            {
                DestroyImmediate(root);
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        #endregion
    }

    private void OnInspectorUpdate()
    {
        this.Repaint();
    }

    private void InputSettings()
    {
        EditorGUILayout.LabelField("Input datas", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Latitude / Longitude");
        var latitude = EditorGUILayout.FloatField(SessionState.GetFloat("latitude", 0));
        var longitude = EditorGUILayout.FloatField(SessionState.GetFloat("longitude", 0));

        if (EditorGUI.EndChangeCheck())
        {
            SessionState.SetFloat("latitude", latitude);
            SessionState.SetFloat("longitude", longitude);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();

        var radius = EditorGUILayout.IntField("Radius (meters)", SessionState.GetInt("radius", 0));
        if (radius < 0)
        {
            radius = 0;
        }
        else if (radius > MAX_RADIUS)
        {
            radius = MAX_RADIUS;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SessionState.SetInt("radius", radius);
        }
    }

    private void UnitySettings()
    {
        EditorGUILayout.LabelField("Unity settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        var radius = EditorGUILayout.IntField("Radius (units)", SessionState.GetInt("editor_radius", 0));
        if (radius < 0)
        {
            radius = 0;
        }
        else if (radius > MAX_RADIUS)
        {
            radius = MAX_RADIUS;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SessionState.SetInt("editor_radius", radius);
        }
    }
}
