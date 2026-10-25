using TMPro;
using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public TMP_Text progress;
    public SatelliteManager manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingScreen.SetActive(true);
    }

    void Update()
    {
        if (manager.finishedLoading == false)
        {
            SetProgress();
        }
        else
        {
            loadingScreen.SetActive(false);
            enabled = false;
        }
    }

    void SetProgress()
    {
        string text;
        switch (manager.totalSatellites)
        {
            case -2:
                text = "Fetching Satellite Data";
                break;
            case -1:
                text = "Parsing Satellite Data";
                break;
            default:
                text = $"Loaded {manager.currentSatellites} out of {manager.totalSatellites}";
                break;
        }
        progress.text = text;
    }
}
