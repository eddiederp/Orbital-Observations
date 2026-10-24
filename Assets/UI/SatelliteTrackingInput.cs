using TMPro;
using UnityEngine;

public class SatelliteTrackingInput : MonoBehaviour
{
    public TMP_Text satelliteTrackingText;
    public TMP_InputField satelliteTrackingField;
    public SatelliteManager satelliteManager;
    public CameraController cameraController;

    string noSatelliteTracked = "No Satellite Selected";
    string notIntError = "Invalid number entered";
    string notFoundError = "NORAD ID not found in dataset";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ValidateInput()
    {
        if (satelliteTrackingField.text != "")
        {
            int id = 0;
            if (int.TryParse(satelliteTrackingField.text, out id))
            {
                Satellite satellite = satelliteManager.satellites.Find(s => s.OMMData.noradID == id);
                if (satellite != null)
                {
                    cameraController.DeselectSatellite();
                    cameraController.SelectSatellite(satellite);
                    satelliteTrackingText.text = "Tracking Satellite: " + satellite.name;
                }
                else
                {
                    cameraController.DeselectSatellite();
                    satelliteTrackingText.text = notFoundError;
                }
            }
            else
            {
                cameraController.DeselectSatellite();
                satelliteTrackingText.text = notIntError;
            }
        }
        else
        {
            cameraController.DeselectSatellite();
            satelliteTrackingText.text = noSatelliteTracked;
        }
    }
}
