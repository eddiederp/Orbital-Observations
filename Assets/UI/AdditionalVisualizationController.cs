using System;
using Unity.VisualScripting;
using UnityEngine;

public class AdditionalVisualizationController : MonoBehaviour
{
    public int currentVisCategory;
    public GameObject earthObj;
    public GameObject launchSitesContainer;

    void InitVis()
    {
        launchSitesContainer.transform.localScale = Vector3.one * GameManager.worldScale;
        foreach (Transform siteTransform in launchSitesContainer.transform.GetComponentInChildren<Transform>())
        {
            GameObject launchSite = siteTransform.gameObject;
            Material launchSiteMaterial = new Material(Shader.Find("Hidden/Unlit/Unlit Simple"));
            launchSiteMaterial.SetColor("_BaseColor", SatelliteCategorizer.GetCategoryMat(launchSite.name, 5).color);
            launchSite.GetComponent<Renderer>().material = launchSiteMaterial;
        }
    }

    public void SetInitVisualization(int initCategory)
    {
        InitVis();
        SetVisActive(initCategory, true);
        currentVisCategory = initCategory;
    }

    public void SetNewVisualization(int newCategory)
    {
        SetVisActive(currentVisCategory, false);
        SetVisActive(newCategory, true);
        currentVisCategory = newCategory;
    }

    void SetVisActive(int category, bool active)
    {
        switch (category)
            {
                case 0:
                    earthObj.GetComponent<Renderer>().material.SetFloat("_CountryOpacity", active ? 0.5f : 0.0f);
                    return;
                // case 1:
                //     return orbitClassMaterials[data];
                // case 2:
                //     return orbitTypeMaterials[data];
                // case 3:
                //     return purposeMaterials[data];
                // case 4:
                //     return usersMaterials[data];
                case 5:
                    earthObj.GetComponent<Renderer>().material.SetFloat("_CountryOpacity", active ? 0.1f : 0.0f);
                    launchSitesContainer.SetActive(active);
                    return;
                default:
                    Debug.LogError("Category not handled!");
                    return;
            }
    }
}