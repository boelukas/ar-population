using Microsoft.MixedReality.WorldLocking.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PersistanceController
{
    private string animationsDirPath;
    private static PersistanceController instance = null;
    public static PersistanceController GetInstance()
    {
        if (instance == null)
        {
            instance = new PersistanceController();
        }
        return instance;
    }

    private PersistanceController()
    {
        animationsDirPath = Path.Combine(Application.persistentDataPath, "animations");


    }

    public void StoreAnimation(GammaDataStructure animation)
    {
        FragmentId currentFragId = WorldLockingManager.GetInstance().FragmentManager.CurrentFragmentId;
        string animationsDir = GetFragementAnimationsDirPath(currentFragId);
        long newId = 0;
        try
        {
            newId = Directory.GetFiles(animationsDir).Length;
        }
        catch (DirectoryNotFoundException)
        {

        }
        WriteFile(Path.Combine(animationsDir, ""+ newId), JsonUtility.ToJson(animation));
    }
    public string[] LoadAnimations()
    {
        FragmentId currentFragId = WorldLockingManager.GetInstance().FragmentManager.CurrentFragmentId;
        string animationsDir = GetFragementAnimationsDirPath(currentFragId);
        string[] animationFileNames = null;
        try
        {
            animationFileNames = Directory.GetFiles(animationsDir);

        }
        catch (DirectoryNotFoundException)
        {
            return new string[0];
        }
        string[] animations = new string[animationFileNames.Length];
        Array.Sort(animationFileNames);
        for (int i = 0; i < animationFileNames.Length; i++)
        {
            animations[i] = ReadFile(animationFileNames[i]);
        }
        return animations;
    }
    public void DeleteAnimation(int id)
    {
        FragmentId currentFragId = WorldLockingManager.GetInstance().FragmentManager.CurrentFragmentId;
        string animationsDir = GetFragementAnimationsDirPath(currentFragId);
        string fileName = Path.Combine(animationsDir, id.ToString());
        DeleteFile(fileName);
    }
    private string GetFragementAnimationsDirPath(FragmentId fragmentId)
    {
        return Path.Combine(animationsDirPath, "" + fragmentId);
    }

    public string ReadFile(string path)
    {
        string content = "";

        try
        {
            StreamReader reader = new StreamReader(path);
            if (reader != null)
            {
                content = reader.ReadToEnd();
                   
            }
        }
        catch (Exception e)
        {
            Debug.Log("[PercistanceController] File Read Exception: " + e.Message);
        }
        
        return content;
    }
    public void WriteFile(string path, string content, bool append = false)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (TextWriter writer = new StreamWriter(path, append))
            {
                writer.Write(content);
            }
        }
        catch (Exception e)
        {
            Debug.Log("[PercistanceController] File Write Exception: " + e.Message);
        }
    }
    public void DeleteFile(string path)
    {
        if (File.Exists(path) && new FileInfo(path).Length > 0)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.Log("[PercistanceController] File Delete Exception: " + e.Message);
            }
        }
    }

    public bool HasValidWltFragmentId()
    {
        FragmentId currentFragId = WorldLockingManager.GetInstance().FragmentManager.CurrentFragmentId;
        return currentFragId != FragmentId.Invalid && currentFragId != FragmentId.Unknown;
    }
}
