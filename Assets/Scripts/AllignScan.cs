using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllignScan : MonoBehaviour
{
    public GameObject scanSelector;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Allign()
    {
        if(gameObject.GetComponent<Interactable>().IsToggled)
        {
            Debug.Log("Start Alligning");
            scanSelector.SetActive(true);
        }
        else
        {
            Debug.Log("Stop Alligning");
            foreach (GameObject scan in GameObject.FindGameObjectsWithTag("3DScan"))
            {
                Object.Destroy(scan);
            }

        }


    }
}
