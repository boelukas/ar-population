using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public string logFileName = "log.txt";
    public long maxLogFileSize = (long)1e9;
    private string logFilePath;
    void Awake()
    {
        logFilePath = Path.Combine(Application.persistentDataPath, logFileName);
    }

    private void OnEnable()
    {
        if(System.IO.File.Exists(logFilePath) && new FileInfo(logFilePath).Length > maxLogFileSize)
        {
            Debug.Log("[Logger] Max log file size reached. Deleting log file.");
            File.Delete(logFilePath);
        }
        Application.logMessageReceived += Log;
        Debug.Log("[Logger] Application started");
    }
    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }
    public void Log(string logString, string stackTrace, LogType type)
    {
        try
        {
            using (TextWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine("["+System.DateTime.Now+"]["+type+"] "+logString);
                writer.WriteLine(stackTrace);
            }
        }
        catch (Exception e)
        {
            Debug.Log("[Logger] File Write Exception: " + e.Message);
        }
    }
}
