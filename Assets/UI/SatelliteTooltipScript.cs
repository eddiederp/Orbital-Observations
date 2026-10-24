using TMPro;
using UnityEngine;

public class SatelliteTooltipScript : MonoBehaviour
{
    public GameObject tooltipPanel;
    public TMP_Text satelliteNameText;
    public TMP_Text satelliteInfoText;
    public TMP_Text satelliteIDText;
    public SatelliteManager satelliteManager;

    void Start()
    {
        tooltipPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
    }

    void TrackMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.layer == 3) //layer 3 = satellite
            {
                SetTooltipActive(true);
                Satellite satellite = satelliteManager.satellites.Find(s => s.gameObject == hitObject);
                satelliteNameText.text = satellite.name;
                satelliteIDText.text = "NORAD ID: " + satellite.OMMData.noradID.ToString();
                satelliteInfoText.text = SetInfoName(satellite.info);
                tooltipPanel.GetComponent<RectTransform>().position = Input.mousePosition;
            }
            else
            {
                SetTooltipActive(false);
            }
        }
        else
        {
            SetTooltipActive(false);
        }
    }

    void SetTooltipActive(bool active)
    {
        if (tooltipPanel.activeSelf != active)
        {
            tooltipPanel.SetActive(active);
        }
    }

    string SetInfoName(SatelliteInfo info)
    {
        int currentCategory = satelliteManager.currentCategory;
        string category = SatelliteCategorizer.categoryNames[currentCategory];
        string data = info.GetVariableFromCategoryEnum(currentCategory);
        return category + ": " + data;
    }
}
