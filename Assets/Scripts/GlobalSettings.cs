using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    public static GameObject[] scans;
    public static GameObject[] humans;

    void Start()
    {
        scans = GameObject.FindGameObjectsWithTag("3DScan");
        humans = GameObject.FindGameObjectsWithTag("Human");
        foreach (GameObject scan in scans)
        {
            scan.SetActive(false);
        }
        foreach (GameObject human in humans)
        {
            human.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
