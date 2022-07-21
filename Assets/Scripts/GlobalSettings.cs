using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class GlobalSettings : MonoBehaviour
{
    public static GameObject[] scans;
    public static GameObject[] humans;
    private RequestHandler requestHander;

    void Start()
    {
        //requestHander = GetComponent<RequestHandler>();
        //requestHander.Request("[-1.689821, -1.027213, -1.733333, -1.733333, -1.027213, -1.666667, -2.066667, -1.010546, -1.666667, -2.4, -1.010546, -1.766666, -2.833333, -1.010546, -2]");
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

}
