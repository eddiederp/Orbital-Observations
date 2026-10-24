using System;
using UnityEngine;

[Serializable]
public struct SatelliteOMMData
{
    //based on OMM Standard
    public string name;
    public string id;
    public DateTime epoch;
    public float meanMotion;
    public float eccentricity;
    public float inclination;
    public float longitudeAscendingNode;
    public float argumentOfPericenter;
    public float meanAnomaly;
    public int ephemerisType;
    public char classType;
    public int noradID;
    public int elementSetNumber;
    public int revolutionAtEpoch;
    public float BStar;
    public float meanMotionDerivative;
    public float meanMotionDoubleDerivative;
}

[Serializable]
public struct SatelliteOrbitComputeData
{
    float epoch;
    float n0;
    float e0;
    float i0;
    float M0;
    float omega0;
    float bigOmega0;
    float bStar;

    public SatelliteOrbitComputeData(SatelliteOMMData data)
    {
        epoch = (float)(data.epoch - DateTime.UnixEpoch).TotalMinutes;
        n0 = data.meanMotion * Mathf.PI * 2.0f / 1440.0f;
        e0 = data.eccentricity;
        i0 = data.inclination * Mathf.Deg2Rad;
        M0 = data.meanAnomaly * Mathf.Deg2Rad;
        omega0 = data.argumentOfPericenter * Mathf.Deg2Rad;
        bigOmega0 = data.longitudeAscendingNode * Mathf.Deg2Rad;
        bStar = data.BStar;
    }
}

[Serializable]
public struct SatelliteInfo
{
    public SatelliteInfo(SatelliteInfo info)
    {
        name = info.name;
        countryUNRegistry = info.countryUNRegistry;
        countryOperator = info.countryOperator;
        owner = info.owner;
        users = info.users;
        purpose = info.purpose;
        orbitClass = info.orbitClass;
        orbitType = info.orbitType;
        launchMass = info.launchMass;
        launchDate = info.launchDate;
        contractor = info.contractor;
        countryContractor = info.countryContractor;
        launchSite = info.launchSite;
        launchVehicle = info.launchVehicle;
        cosparID = info.cosparID;
        noradID = info.noradID;
    }
    public string name;
    public string countryUNRegistry;
    public string countryOperator;
    public string owner;
    public string users;
    public string purpose;
    public string orbitClass;
    public string orbitType;
    public int launchMass;
    public DateTime launchDate;
    public string contractor;
    public string countryContractor;
    public string launchSite;
    public string launchVehicle;
    public string cosparID;
    public int noradID;

    public string GetVariableFromCategoryEnum(int category)
    {
        switch (category)
        {
            case 0:
                return countryOperator;
            case 1:
                return orbitClass;
            case 2:
                return orbitType;
            case 3:
                return purpose;
            case 4:
                return users;
            case 5:
                return launchSite;
            default:
                return null;
        }
    }
}