using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuController : MonoBehaviour
{
    public GameObject scanSelector;
    private GameObject[] scans;
    private GameObject[] humans;

    void Awake()
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

    // Start is called before the first frame update
    void Start()

    {


    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggleAllignMode()
    {
        if(gameObject.GetComponent<Interactable>().IsToggled)
        {
            Debug.Log("Start Alligning");
            //scanSelector.SetActive(true);
            foreach (GameObject scan in scans)
            {
                scan.SetActive(true);
            }
            foreach (GameObject human in humans)
            {
                human.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Stop Alligning");
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
}
