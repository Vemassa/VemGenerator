using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coords
{
    public Coords(float latitude, float longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
    public float Latitude { set; get; }
    public float Longitude { set; get; }
}

public class CoordinatesUtils
{
    private const float earthRadius = 6371e3f;
    private static float ToRad(float deg)
    {
        return deg * Mathf.PI / 180;
    }

    private static float ToDeg(float rad)
    {
        return rad * 180 / Mathf.PI;
    }

    public static Coords Destination((float Lat, float Lon) startPoint, float distance, float bearing)
    {
        var brngInRad = ToRad(bearing);
        var latInRad = ToRad(startPoint.Lat);
        var lonInRad = ToRad(startPoint.Lon);

        var destLat = Mathf.Asin(Mathf.Sin(latInRad) * Mathf.Cos(distance / earthRadius) + Mathf.Cos(latInRad) * Mathf.Sin(distance / earthRadius) * Mathf.Cos(brngInRad));
        var destLon = lonInRad + Mathf.Atan2(Mathf.Sin(brngInRad) * Mathf.Sin(distance / earthRadius) * Mathf.Cos(latInRad), Mathf.Cos(distance / earthRadius) - Mathf.Sin(latInRad) * Mathf.Sin(destLat));

        return (new Coords(ToDeg(destLat), ToDeg(destLon)));
    }

    // Create geo square from given center point.
    // Returns array with geopoint in clockwise order starting from top-right.
    // Radius is distance between center point and each square edges.
    public static Coords[] SquareFromCenter((float Lat, float Lon) startPoint, float radius)
    {
        var square = new Coords[4];
        var bearings = new float[] { 45, 135, 225, 315 };

        for (int i = 0; i < bearings.Length; i++)
        {
            square[i] = Destination(startPoint, radius, bearings[i]);
            //Debug.Log(square[i].Latitude + ", " + square[i].Longitude);
        }

        return square;
    }


    // Create sim(plane) square from given center point.
    // Returns array with coordinates in clockwise order starting from top-right.
    // Radius is distance between center point and each square edges.
    public static Vector3[] SquareFromCenterSim(Vector3 startPoint, float radius) {
        var square = new Vector3[4];
        var angles = new float[] { 45, 135, 225, 315 };

        for (int i = 0; i < angles.Length; i++)
        {
            var x = Mathf.Cos(angles[i] * Mathf.Deg2Rad);
            var z = Mathf.Sin(angles[i] * Mathf.Deg2Rad);

            square[i] = (new Vector3(x, 0, z) * radius) + startPoint;
        }

        return square;
    }
}
