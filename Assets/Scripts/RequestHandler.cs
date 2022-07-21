using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class RequestHandler : MonoBehaviour
{
    public void Request(string data, Action<string> responseCallback)
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
            StartCoroutine(onResponse(req, responseCallback));
        }
        catch (Exception e) { Debug.Log("ERROR : " + e.Message); }
    }
    private IEnumerator onResponse(UnityWebRequest req, Action<string> responseCallback)
    {

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
        }
        else
        {
            string responseText = req.downloadHandler.text;
            responseText = Regex.Unescape(responseText);
            responseText = responseText.Substring(1, responseText.Length - 2);
            Debug.Log("RequestHandler: Success, received response.");
            responseCallback(responseText);

        }
        //byte[] results = req.downloadHandler.data;
        Debug.Log("Second Success");
        req.Dispose();
        // Some code after success

    }
}
