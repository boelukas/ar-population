using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadHumansClickHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaceCube()
    {
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(2, 0, 0);
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        //SpatialMeshExporter.Save(observer, "C:\\Users\\Lukas\\Projects");
        Debug.Log("pressed");

        SpatialMeshExporter.Save("C:\\Data\\Users\\siwei.1995@163.com\\3D Objects\\");

        // Get the first Mesh Observer available, generally we have only one registered
        //var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        // Set to not visible
        //observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;

        // Set to visible and the Occlusion material
        //observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;

        // Loop through all known Meshes
        foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values)
        {
            Mesh mesh = meshObject.Filter.mesh;
            // Do something with the Mesh object
            SpatialMeshExporter.Save(observer, "C:\\Users\\Lukas\\Projects");

        }
    }
}
