using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ServerSettings : MonoBehaviour
{
    public GameObject inputField;
    private RequestHandler requestHandler;
    public GameObject pingResponseVisualizer;
    public bool connectionSuccessful = false;
    private System.Action<bool> requestResponseCallback;
    private ConfigController configController;

    void Awake()
    {
        requestHandler = gameObject.AddComponent<RequestHandler>();
        requestResponseCallback = new System.Action<bool>(SetSuccess);
        configController = GameObject.Find("ConfigController").GetComponent<ConfigController>();

    }
    private void OnEnable()
    {
        configController.ReadConfigFile();
        var inputFildScript = inputField.GetComponent<MRTKTMPInputField>();
        inputFildScript.text = configController.config.gammaServer;
        PingServer();
    }

    private void SetSuccess(bool success)
    {
        connectionSuccessful = success;
    }
    public void PingServer()
    {

        Debug.Log("[ServerSettings] Sending GET request.");
        StartCoroutine(requestHandler.GetRequest(configController.config.gammaServer, requestResponseCallback));
    }

    public void UpdateGammaServerName(GameObject inputField)
    {
        Debug.Log("[ServerSettings][GUI] Updating Gamma server name");

        configController.config.gammaServer = inputField.GetComponent<MRTKTMPInputField>().text;
        configController.WriteConfigFile();
        PingServer();
    }
}
