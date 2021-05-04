using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimCoordinatesUtils
{
    static Vector3 SimPointFromPer(Vector3 from, Vector3 to, float perX, float perZ)
    {
        return new Vector3(from.x + (to.x - from.x) * perX, 1, from.z + (to.z - from.z) * perZ);
    }

    public static Vector3 GPSToSim((float latitude, float longitude) GPSPoint, Tile tile)
    {
        return GPSToSim(new Coords(GPSPoint.latitude, GPSPoint.longitude), tile);
    }

    public static Vector3 GPSToSim(Coords GPSPoint, Tile tile) {

        var xPercentageFromBottomLeft = ((GPSPoint.Longitude - tile.BottomLeft.Longitude) / (tile.TopRight.Longitude - tile.BottomLeft.Longitude));
        var zPercentageFromBottomLeft = ((GPSPoint.Latitude - tile.BottomLeft.Latitude) / (tile.TopRight.Latitude - tile.BottomLeft.Latitude));
        var simPoint = SimPointFromPer(tile.SimBottomLeft, tile.SimTopRight, xPercentageFromBottomLeft, zPercentageFromBottomLeft);

        return simPoint;
    }

    public static bool PolygonIsClockwise(Vector3[] polygon)
    {
        float total = 0;
        int loopIterator;

        for (int i = 0; i < polygon.Length; i++)
        {
            loopIterator = i + 1 >= polygon.Length ? 0 : i + 1;
            total += (polygon[loopIterator].x - polygon[i].x) * (polygon[loopIterator].z + polygon[i].z);
        }

        return total >= 0;
    }
}
