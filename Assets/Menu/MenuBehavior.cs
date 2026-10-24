using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehavior : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    public void MenuToDesktop()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif    
    }

    public void MenuToApp()
    {
        SceneManager.LoadScene(1);
    }
    public void AppToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void MenuToSettings()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    public void MenuToCredits()
    {
        menuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }
    public void SettingsToMenu()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
    public void CreditsToMenu()
    {
        creditsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}
