using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigController : MonoBehaviour
{
    public Config config;
    public string defaultGammaServerAddress = "http://217.22.132.16:8080/";
    public float defaultSpatialMeshExtent = 100;
    public float defaultSpatialMeshUpdateInterval = 2;
    public string configFileName = "config.json";
    private string configPath;

    void Start()
    {
        configPath = Path.Combine(Application.persistentDataPath, configFileName);
        ReadConfigFile();
    }
    void SetDefaultConfig()
    {
        config = new Config();
        config.gammaServer = defaultGammaServerAddress;
        config.spatialMeshObervationExtent = defaultSpatialMeshExtent;
        config.spatialMeshUpdateInterval = defaultSpatialMeshUpdateInterval;
        WriteConfigFile();
    }

    public void ReadConfigFile()
    {
        if (System.IO.File.Exists(configPath) && new FileInfo(configPath).Length > 0)
        {
            try
            {
                StreamReader reader = new StreamReader(configPath);
                if (reader != null)
                {
                    string content = reader.ReadToEnd();
                    config = JsonUtility.FromJson<Config>(content);
                }
            }
            catch (Exception e)
            {
                Debug.Log("[ServerSettings] File Read Exception: " + e.Message);
            }
        }
        else
        {
            SetDefaultConfig();
        }
    }

    public void WriteConfigFile()
    {
        try
        {
            using (TextWriter writer = File.CreateText(configPath))
            {
                string jsonConfig = JsonUtility.ToJson(config);

                writer.Write(jsonConfig);
            }
        }
        catch (Exception e)
        {
            Debug.Log("[ServerSettings] File Write Exception: " + e.Message);
        }
    }
}
