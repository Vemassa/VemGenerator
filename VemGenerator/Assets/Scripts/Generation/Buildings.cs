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

public sealed class Buildings
{
    private static readonly Buildings instance = new Buildings();

    private readonly Material material;
    private GameObject mainObject;

    static Buildings() { }

    private Buildings()
    {
        this.material = Resources.Load("MyMaterial") as Material;
    }

    public static Buildings Instance
    {
        get
        {
            return instance;
        }
    }

    public void CreateBuildings(Coords worldPoint, int worldRadius)
    {
        if (this.mainObject == null)
        {
            SetupGOInstance();
        }

        var square = CoordinatesUtils.SquareFromCenter((worldPoint.Latitude, worldPoint.Longitude), worldRadius);
        var squareSim = CoordinatesUtils.SquareFromCenterSim(new Vector3(0, 1, 0), 260);
        var tile = new Tile(square[2], square[0], squareSim[2], squareSim[0]);

        var data = Overpass.GetBuildingsInArea(tile);
        var dataObj = JsonConvert.DeserializeObject<DataProperties>(data);

        foreach (Elements elem in dataObj.elements)
        {
            Vector3[] points = new Vector3[elem.geometry.Length];

            for (int i = 0; i < elem.geometry.Length; i++)
            {
                Coords geoPoint = new Coords(elem.geometry[i].lat, elem.geometry[i].lon);
                Vector3 simPoint = SimCoordinatesUtils.GPSToSim(geoPoint, tile);

                points[i] = simPoint;
            }

            GenerateBuilding(points);
        }
    }

    private void SetupGOInstance()
    {
        this.mainObject = new GameObject("BuildingsRoot");
        this.mainObject.transform.position = new Vector3(0, 0, 0);
    }

    private void GenerateBuilding(Vector3[] vertices)
    {
        // Pass as class constant variable.
        const float buildingHeight = 5;
        var isClockwise = SimCoordinatesUtils.PolygonIsClockwise(vertices);

        Mesh newMesh = new Mesh();
        GameObject building = new GameObject("Building");
        GameObject shapes = new GameObject("Shapes");

        building.transform.parent = mainObject.transform;
        building.transform.position = new Vector3(0, 0, 0);
        shapes.transform.parent = building.transform;

        shapes.AddComponent<MeshFilter>();
        shapes.AddComponent<MeshRenderer>();
        shapes.transform.position = new Vector3(0, 0, 0);
        shapes.GetComponent<MeshFilter>().mesh = newMesh;
        shapes.GetComponent<MeshRenderer>().material = this.material;

        CombineInstance[] combine = new CombineInstance[vertices.Length];
        Vector3[] extrudedVertices = new Vector3[vertices.Length];
        int[] triangles;

        for (int i = 0; i < vertices.Length; i++)
        {
            extrudedVertices[i] = new Vector3(vertices[i].x, vertices[i].y + buildingHeight, vertices[i].z);

            if (i > 0)
            {
                if (isClockwise)
                {
                    triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
                } else
                {
                    triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
                }
                Mesh face = new Mesh
                {
                    vertices = new Vector3[] { vertices[i - 1], extrudedVertices[i - 1], vertices[i], extrudedVertices[i] },
                    triangles = triangles,
            };

                combine[i - 1].mesh = face;
                combine[i - 1].transform = building.transform.localToWorldMatrix;
            }
        }

        combine[vertices.Length - 1].mesh = GenerateRoof(extrudedVertices);
        combine[vertices.Length - 1].transform = building.transform.localToWorldMatrix;

        newMesh.CombineMeshes(combine);
        newMesh.RecalculateNormals();
    }

    private Mesh GenerateRoof(Vector3[] vertices)
    {
        Mesh roof = new Mesh();

        Vector3[] optimizedVertices = new Vector3[vertices.Length - 1];

        for (int i = 0; i < vertices.Length - 1; i++)
        {
            optimizedVertices[i] = vertices[i];
        }

        int[] roofTriangles = GenerateTriangles(optimizedVertices);

        roof.vertices = optimizedVertices;
        roof.triangles = roofTriangles;
        roof.RecalculateNormals();

        return roof;
    }

    // Triangles have to be built clock-wise.
    // Depending if received data points are clockwise or counter-clockwise, triangles have to be built from different points.
    private int[] GenerateTriangles(Vector3[] extrudedVertices)
    {
        Vector2[] roof = new Vector2[extrudedVertices.Length];
        for (int i = 0; i < extrudedVertices.Length; i++)
        {
            roof[i] = new Vector2(extrudedVertices[i].x, extrudedVertices[i].z);
        }

        Triangulator tr = new Triangulator(roof);
        int[] triangles = tr.Triangulate();

        return (triangles);
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