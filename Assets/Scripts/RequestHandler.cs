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
    public void PostRequest(string url, string data, Action<string> responseCallback, Action failureCallback)
    {
        try
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(jsonToSend);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.disposeDownloadHandlerOnDispose = true;
            req.disposeUploadHandlerOnDispose = true;

            req.SetRequestHeader("Content-Type", "application/json");
            StartCoroutine(OnResponse(req, responseCallback, failureCallback));
        }
        catch (Exception e) { Debug.Log("[RequestHandler] ERROR : " + e.Message); }
    }
    private IEnumerator OnResponse(UnityWebRequest req, Action<string> responseCallback, Action failureCallback)
    {

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            string responseText = req.downloadHandler.text;
            responseText = Regex.Unescape(responseText);
            responseText = responseText.Substring(1, responseText.Length - 2);
            Debug.Log("[RequestHandler] Success, received response.");
            responseCallback(responseText);
        }
        else
        {
            Debug.Log("[RequestHandler] Network error has occured: " + req.GetResponseHeader(""));
            dialogController.OpenDialog("Gamma Server Request Error", req.GetResponseHeader(""));
            failureCallback();

        }
        req.Dispose();
    }


    public IEnumerator GetRequest(string uri, System.Action<bool> requestResponseCallback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.timeout = 3;

            yield return webRequest.SendWebRequest();
      
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log("[RequestHandler] Success, received: " + webRequest.downloadHandler.text);
                    requestResponseCallback(true);
                    break;
                default:
                    Debug.Log("[RequestHandler] Error, received: " + webRequest.error);
                    requestResponseCallback(false);
                    break;

            }
        }
    }
    
}
