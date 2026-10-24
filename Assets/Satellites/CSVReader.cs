using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Globalization;
using System.IO;

public class CSVReader
{
    int headerLines = 1;
    char delimiter = ',';

    public List<SatelliteOMMData> ReadOrbitData(string orbitDataFilePath)
    {
        List<SatelliteOMMData> orbitDataList = new List<SatelliteOMMData>();
        
        string[] orbitDataLines = File.ReadAllLines(orbitDataFilePath);

        for (int i = headerLines; i < orbitDataLines.Length; i++)
        {
            string[] elements = orbitDataLines[i].Split(delimiter);
            try
            {
                SatelliteOMMData data = new SatelliteOMMData() 
                {
                    name = elements[0],
                    id = elements[1],
                    epoch = DateTime.Parse(
                        elements[2],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal |
                        DateTimeStyles.AdjustToUniversal
                    ),
                    meanMotion = float.Parse(elements[3], NumberStyles.Float, CultureInfo.InvariantCulture),
                    eccentricity = float.Parse(elements[4], NumberStyles.Float, CultureInfo.InvariantCulture),
                    inclination = float.Parse(elements[5], NumberStyles.Float, CultureInfo.InvariantCulture),
                    longitudeAscendingNode = float.Parse(elements[6], NumberStyles.Float, CultureInfo.InvariantCulture),
                    argumentOfPericenter = float.Parse(elements[7], NumberStyles.Float, CultureInfo.InvariantCulture),
                    meanAnomaly = float.Parse(elements[8], NumberStyles.Float, CultureInfo.InvariantCulture),
                    ephemerisType = int.Parse(elements[9]),
                    classType = elements[10][0],
                    noradID = int.Parse(elements[11]),
                    elementSetNumber = int.Parse(elements[12]),
                    revolutionAtEpoch = int.Parse(elements[13]),
                    BStar = float.Parse(elements[14], NumberStyles.Float, CultureInfo.InvariantCulture),
                    meanMotionDerivative = float.Parse(elements[15], NumberStyles.Float, CultureInfo.InvariantCulture),
                    meanMotionDoubleDerivative = float.Parse(elements[16], NumberStyles.Float, CultureInfo.InvariantCulture)
                };
                orbitDataList.Add(data);
            }
            catch(IndexOutOfRangeException)
            {
                Debug.Log("Something's up with orbit data. Index out of range, i:" + i.ToString());
                continue;
            }
        }

        return orbitDataList;
    }
    public List<SatelliteInfo> ReadSatelliteInfo(TextAsset infoFile)
    {
        List<SatelliteInfo> infoList = new List<SatelliteInfo>();
        
        string infoText = infoFile.text;
        string[] infoLines = infoText.Split(Environment.NewLine);

        for (int i = headerLines; i < infoLines.Length; i++)
        {
            string[] elements = infoLines[i].Split(delimiter);
            try
            {
                SatelliteInfo info = new SatelliteInfo() 
                {
                    name = elements[0],
                    countryUNRegistry = elements[1],
                    countryOperator = elements[2],
                    owner = elements[3],
                    users = elements[4],
                    purpose = elements[5],
                    orbitClass = elements[6],
                    orbitType = elements[7],
                    launchMass = int.Parse(elements[8]),
                    launchDate = DateTime.Parse(
                        elements[9],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal |
                        DateTimeStyles.AdjustToUniversal
                    ),
                    contractor = elements[10],
                    countryContractor = elements[11],
                    launchSite = elements[12],
                    launchVehicle = elements[13],
                    cosparID = elements[14],
                    noradID = int.Parse(elements[15]),
                };
                infoList.Add(info);
            }
            catch(IndexOutOfRangeException)
            {
                Debug.Log("Something's up with info. Index out of range, i:" + i.ToString());
                continue;
            }
            catch(FormatException)
            {
                Debug.Log("Something's up with info. Format exception, i:" + i.ToString() + " " + elements[8] + " " + elements[15]);
                continue;
            }
        }

        return infoList;
    }
}
