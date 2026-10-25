using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public float latitude;
    public float longitude;
    public float altitude;
    public float satelliteDistance;
    [Header("Parameters")]
    public float minAltitude;
    public float minSatelliteDistance;
    public float panSpeed;
    public float zoomSpeed;
    [Header("Satellite Selection")]
    private Satellite currentSatellite;
    public bool isTrackingSatellite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        latitude = 0f;
        longitude = 0f;
        altitude = 13000f;
        satelliteDistance = 4000f;
        isTrackingSatellite = false;
        currentSatellite = null;
    }

    public Satellite GetCurrentTrackedSatellite()
    {
        return currentSatellite;
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseInput();
        MoveCamera();
    }

    void GetMouseInput()
    {   
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (isTrackingSatellite)
            {
                satelliteDistance -= Input.mouseScrollDelta.y * satelliteDistance * zoomSpeed * Time.deltaTime;
                satelliteDistance = Mathf.Max(satelliteDistance, minSatelliteDistance);
            }
            else 
            {
                altitude -= Input.mouseScrollDelta.y * altitude * zoomSpeed * Time.deltaTime;
                altitude = Mathf.Max(altitude, minAltitude);
            }
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            longitude -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            longitude += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            latitude += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            latitude -= panSpeed * Time.deltaTime;
        }
        latitude = Mathf.Max(Mathf.Min(latitude, Mathf.PI / 2f - 0.1f), Mathf.PI / -2f + 0.1f);
    }

    void MoveCamera()
    {
        if (isTrackingSatellite)
        {
            Vector3 satellitePosition = currentSatellite.gameObject.transform.position;
            gameObject.transform.position = GetCameraPosAtCoords(satellitePosition, satellitePosition.normalized, latitude, longitude, satelliteDistance);
            gameObject.transform.LookAt(satellitePosition, satellitePosition.normalized);
        }
        else
        {
            gameObject.transform.position = GetCameraPosAtCoords(Vector3.zero, Vector3.up, latitude, longitude, altitude);  
            gameObject.transform.LookAt(Vector3.zero); 
        }
    }

    Vector3 GetCameraPosAtCoords(Vector3 center, Vector3 upDir, float lat, float lon, float alt)
    {
        Vector3 position = new Vector3(
            alt * Mathf.Cos(lat) * Mathf.Cos(lon),
            alt * Mathf.Sin(lat),
            alt * Mathf.Cos(lat) * Mathf.Sin(lon)
        );
        if (upDir != Vector3.up && center != Vector3.zero)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, upDir);
            position = rotation * position;
        }
        Vector3 finalPos = position * GameManager.worldScale + center;
        //Debug.Log($"Center: {center}, upDir: {upDir}, camera position: {finalPos}");
        return finalPos;
    }

    public void SelectSatellite(Satellite satellite)
    {
        currentSatellite = satellite;
        isTrackingSatellite = true;
        satellite.SetSelected(true);
    }
    public void DeselectSatellite()
    {
        Debug.Log(currentSatellite);
        if (currentSatellite != null)
        {
            currentSatellite.SetSelected(false);
            currentSatellite = null; 
        }
        isTrackingSatellite = false;
    }
}
