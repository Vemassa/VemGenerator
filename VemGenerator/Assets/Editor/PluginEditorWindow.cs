using UnityEditor;
using UnityEngine;

public class PluginEditorWindow : EditorWindow
{
    static private Vector2 lastWorldPoint = new Vector2(0, 0);
    static private int radius = 0;

    [MenuItem("Tools/VemGeneration")]
    public static void ShowWindow()
    {
        GetWindow<PluginEditorWindow>(false, "VemGeneration", true);
    }

    public void OnGUI()
    {
        lastWorldPoint = EditorGUILayout.Vector2Field("Center point coordinates (x = Longitude, y = Latitude).", lastWorldPoint);
        EditorGUILayout.Space();
        radius = EditorGUILayout.IntField("Radius from center point (meters).", radius);
        EditorGUILayout.Space();

        if (GUILayout.Button("Create"))
        {
            var root = GameObject.Find("BuildingsRoot");

            if (root != null)
            {
                DestroyImmediate(root);
            }

            Buildings.Instance.CreateBuildings(new Coords(lastWorldPoint.y, lastWorldPoint.x), radius);
        }
        if (GUILayout.Button("Reset"))
        {
            var root = GameObject.Find("BuildingsRoot");

            if (root != null)
            {
                DestroyImmediate(root);
            }
        }
    }

    private void OnInspectorUpdate()
    {
        this.Repaint();
    }

}
