using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsController : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] filteredResolutions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupResolutionDropdown();
        
        fullscreenToggle.isOn = Screen.fullScreen;

    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    void SetupResolutionDropdown() //ai slop
    {
        // 1. Get all resolutions supported by the current monitor
        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        List<Resolution> uniqueResolutions = new List<Resolution>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            // Optional: Filter out duplicate resolutions with different refresh rates
            // if you just want to show standard width x height options.
            string option = allResolutions[i].width + " x " + allResolutions[i].height;
            
            if (!options.Contains(option))
            {
                options.Add(option);
                uniqueResolutions.Add(allResolutions[i]);

                // Check if this matches the current application resolution
                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = uniqueResolutions.Count - 1;
                }
            }
        }

        // Cache the list to map the dropdown index back to the resolution object
        filteredResolutions = uniqueResolutions.ToArray();

        // 2. Populate the dropdown UI
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // 3. Public function triggered when the player changes the dropdown
    public void SetResolution(int resolutionIndex) //ai slop
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        
        // Applies width, height, and preserves the current fullscreen setting
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
