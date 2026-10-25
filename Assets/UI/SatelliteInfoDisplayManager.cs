using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SatelliteInfoDisplayManager : MonoBehaviour
{
    public SatelliteManager satelliteManager;
    public TMP_Text categoryText;
    public GameObject infoDisplay;
    public GameObject keyItemPrefab;
    public AdditionalVisualizationController additionalVisController;
    public GameObject barChartDisplay;
    public GameObject barChartItemPrefab;

    public void InitInfoDisplay()
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
        
        ResetDisplayContent();
        int largestAmount = 0;
        foreach (KeyValuePair<string, Material> pair in SatelliteCategorizer.GetCategoryPairs(satelliteManager.currentCategory))
        {
            CreateInfoDisplayContent(pair.Key, pair.Value);
            int amount = SatelliteCategorizer.occurenceDictionary[satelliteManager.currentCategory][pair.Key];
            if (largestAmount == 0)
            {
                CreateBarChartDisplayContent(amount, amount, pair.Value);
                largestAmount = amount;
            }
            else
            {
                CreateBarChartDisplayContent(amount, largestAmount, pair.Value);
            }
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

    void CreateBarChartDisplayContent(int amount, int largestAmount, Material mat)
    {
        GameObject barChartItem = Instantiate(barChartItemPrefab);
        barChartItem.transform.SetParent(barChartDisplay.transform);
        float height = barChartItem.GetComponent<RectTransform>().rect.height;
        barChartItem.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * (amount / (float)largestAmount));
        TMP_Text itemText = barChartItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        itemText.text = amount.ToString();
        barChartItem.GetComponent<RawImage>().color = mat.color;
    }

    void ResetDisplayContent()
    {
        for (int i = infoDisplay.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(infoDisplay.transform.GetChild(i).gameObject);
        }
        for (int i = barChartDisplay.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(barChartDisplay.transform.GetChild(i).gameObject);
        }
    }
}
