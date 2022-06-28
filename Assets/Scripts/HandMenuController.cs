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
            //scanSelector.SetActive(true);
            foreach (GameObject scan in GlobalSettings.scans)
            {
                scan.SetActive(true);
            }
            foreach (GameObject human in GlobalSettings.humans)
            {
                human.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Stop Alligning");
            foreach (GameObject scan in GlobalSettings.scans)
            {
               scan.SetActive(false);
            }
            foreach (GameObject human in GlobalSettings.humans)
            {
                human.SetActive(true);
            }

        }


    }
    public void ToggleSpatialMeshObvserver()
    {
        if (gameObject.GetComponent<Interactable>().IsToggled)
        {
            CoreServices.SpatialAwarenessSystem.Disable();
        }
        else
        {
            CoreServices.SpatialAwarenessSystem.Enable();
        }
    }

    private void DeactivateSpatialMesh()
    {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
    }
    private void ActivateSpatialMesh()
    {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
    }
}
