using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RequestHandler : MonoBehaviour
{
    public void Request(string data)
    {
        try
        {
            string url = "http://localhost:8080";
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(jsonToSend);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.disposeDownloadHandlerOnDispose = true;
            req.disposeUploadHandlerOnDispose = true;

            req.SetRequestHeader("Content-Type", "application/json");
            StartCoroutine(onResponse(req));
        }
        catch (Exception e) { Debug.Log("ERROR : " + e.Message); }
    }
    private IEnumerator onResponse(UnityWebRequest req)
    {

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.ConnectionError)
            Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
        else
            Debug.Log("Success " + req.downloadHandler.text);
        //byte[] results = req.downloadHandler.data;
        Debug.Log("Second Success");
        req.Dispose();
        // Some code after success

    }
}
