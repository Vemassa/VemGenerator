using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

[Serializable]
public class BuildingsData
{
    public string type;
    public string generator;
    public string copyright;
    public string timestamp;
    public Feature[] features;
}

[Serializable]
public class Feature
{
    public string type;
    public Properties properties;
    public Geometry geometry;
    public string id;
}

[Serializable]
public class Properties
{
    public string id;
    public string building;
    public string source;
}

public class Tile
{
    public Vector3 TopLeft { get; }
    public Vector3 BottomRight { get; }

    public Vector3 SimTopLeft { get; }
    public Vector3 SimBottomRight { get; }

    public Tile(Vector3 topLeft, Vector3 bottomRight, Vector3 simTopLeft, Vector3 simBottomRight)
    {
        this.TopLeft = topLeft;
        this.BottomRight = bottomRight;
        this.SimTopLeft = simTopLeft;
        this.SimBottomRight = simBottomRight;
    }

    public float LatPercent(Vector2 point)
    {
        // ((input - min) * 100) / (max - min)
        return (point.x - TopLeft.x) * 100 / (BottomRight.x - TopLeft.x);
    }

    public float LonPercent(Vector2 point)
    {
        return (point.y - TopLeft.z) * 100 / (BottomRight.z - TopLeft.z);
    }

    public Vector3 SimPointFromPercentage(Vector3 pos1, Vector3 pos2, float percentage)
    {
        return Vector3.Lerp(pos1, pos2, percentage / 100);
    }

    public Vector3 GPSToSimPoint(Vector2 GPSPoint)
    {
        float percentLat = this.LatPercent(GPSPoint);
        float percentLon =this.LonPercent(GPSPoint);

        Vector3 simLat = this.SimPointFromPercentage(new Vector3(this.SimTopLeft.x, 0, this.SimTopLeft.z), new Vector3(this.SimBottomRight.x, 0, this.SimTopLeft.z), percentLat);
        Vector3 simLon = this.SimPointFromPercentage(new Vector3(this.SimTopLeft.x, 0, this.SimTopLeft.z), new Vector3(this.SimTopLeft.x, 0, this.SimBottomRight.z), percentLon);

        return new Vector3(simLat.x, 0, simLon.z);
    }
}

[Serializable]
public class Geometry
{
    public string type;
    public List<List<List<double>>> coordinates { get; set; }
}

public class Buildings : MonoBehaviour
{
    Mesh mesh;
    string path = "./Assets/Ressources/MapData/titi.json";
    public Material material;
    public Material material2;
    public Material material3;
    public Material material4;

    void Start()
    {
        GeoZone();
    }

    private void Update()
    {
    }

    private BuildingsData ParseBuildings()
    {
        return JsonConvert.DeserializeObject<BuildingsData>(File.ReadAllText(path));
    }

    private void CreateBuildings(Tile currentTile)
    {
        BuildingsData datas = ParseBuildings();

        foreach (Feature feat in datas.features)
        {
            feat.geometry.coordinates.ForEach(delegate (List<List<double>> one)
            {
                Vector3[] points = new Vector3[one.Count];

                for (int i = 0; i < one.Count; i++)
                {
                    Vector2 Point = new Vector2((float)one[i][1], (float)one[i][0]);
                    Vector3 simPos = currentTile.GPSToSimPoint(Point);

                    /*GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = simPos;
                    cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);*/

                    points[i] = simPos;
                }

                FlatBuilding(points);
            });
        }
    }

    private void GeoZone()
    {
        Vector3 TLS = new Vector3(0, 0, 50);
        Vector3 BRS = new Vector3(50, 0, 0);

        Tile mainTile = new Tile(new Vector3(43.612219f, 0, 1.4207983f), new Vector3(43.5962856f, 0, 1.4460325f), TLS, BRS);

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = mainTile.SimTopLeft;

        GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere2.transform.position = mainTile.SimBottomRight;

        CreateBuildings(mainTile);
    }

    private void FlatBuilding(Vector3[] points)
    {
        Vector3[] vertices = points;
        Vector3[] extrudedVertices = new Vector3[vertices.Length * 2];
        int[] triangles = new int[(vertices.Length - 1) * 6];

        for (int i = 0, j = 0, y = 0; j < vertices.Length; i++, j++)
        {
            extrudedVertices[i] = vertices[j];
            extrudedVertices[++i] = new Vector3(vertices[j].x, vertices[j].y - 1, vertices[j].z);

            if (y < triangles.Length)
            {
                triangles[y] = y == 0 ? 0 : triangles[y - 1] - 1; // 0
                triangles[y + 1] = triangles[y] + 1; // 1
                triangles[y + 2] = triangles[y] + 2; // 2
                y += 3;
                triangles[y] = triangles[y - 1];
                triangles[y + 1] = triangles[y - 2];
                triangles[y + 2] = triangles[y] + 1;
                y += 3;
            }
        }

        Vector2[] roof = new Vector2[vertices.Length - 1];
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            roof[i] = new Vector2(vertices[i].x, vertices[i].z);
            Debug.Log(roof[i]);
        }

        Triangulator tr = new Triangulator(roof);
        int[] indices = tr.Triangulate();
        
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] *= 2;
        }

        int[] z = triangles.Concat(indices).ToArray();

        GameObject newGameObject = new GameObject("Building");

        newGameObject.AddComponent<MeshFilter>();
        newGameObject.AddComponent<MeshRenderer>();
        newGameObject.transform.position = new Vector3(0, 0, 0);

        mesh = new Mesh();
        newGameObject.GetComponent<MeshFilter>().mesh = mesh;
        newGameObject.GetComponent<MeshRenderer>().material = material;
        mesh.vertices = extrudedVertices;
        mesh.triangles = z;
        mesh.RecalculateNormals();

        MeshFilter[] meshFilters = newGameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i2 = 0;
        while (i2 < meshFilters.Length)
        {
            combine[i2].mesh = meshFilters[i2].sharedMesh;
            combine[i2].transform = meshFilters[i2].transform.localToWorldMatrix;
            meshFilters[i2].gameObject.SetActive(false);

            i2++;
        }
        newGameObject.GetComponent<MeshFilter>().mesh = new Mesh();
        newGameObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        newGameObject.gameObject.SetActive(true);
    }
}

public class Triangulator
{
    private List<Vector2> m_points = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}