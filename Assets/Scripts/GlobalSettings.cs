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

        Debug.Log("Start Printing meshes:");
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        // Loop through all known Meshes
        CombineInstance[] combine = new CombineInstance[observer.Meshes.Count];
        int i = 0;
        foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values)
        {
            Mesh mesh = meshObject.Filter.mesh;
            combine[i].mesh = meshObject.Filter.sharedMesh;
            combine[i].transform = meshObject.Filter.transform.localToWorldMatrix;

            Debug.Log("mesh");
            // Do something with the Mesh object
            i++;
        }
        GameObject go = new GameObject();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        go.AddComponent<MeshCollider>();
        go.GetComponent<MeshFilter>().mesh = new Mesh();
        go.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        go.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        go.SetActive(true);
        go.AddComponent<NavMeshSurface>();
        go.GetComponent<NavMeshSurface>().BuildNavMesh();

        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;

        for(int j = 0; j < 5; j++)
        {
            NavMeshPath sampPath;
            bool found = NavMeshHelper.SamplePath(go.GetComponent<MeshFilter>().mesh.bounds.center, go.GetComponent<MeshFilter>().mesh.bounds.size, out sampPath);
            Debug.Log("Found Path: " + found);
            if (found)
            {
                NavMeshHelper.VisualizePath(sampPath);
                NavMeshHelper.ExportPath(sampPath, "C:\\Users\\Lukas\\Projects\\ar-population\\Data\\UnityTrajectories", "test_traj_"+j+".json");
            }
        }


    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
