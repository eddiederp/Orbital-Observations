using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SatelliteManager : MonoBehaviour
{
    [Header("References")]
    public Material orbitMaterial;
    string csvFilePath;
    public TextAsset satelliteInfoFile;
    public GameObject satellitePrefab;
    public ComputeShader orbitComputeSolver;
    public DataNetworkHandler networkHandler;
    private CSVReader reader;
    private SGP4Solver solver;
    [Header("Lists")]
    public List<Satellite> satellites;
    public List<SatelliteOrbitComputeData> satelliteOrbitData;
    public List<SatelliteInfo> allSatelliteInfoDataset;
    [Header("Parameters")]
    public int currentCategory;
    int numCategories = 6;
    public float t;
    public int numSegments = 50;
    public float orbitPrediction;

    void Awake()
    {
        csvFilePath = Path.Combine(Application.persistentDataPath, "satellite_info.csv");
        SatelliteCategorizer.SetCategoryMats();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadDependencies();
        TryGetNewSatelliteData();
    }

    void Update()
    {
        UpdateSatellitesCPU();
    }

    void TryGetNewSatelliteData()
    {
        networkHandler.GetSatelliteData(csvFilePath, (success, message) =>
        {
            LoadSatelliteData();
            switch(success)
            {
                case 0:
                    Debug.LogError($"Web Request Failed! {message}");
                    break;
                case 1:
                    Debug.LogWarning($"Web request not made. {message}");
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        });
    }

    void UpdateSatellitesCPU()
    {
        foreach (Satellite satellite in satellites)
        {
            satellite.UpdateObjectPosition(solver, t);
        }
    }

    void UpdateSatellitesGPU()
    {
        solver.GetAllSatellitePositions(satelliteOrbitData, t);
    }

    void LoadDependencies()
    {
        reader = new CSVReader();
        satellites = new List<Satellite>();
        satelliteOrbitData = new List<SatelliteOrbitComputeData>();
        solver = new SGP4Solver(orbitComputeSolver);
    }

    void LoadSatelliteData()
    {
        List<SatelliteOMMData> satelliteData = reader.ReadOrbitData(csvFilePath);
        allSatelliteInfoDataset = reader.ReadSatelliteInfo(satelliteInfoFile);
        foreach (SatelliteOMMData dataObject in satelliteData)
        {
            SatelliteInfo info = allSatelliteInfoDataset.Find(i => i.noradID == dataObject.noradID);
            Satellite newSatellite = new Satellite();
            newSatellite.OMMData = dataObject;
            if (dataObject.eccentricity >= 0.9f)
            {
                continue;
            }
            //manual adjustment for starlink and oneweb constellations post ucs cutoff
            if (info.name == null)
            {
                if (dataObject.name.Contains("ISS") || dataObject.name == "POISK") //skip sub components of iss
                {
                    continue;
                }
                SatelliteInfo placeHolderInfo = SatelliteCategorizer.GetPlaceholderInfo(dataObject.name);
                if (placeHolderInfo.name != "")
                {
                    info = placeHolderInfo;
                }
                newSatellite.name = dataObject.name;
            }
            else
            {
                newSatellite.name = info.name;
            }
            newSatellite.gameObject = Instantiate(satellitePrefab);
            newSatellite.gameObject.transform.parent = gameObject.transform;
            newSatellite.gameObject.name = newSatellite.OMMData.name;
            newSatellite.gameObject.transform.localScale *= GameManager.worldScale;
            newSatellite.info = info;

            newSatellite.InitializeRenderer(orbitMaterial);
            newSatellite.RenderOrbit(solver, numSegments, orbitPrediction, t);
            newSatellite.SetMaterial(currentCategory);
            newSatellite.SetSelected(false);
            satellites.Add(newSatellite);
            satelliteOrbitData.Add(new SatelliteOrbitComputeData(dataObject));
        }
    }

    public void UpdateSatelliteVisualization(int categoryChange)
    {
        currentCategory += categoryChange;
        if (currentCategory < 0) currentCategory = numCategories - 1;
        if (currentCategory == numCategories) currentCategory = 0;
        foreach (Satellite satellite in satellites)
        {
            satellite.SetMaterial(currentCategory);
        }
    }
}
