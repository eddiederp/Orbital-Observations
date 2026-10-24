using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.IO;

public class DataNetworkHandler : MonoBehaviour
{
    private string requestURL = "https://celestrak.org/NORAD/elements/gp.php?GROUP=active&FORMAT=CSV";
    public void GetSatelliteData(string orbitDataFilePath, Action<int, string> onComplete)
    {
        if (ValidTimePassed(out float time))
        {
            PlayerPrefs.SetString("LastRequestTime", DateTime.UtcNow.ToString());
            PlayerPrefs.SetString("LastRequestURL", requestURL);
            StartCoroutine(GetSatelliteDataCoroutine(orbitDataFilePath, onComplete));
        }
        else
        {
            onComplete?.Invoke(1, $"Not enough time passed. ({time} minutes)");
        }
    }

    public bool ValidTimePassed(out float time)
    {
        string lastRequestTimeStr = PlayerPrefs.GetString("LastRequestTime");
        string lastRequestURL = PlayerPrefs.GetString("LastRequestURL");
        if (lastRequestTimeStr == String.Empty || lastRequestURL == String.Empty)
        {
            time = 0;
            return true; //just return true if we haven't set string yet ig
        }
        DateTime lastRequestTime = DateTime.Parse(lastRequestTimeStr);
        double timePassed = (DateTime.UtcNow - lastRequestTime).TotalMinutes;
        Debug.Log(lastRequestURL);
        time = (float)timePassed;
        return !(timePassed < 120.0 && requestURL == lastRequestURL); //returns false if less than 2 hours after last request and we are getting same request
    }

    private IEnumerator GetSatelliteDataCoroutine(string filePath, Action<int, string> onComplete)
    {
        UnityWebRequest www = UnityWebRequest.Get(requestURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onComplete?.Invoke(0, www.error);
        }
        else
        {
            File.WriteAllText(filePath, www.downloadHandler.text);
            onComplete?.Invoke(2, "Success");
        }
    }
}