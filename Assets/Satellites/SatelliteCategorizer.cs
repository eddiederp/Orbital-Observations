using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SatelliteCategorizer
{
    public static string[] categoryNames = {"Operator (Nation)", "Orbit Class", "Orbit Type", "Purpose", "Users", "Launch Site"};
    
    public static Material otherMat;
    public static Material noDataMat;

    static Dictionary<string, Material> countryMaterials;
    static Dictionary<string, Material> orbitClassMaterials;
    static Dictionary<string, Material> orbitTypeMaterials;
    static Dictionary<string, Material> purposeMaterials;
    static Dictionary<string, Material> usersMaterials;
    static Dictionary<string, Material> launchSiteMaterials;

    static Dictionary<string, SatelliteInfo> placeholderInfo = new Dictionary<string, SatelliteInfo>()
    {
        //starlink constellation
        {"STARLINK", new SatelliteInfo
                    {
                        owner = "SpaceX",
                        countryOperator = "USA",
                        orbitClass = "LEO",
                        orbitType = "Non-Polar Inclined",
                        launchSite = "Cape Canaveral", // IF TIME, FIX TO GET ACTUAL LAUNCH SITE, SOME ARE LAUNCHED FROM VANDENBERG
                        purpose = "Communications",
                        users = "Commercial"
                    }
        },
        //guowang constellation
        {"DIGUI", new SatelliteInfo
                    {
                        owner = "China SatNet",
                        countryOperator = "China",
                        orbitClass = "LEO",
                        orbitType = "Non-Polar Inclined",
                        launchSite = "Wenchang Space Center", // IF TIME, FIX TO GET ACTUAL LAUNCH SITE, SOME ARE LAUNCHED FROM OTHER CHINESE SITES
                        purpose = "Communications",
                        users = "Commercial"
                    }
        },
        //amazon leo constellation
        {"KUIPER", new SatelliteInfo
                    {
                        owner = "Amazon",
                        countryOperator = "USA",
                        orbitClass = "LEO",
                        orbitType = "Non-Polar Inclined",
                        launchSite = "Cape Canaveral",
                        purpose = "Communications",
                        users = "Commercial"
                    }
        },
        //eutelsat oneweb constellation
        {"ONEWEB", new SatelliteInfo
                    {
                        owner = "EutelSat OneWeb",
                        countryOperator = "France",
                        orbitClass = "LEO",
                        orbitType = "Non-Polar Inclined",
                        purpose = "Communications",
                        users = "Commercial"
                    }
        }
    };

    public static SatelliteInfo GetPlaceholderInfo(string name)
    {
        foreach (KeyValuePair<string, SatelliteInfo> pair in placeholderInfo)
        {
            if (name.Contains(pair.Key))
            {
                SatelliteInfo info = new SatelliteInfo(pair.Value)
                {
                    name = name
                };
                return info;
            }
        }
        return new SatelliteInfo()
        {
            name = ""
        };
    }

    public static void SetCategoryMats()
    {
        otherMat = new Material(Shader.Find("Custom/SphereShader")) {color = Color.gray8};
        noDataMat = new Material(Shader.Find("Custom/SphereShader")) {color = Color.gray3};
        countryMaterials = new Dictionary<string, Material>()
        {
            {"Multinational", new Material(Shader.Find("Custom/SphereShader")) {color = Color.white}},
            {"USA", new Material(Shader.Find("Custom/SphereShader")) {color = Color.cyan}},
            {"ESA", new Material(Shader.Find("Custom/SphereShader")) {color = Color.blue}},
            {"Russia", new Material(Shader.Find("Custom/SphereShader")) {color = Color.green}},
            {"China", new Material(Shader.Find("Custom/SphereShader")) {color = Color.red}},
            {"India", new Material(Shader.Find("Custom/SphereShader")) {color = Color.orange}},
            {"Canada", new Material(Shader.Find("Custom/SphereShader")) {color = Color.hotPink}},
            {"Japan", new Material(Shader.Find("Custom/SphereShader")) {color = Color.purple}},
            {"United Kingdom", new Material(Shader.Find("Custom/SphereShader")) {color = Color.pink}}
        };
        orbitClassMaterials = new Dictionary<string, Material>()
        {
            {"LEO", new Material(Shader.Find("Custom/SphereShader")) {color = Color.red}},
            {"MEO", new Material(Shader.Find("Custom/SphereShader")) {color = Color.yellow}},
            {"GEO", new Material(Shader.Find("Custom/SphereShader")) {color = Color.limeGreen}},
            {"Elliptical", new Material(Shader.Find("Custom/SphereShader")) {color = Color.cyan}}
        };
        orbitTypeMaterials = new Dictionary<string, Material>()
        {
            {"Sun-Synchronous", new Material(Shader.Find("Custom/SphereShader")) {color = Color.yellow}},
            {"Non-Polar Inclined", new Material(Shader.Find("Custom/SphereShader")) {color = Color.green}},
            {"Polar", new Material(Shader.Find("Custom/SphereShader")) {color = Color.cyan}},
            {"Molniya", new Material(Shader.Find("Custom/SphereShader")) {color = Color.red}},
            {"None", new Material(Shader.Find("Custom/SphereShader")) {color = Color.white}},
        };
        purposeMaterials = new Dictionary<string, Material>()
        {
            {"Communications", new Material(Shader.Find("Custom/SphereShader")) {color = Color.blue}},
            {"Earth Observation", new Material(Shader.Find("Custom/SphereShader")) {color = Color.green}},
            {"Earth Science", new Material(Shader.Find("Custom/SphereShader")) {color = Color.greenYellow}},
            {"Space Observation", new Material(Shader.Find("Custom/SphereShader")) {color = Color.purple}},
            {"Space Science", new Material(Shader.Find("Custom/SphereShader")) {color = Color.hotPink}},
            {"Technology Development", new Material(Shader.Find("Custom/SphereShader")) {color = Color.red}},
            {"Technology Demonstration", new Material(Shader.Find("Custom/SphereShader")) {color = Color.orange}},
            {"Navigation", new Material(Shader.Find("Custom/SphereShader")) {color = Color.yellow}},
            {"Surveillance", new Material(Shader.Find("Custom/SphereShader")) {color = Color.saddleBrown}},
            {"Meteorological", new Material(Shader.Find("Custom/SphereShader")) {color = Color.cyan}},
        };
        usersMaterials = new Dictionary<string, Material>()
        {
            {"Commercial", new Material(Shader.Find("Custom/SphereShader")) {color = Color.blue}},
            {"Military", new Material(Shader.Find("Custom/SphereShader")) {color = Color.red}},
            {"Government", new Material(Shader.Find("Custom/SphereShader")) {color = Color.green}},
            {"Civil", new Material(Shader.Find("Custom/SphereShader")) {color = Color.cyan}},
        };
        launchSiteMaterials = new Dictionary<string, Material>()
        {
            {"Cape Canaveral", new Material(Shader.Find("Custom/SphereShader")) {color = Color.cyan}},
            {"Vandenberg AFB", new Material(Shader.Find("Custom/SphereShader")) {color = Color.mediumSpringGreen}},
            {"Baikonur Cosmodrome", new Material(Shader.Find("Custom/SphereShader")) {color = Color.green}},
            {"Vostochny Cosmodrome", new Material(Shader.Find("Custom/SphereShader")) {color = Color.olive}},
            {"Plesetsk Cosmodrome", new Material(Shader.Find("Custom/SphereShader")) {color = Color.yellowGreen}},
            {"Guiana Space Center", new Material(Shader.Find("Custom/SphereShader")) {color = Color.blue}},
            {"Satish Dhawan Space Centre", new Material(Shader.Find("Custom/SphereShader")) {color = Color.orange}},
            {"Jiuquan Satellite Launch Center", new Material(Shader.Find("Custom/SphereShader")) {color = Color.red}},
            {"Taiyuan Launch Center", new Material(Shader.Find("Custom/SphereShader")) {color = Color.crimson}},
            {"Wenchang Space Center", new Material(Shader.Find("Custom/SphereShader")) {color = Color.tomato}},
            {"Xichang Satellite Launch Center", new Material(Shader.Find("Custom/SphereShader")) {color = Color.firebrick}},
            {"Tanegashima Space Center", new Material(Shader.Find("Custom/SphereShader")) {color = Color.purple}},
            {"Rocket Lab Launch Complex 1", new Material(Shader.Find("Custom/SphereShader")) {color = Color.yellow}},
        };
    }

    public static Material GetCategoryMat(string data, int category)
    {
        try
        {
            switch (category)
            {
                case 0:
                    return countryMaterials[data];
                case 1:
                    return orbitClassMaterials[data];
                case 2:
                    return orbitTypeMaterials[data];
                case 3:
                    return purposeMaterials[data];
                case 4:
                    return usersMaterials[data];
                case 5:
                    return launchSiteMaterials[data];
                default:
                    Debug.LogError("Category not handled!");
                    return null;
            }
        }
        catch (KeyNotFoundException)
        {
            return otherMat;
        }   
    }
    public static List<KeyValuePair<string, Material>> GetCategoryPairs(int category)
    {
        switch (category)
        {
            case 0:
                return countryMaterials.ToList();
            case 1:
                return orbitClassMaterials.ToList();
            case 2:
                return orbitTypeMaterials.ToList();
            case 3:
                return purposeMaterials.ToList();
            case 4:
                return usersMaterials.ToList();
            case 5:
                return launchSiteMaterials.ToList();
            default:
                Debug.LogError("Category not handled!");
                return null;
        }
    }

    public static Material GetSatelliteMaterial(SatelliteInfo info, int category)
    {
        string data = info.GetVariableFromCategoryEnum(category);
        if (data != null)
        {
            if (data.Contains('/'))
            {
                data = data.Split('/')[0]; //first string in list
            }

            return GetCategoryMat(data, category);
        }
        return noDataMat;
    } 
}

