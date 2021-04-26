using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

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
    public static IEnumerator GetBuildingsInArea(Tile tile, Action<string> callback)
    {
        var areaString = tile.BottomLeft.Latitude.ToString().Replace(',', '.') + ", " + tile.BottomLeft.Longitude.ToString().Replace(',', '.') + ", " + tile.TopRight.Latitude.ToString().Replace(',', '.') + ", " + tile.TopRight.Longitude.ToString().Replace(',', '.');
        WWWForm form = new WWWForm();

        form.AddField("data", "[out:json];way[\"building\"](" + areaString + ");out geom;");

        using (UnityWebRequest www = UnityWebRequest.Post("https://overpass-api.de/api/interpreter", form))
        {
            yield return www.SendWebRequest();

            if (www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                callback(www.downloadHandler.text);
            }
        }
    }
}
