using System;
using System.Threading;
using System.Threading.Tasks;
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
    private CancellationTokenSource _cts;
    public ComputeShader orbitComputeSolver;
    public DataNetworkHandler networkHandler;
    private CSVReader reader;
    private SGP4Solver solver;
    [Header("Lists")]
    public List<Satellite> satellites;
    public List<SatelliteOrbitComputeData> satelliteOrbitData;
    public List<SatelliteInfo> allSatelliteInfoDataset;
    public SatelliteInfoDisplayManager satelliteInfoDisplay;
    [Header("Parameters")]
    public int currentCategory;
    int numCategories = 6;
    public float t;
    public int numSegments = 50;
    public float orbitPrediction;
    public int totalSatellites = -1;
    public int currentSatellites = 0;
    public bool finishedLoading = false;

    void Awake()
    {
        csvFilePath = Path.Combine(Application.persistentDataPath, "satellite_info.csv");
        SatelliteCategorizer.SetCategoryMats();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadDependencies();
        _cts = new CancellationTokenSource();
        _ = LoadSatelliteData(_cts.Token);
    }

    void Update()
    {
        UpdateSatellitesCPU();
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
        reader = new CSVReader(numCategories);
        satellites = new List<Satellite>();
        satelliteOrbitData = new List<SatelliteOrbitComputeData>();
        solver = new SGP4Solver(orbitComputeSolver);
    }

    private async Task LoadSatelliteData(CancellationToken token)
    {
        try
        {
            await networkHandler.GetSatelliteData(csvFilePath, token); //get celestrak data and write to csv if enough time has passed
            totalSatellites = -1; //signal to loading screen we have finished loading data

            string[] ommDataLines = File.ReadAllLines(csvFilePath); 
            string[] infoLines = satelliteInfoFile.text.Split(Environment.NewLine);

            Dictionary<int, Dictionary<string, int>> occurenceDict;
            List<SatelliteOMMData> satelliteData = await Task.Run(() => reader.ReadOrbitData(ommDataLines, token));
            (allSatelliteInfoDataset, occurenceDict) = await Task.Run(() => reader.ReadSatelliteInfo(infoLines, token));
            SatelliteCategorizer.occurenceDictionary = occurenceDict; // pass occurencedict to categorizer

            totalSatellites = satelliteData.Count;
            currentSatellites = 0;

            foreach (SatelliteOMMData dataObject in satelliteData)
            {
                token.ThrowIfCancellationRequested();
                SatelliteInfo info = allSatelliteInfoDataset.Find(i => i.noradID == dataObject.noradID);
                Satellite newSatellite = new Satellite();
                newSatellite.OMMData = dataObject;
                //manual adjustment for starlink and oneweb constellations post ucs cutoff
                if (info.name == null)
                {
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

                currentSatellites += 1;    
                if (currentSatellites % 1000 == 0) await Task.Yield();
            }

            finishedLoading = true;
            satelliteInfoDisplay.InitInfoDisplay();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Operation cancelled");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error: " + ex.Message);
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

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
