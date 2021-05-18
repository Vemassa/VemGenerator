using UnityEditor;
using UnityEngine;

public class PluginEditorWindow : EditorWindow
{
    private const int MAX_RADIUS = 5000;
    private const int DEFAULT_EDITOR_RADIUS = 300;
    private const int DEFAULT_EDITOR_HEIGHT = 5;
    private readonly Vector3 DEFAULT_EDITOR_POINT = new Vector3(0, 0, 0);

    [MenuItem("Tools/VemGeneration")]
    public static void ShowWindow()
    {
        GetWindow<PluginEditorWindow>(false, "VemGeneration", true);
    }

    public void OnGUI()
    {
        if (!EditorGUIUtility.wideMode)
        {
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
        }

        InputSettings();
        EditorGUILayout.Space();
        UnitySettings();
        EditorGUILayout.Space();
        ActionButtons();
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

        var radius = EditorGUILayout.IntField("Radius (units)", SessionState.GetInt("editor_radius", DEFAULT_EDITOR_RADIUS));
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

        EditorGUI.BeginChangeCheck();

        var height = EditorGUILayout.FloatField("Height (units)", SessionState.GetFloat("editor_height", DEFAULT_EDITOR_HEIGHT));

        if (EditorGUI.EndChangeCheck())
        {
            SessionState.SetFloat("editor_height", height);
        }
    }

    private void ActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create", GUILayout.MaxWidth(300)))
        {
            GameObject buildings = Buildings.Instance.GetGameObject();

            if (buildings != null)
            {
                DestroyImmediate(buildings);
            }

            Buildings.Instance.CreateEnvironment(new Coords(SessionState.GetFloat("latitude", 0),
                SessionState.GetFloat("longitude", 0)),
                SessionState.GetInt("radius", 0),
                DEFAULT_EDITOR_POINT,
                SessionState.GetFloat("editor_height",
                DEFAULT_EDITOR_HEIGHT));
        }
        if (GUILayout.Button("Destroy", GUILayout.MaxWidth(300)))
        {
            GameObject buildings = Buildings.Instance.GetGameObject();

            if (buildings != null)
            {
                DestroyImmediate(buildings);
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}
