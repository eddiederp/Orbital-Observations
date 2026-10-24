using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SatelliteInfoDisplayManager : MonoBehaviour
{
    public SatelliteManager satelliteManager;
    public TMP_Text categoryText;
    public GameObject infoDisplay;
    public GameObject keyItemPrefab;
    public AdditionalVisualizationController additionalVisController;

    void Start()
    {
        additionalVisController.SetInitVisualization(satelliteManager.currentCategory);
        UpdateCategory(0);
    }

    public void UpdateCategory(int change)
    {
        if (change != 0)
        {
            satelliteManager.UpdateSatelliteVisualization(change);
        }
        additionalVisController.SetNewVisualization(satelliteManager.currentCategory);
        categoryText.text = SatelliteCategorizer.categoryNames[satelliteManager.currentCategory];
        
        ResetInfoDisplayContent();
        foreach (KeyValuePair<string, Material> pair in SatelliteCategorizer.GetCategoryPairs(satelliteManager.currentCategory))
        {
            CreateInfoDisplayContent(pair.Key, pair.Value);
        }
        CreateInfoDisplayContent("Other", SatelliteCategorizer.otherMat);
        CreateInfoDisplayContent("No Data", SatelliteCategorizer.noDataMat);
    }

    void CreateInfoDisplayContent(string text, Material mat)
    {
        GameObject keyItem = Instantiate(keyItemPrefab);
        keyItem.transform.SetParent(infoDisplay.transform);
        TMP_Text keyDesc = keyItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        keyDesc.text = text;
        keyDesc.color = mat.color;
    }

    void ResetInfoDisplayContent()
    {
        for (int i = infoDisplay.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(infoDisplay.transform.GetChild(i).gameObject);
        }
    }
}
