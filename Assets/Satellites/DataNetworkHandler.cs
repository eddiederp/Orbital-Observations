using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

public class DataNetworkHandler : MonoBehaviour
{
    private string requestURL = "https://celestrak.org/NORAD/elements/gp.php?GROUP=active&FORMAT=CSV";
    public async Task<int> GetSatelliteData(string orbitDataFilePath, CancellationToken token)
    {
        if (ValidTimePassed(out float time))
        {
            PlayerPrefs.SetString("LastRequestTime", DateTime.UtcNow.ToString());
            PlayerPrefs.SetString("LastRequestURL", requestURL);
            UnityWebRequest www = UnityWebRequest.Get(requestURL);
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    www.Abort();
                    token.ThrowIfCancellationRequested();
                }
                await Task.Yield();
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"Network error: {www.error}");
            }
            else
            {
                File.WriteAllText(orbitDataFilePath, www.downloadHandler.text);
                return 1;
            }
        }
        else
        {
            return 0;
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
}