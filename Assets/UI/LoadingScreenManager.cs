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
            Debug.Log("yes");
            SetProgress();
        }
        else
        {
            Debug.Log("no");
            loadingScreen.SetActive(false);
            this.enabled = false;
        }
    }

    void SetProgress()
    {
        string text = $"Loaded {manager.currentSatellites} out of {manager.totalSatellites}";
        progress.text = text;
    }
}
