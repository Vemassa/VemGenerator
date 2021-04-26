using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimCoordinatesUtils
{
    static Vector3 SimPointFromPer(Vector3 from, Vector3 to, float perX, float perZ)
    {
        return new Vector3(from.x + (to.x - from.x) * perX, 1, from.z + (to.z - from.z) * perZ);
    }

    public static Vector3 GPSToSim(Coords GPSPoint, Tile tile) {

        var xPercentageFromBottomLeft = ((GPSPoint.Longitude - tile.BottomLeft.Longitude) / (tile.TopRight.Longitude - tile.BottomLeft.Longitude));
        var zPercentageFromBottomLeft = ((GPSPoint.Latitude - tile.BottomLeft.Latitude) / (tile.TopRight.Latitude - tile.BottomLeft.Latitude));
        var simPoint = SimPointFromPer(tile.SimBottomLeft, tile.SimTopRight, xPercentageFromBottomLeft, zPercentageFromBottomLeft);

        return simPoint;
    }
}
