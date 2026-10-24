using System;
using UnityEngine;

public class SunTracker : MonoBehaviour
{
    public SatelliteManager manager;
    // Update is called once per frame
    void Update()
    {
        TrackSun();
    }

    void TrackSun()
    {
        DateTime currentTime = DateTime.UtcNow.AddMinutes(manager.t);
        float dayProgress = currentTime.Hour / 24f + currentTime.Minute / 1440f;
        float rotation = 360f * dayProgress;
        gameObject.transform.rotation = Quaternion.Euler(0f, rotation + 90f, 0f);
    }
}
