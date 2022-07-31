using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SpatialAwarenessHandler = Microsoft.MixedReality.Toolkit.SpatialAwareness.IMixedRealitySpatialAwarenessObservationHandler<Microsoft.MixedReality.Toolkit.SpatialAwareness.SpatialAwarenessMeshObject>;
public class PathSetter : MonoBehaviour, IMixedRealityPointerHandler
{
    public GameObject path;
    public List<GameObject> wayPoints;
    private GameObject parentGo;
    private void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    private void OnDisable()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }
    void Start()
    {
        wayPoints = new List<GameObject>();
        initPath();
    }
    private void initPath()
    {
        path = new GameObject("SpatialMeshPath");
        if(parentGo != null)
        {
            path.transform.parent = parentGo.transform;
        }
        else
        {
            path.transform.parent = GameObject.Find("MixedRealitySceneContent").transform;
        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        var result = eventData.Pointer.Result;
        var hitPosition = result.Details.Point;
        // Check if hitting spatial mapping layer
        if (result.CurrentPointerTarget?.layer == 31)
        {
            GameObject cornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cornerSphere.transform.parent = path.transform;
            cornerSphere.transform.position = hitPosition;
            cornerSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            cornerSphere.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);
            wayPoints.Add(cornerSphere);
            if(wayPoints.Count > 1)
            {
                NavMeshHelper.DrawLine(wayPoints[^2].transform.position, wayPoints[^1].transform.position, path);
            }
        }
        else
        {
            Debug.Log("Hit surface with layer: " + result.CurrentPointerTarget?.layer.ToString());
        }
    }

    public Vector3[] GetWayPoints()
    {
        Vector3[] positions = new Vector3[wayPoints.Count];
    
        for(int i = 0; i < wayPoints.Count; i++)
        {
            positions[i] = wayPoints[i].transform.position;
        }
        return positions;
    }
    public void ResetPath(bool destroyPath)
    {
        if(destroyPath)
            Destroy(path);
        initPath();
        wayPoints = new List<GameObject>();
    }

    public void SetParent(GameObject p)
    {
        parentGo = p;
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
    }
}