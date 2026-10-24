using System;
using UnityEngine;

[Serializable]
public class Satellite
{
    public string name;
    public SatelliteOMMData OMMData;
    public SatelliteInfo info;
    public GameObject gameObject;
    public LineRenderer orbitRenderer;

    public void UpdateObjectPosition(SGP4Solver solver, float t)
    {
        Vector3 newPosition = solver.GetPositionAndVelocityFromOMM(OMMData, t).Item1;
        gameObject.transform.position = newPosition * GameManager.worldScale;
    }

    public void UpdateObjectPosition(Vector3 position)
    {
        gameObject.transform.position = position;
    }

    public void InitializeRenderer(Material orbitMaterial)
    {
        orbitRenderer = gameObject.AddComponent<LineRenderer>();
        orbitRenderer.material = orbitMaterial;
        orbitRenderer.startWidth = 10 * GameManager.worldScale;
        orbitRenderer.endWidth = 10 * GameManager.worldScale;
    }

    public void RenderOrbit(SGP4Solver solver, int steps, float predictionLength, float t)
    {

        //meanMotion: rev/day -> t to step by 
        float minPerRev = 1440.0f * predictionLength / OMMData.meanMotion;
        int orbitRes = (int)(steps * minPerRev);
        float tPerStep = minPerRev / orbitRes;

        Vector3[] orbitPositions = new Vector3[orbitRes + 1];
        
        for (int i = 0; i <= orbitRes; i++)
        {
            orbitPositions[i] = solver.GetPositionAndVelocityFromOMM(OMMData, t + i * tPerStep).Item1 * GameManager.worldScale;
        }
        orbitRenderer.positionCount = orbitRes + 1;
        orbitRenderer.SetPositions(orbitPositions);
    }

    public void SetSelected(bool selected)
    {
        orbitRenderer.enabled = selected;
    }

    public void SetMaterial(int category)
    {
        gameObject.GetComponent<Renderer>().material = SatelliteCategorizer.GetSatelliteMaterial(info, category);
    }
}