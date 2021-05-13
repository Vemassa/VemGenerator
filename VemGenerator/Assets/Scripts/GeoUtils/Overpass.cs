using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

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
public static class Overpass
{
    public static string GetBuildingsInArea(Tile tile)
    {
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://lz4.overpass-api.de/api/interpreter");
        var areaString = tile.BottomLeft.Latitude.ToString().Replace(',', '.') + ", " + tile.BottomLeft.Longitude.ToString().Replace(',', '.') + ", " + tile.TopRight.Latitude.ToString().Replace(',', '.') + ", " + tile.TopRight.Longitude.ToString().Replace(',', '.');
        var postData = "data=" + "[out:json];way[\"building\"](" + areaString + ");out geom;";
        var data = Encoding.ASCII.GetBytes(postData);

        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = data.Length;

        Stream dataStream = request.GetRequestStream();
        dataStream.Write(data, 0, data.Length);
        dataStream.Close();

        WebResponse response = request.GetResponse();
        string responseFromServer = "";

        using (dataStream = response.GetResponseStream())
        {
            StreamReader reader = new StreamReader(dataStream);
             responseFromServer = reader.ReadToEnd();
        }

        response.Close();

        return (responseFromServer);
    }
}
