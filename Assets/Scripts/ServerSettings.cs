using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ServerSettings : MonoBehaviour
{
    public GameObject inputField;
    public string defaultGammaServerAddress = "http://217.22.132.16:8080/";
    public Config config;
    private string configFileName = "config.json";
    private RequestHandler requestHandler;
    public GameObject pingResponseVisualizer;
    public bool connectionSuccessful = false;
    private System.Action<bool> requestResponseCallback;


    void Awake()
    {
        requestHandler = gameObject.AddComponent<RequestHandler>();
        requestResponseCallback = new System.Action<bool>(SetSuccess);

    }
    private void OnEnable()
    {
        ReadConfigFile();
        var inputFildScript = inputField.GetComponent<MRTKTMPInputField>();
        inputFildScript.text = config.gammaServer;
        PingServer();
    }

    private void SetSuccess(bool success)
    {
        connectionSuccessful = success;
    }
    public void PingServer()
    {

        Debug.Log("[ServerSettings] Sending GET request.");
        StartCoroutine(requestHandler.GetRequest(config.gammaServer, requestResponseCallback));
    }

    public void ReadConfigFile()
    {
        string path = Path.Combine(Application.persistentDataPath, configFileName);
        if (System.IO.File.Exists(path) && new FileInfo(path).Length > 0)
        {
            try
            {
                StreamReader reader = new StreamReader(path);
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
            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    config = new Config();
                    config.gammaServer = defaultGammaServerAddress;
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
    private void WriteConfigFile()
    {
        string path = Path.Combine(Application.persistentDataPath, configFileName);
        try
        {
            using (TextWriter writer = File.CreateText(path))
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
    public void UpdateGammaServerName(GameObject inputField)
    {
        Debug.Log("[ServerSettings][GUI] Updating Gamma server name");

        config.gammaServer = inputField.GetComponent<MRTKTMPInputField>().text;
        WriteConfigFile();
        PingServer();
    }
}
