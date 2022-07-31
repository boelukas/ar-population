using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class RequestHandler : MonoBehaviour
{
    public DialogController dialogController;

    private void Start()
    {
        dialogController = GameObject.Find("DialogController").GetComponent<DialogController>();
    }
    public void Request(string data, Action<string> responseCallback, Action failureCallback)
    {
        try
        {
            string url = "http://217.22.132.16:8080";
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(jsonToSend);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.disposeDownloadHandlerOnDispose = true;
            req.disposeUploadHandlerOnDispose = true;

            req.SetRequestHeader("Content-Type", "application/json");
            StartCoroutine(onResponse(req, responseCallback, failureCallback));
        }
        catch (Exception e) { Debug.Log("ERROR : " + e.Message); }
    }
    private IEnumerator onResponse(UnityWebRequest req, Action<string> responseCallback, Action failureCallback)
    {

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            string responseText = req.downloadHandler.text;
            responseText = Regex.Unescape(responseText);
            responseText = responseText.Substring(1, responseText.Length - 2);
            Debug.Log("RequestHandler: Success, received response.");
            responseCallback(responseText);
        }
        else
        {
            Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
            dialogController.OpenDialog("Gamma Server Request Error", req.GetResponseHeader(""));
            failureCallback();

        }
        req.Dispose();
    }


    public IEnumerator GetRequest(string uri, System.Action<bool> requestResponseCallback)
    {
        //string uri = "http://217.22.132.16:8080";
        Debug.Log(uri);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            webRequest.timeout = 3;

            yield return webRequest.SendWebRequest();
      
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    requestResponseCallback(true);
                    break;
                default:
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                    requestResponseCallback(false);
                    break;

            }
        }
    }
    
}
