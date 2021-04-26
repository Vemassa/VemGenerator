using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[Serializable]
public class DataProperties
{
    public string version;
    public string generator;
    public Osm3s osm3s;
    public Elements[] elements;
}

public class Osm3s
{
    public string timestamp_osm_base;
    public string copyright;
}

public class Elements
{
    public string type;
    public string id;
    public Bounds bounds;
    public long[] nodes;
    public Geometry[] geometry;
    public Tags tags;
}

public class Bounds
{
    public float minLat;
    public float minLon;
    public float maxLat;
    public float maxLon;
}

public class Geometry
{
    public float lat;
    public float lon;
}

public class Tags
{
    public string building;
    public string buildingLevels;
    public string source;
}

public class Tile
{
    public Coords BottomLeft { get; }
    public Coords TopRight { get; }
    public Vector3 SimBottomLeft { get; }
    public Vector3 SimTopRight { get; }

    public Tile(Coords bottomLeft, Coords topRight, Vector3 simBottomLeft, Vector3 simTopRight)
    {
        this.BottomLeft = bottomLeft;
        this.TopRight = topRight;
        this.SimBottomLeft = simBottomLeft;
        this.SimTopRight = simTopRight;
    }
}

public class Buildings : MonoBehaviour
{
    Mesh mesh;
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

    private void GeoZone()
    {

        float lat = 43.59025623945865f;
        float lon = 1.4394838799801293f;
        float radius = 100;

        var square = CoordinatesUtils.SquareFromCenter((lat, lon), radius);
        var squareSim = CoordinatesUtils.SquareFromCenterSim(new Vector3(0, 1, 0), radius);

        var tile = new Tile(square[2], square[0], squareSim[2], squareSim[0]);

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = tile.SimBottomLeft;

        GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere2.transform.position = tile.SimTopRight;

        StartCoroutine(Overpass.GetBuildingsInArea(tile, data =>
        {
            var dataProperties = JsonConvert.DeserializeObject<DataProperties>(data);
            CreateBuildings(tile, dataProperties);
        }));
    }

    private void CreateBuildings(Tile tile, DataProperties data)
    {

        foreach (Elements elem in data.elements)
        {
            Vector3[] points = new Vector3[elem.geometry.Length];

            for (int i = 0; i < elem.geometry.Length; i++)
            {
                Coords geoPoint = new Coords(elem.geometry[i].lat, elem.geometry[i].lon);
                Vector3 simPoint = SimCoordinatesUtils.GPSToSim(geoPoint, tile);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                cube.transform.position = simPoint;
                cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                points[i] = simPoint;
            }

            FlatBuilding(points);
        }
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
            //Debug.Log(roof[i]);
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