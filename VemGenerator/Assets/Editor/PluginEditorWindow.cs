using UnityEditor;
using UnityEngine;

public class PluginEditorWindow : EditorWindow
{
    static private Vector2 lastWorldPoint = new Vector2(0, 0);
    static private int radius = 0;

    private const int MAX_RADIUS = 2000;

    [MenuItem("Tools/VemGeneration")]
    public static void ShowWindow()
    {
        GetWindow<PluginEditorWindow>(false, "VemGeneration", true);
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Input datas", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Latitude / Longitude");
        lastWorldPoint.y = EditorGUILayout.FloatField(lastWorldPoint.y);
        lastWorldPoint.x = EditorGUILayout.FloatField(lastWorldPoint.x);
        EditorGUILayout.EndHorizontal();

        radius = EditorGUILayout.IntField("Radius (meters)", radius);
        if (radius < 0)
        {
            radius = 0;
        } else if (radius > MAX_RADIUS)
        {
            radius = MAX_RADIUS;
        }
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

            Buildings.Instance.CreateBuildings(new Coords(lastWorldPoint.y, lastWorldPoint.x), radius);
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

}
