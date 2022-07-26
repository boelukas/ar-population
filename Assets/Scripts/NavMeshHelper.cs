using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshHelper : MonoBehaviour
{
    public static bool SamplePath(Vector3 center, Vector3 boundingBoxSize, out NavMeshPath path)
    {
        path = null;
        NavMeshHit hit;

        Vector3 randLoation = new Vector3(Random.Range(-boundingBoxSize.x / 2f, boundingBoxSize.x / 2f), Random.Range(-boundingBoxSize.y / 2f, boundingBoxSize.y / 2f), Random.Range(-boundingBoxSize.z / 2f, boundingBoxSize.z / 2f));
        Vector3 randomStart = randLoation + center;

        float maxDistance = Mathf.Max(Mathf.Max(boundingBoxSize.x, boundingBoxSize.y), boundingBoxSize.z);

        bool foundStartPos = NavMesh.SamplePosition(randomStart, out hit, maxDistance, NavMesh.AllAreas);

        if (!foundStartPos)
        {
            return false;
        }
        Vector3 startPos = hit.position;



        Vector3 randomEnd = startPos + Random.insideUnitSphere * maxDistance / 2;
        randomEnd.y = randomStart.y;
        bool foundEndPos = NavMesh.SamplePosition(randomEnd, out hit, maxDistance, NavMesh.AllAreas);
        if (!foundEndPos)
        {
            return false;
        }
        Vector3 endPos = hit.position;

        NavMeshPath path1 = new NavMeshPath();
        bool pathFound = NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path1);
        path = path1;
        return pathFound;
    }
    public static void VisualizePath(NavMeshPath path, GameObject parentGo)
    {
        GameObject pathObject = new GameObject("Path");
        pathObject.transform.parent = parentGo.transform;
        Vector3[] corners = path.corners;
        int numCorners = corners.Length;
        if(numCorners == 0)
        {
            return;
        } else if( numCorners == 1){
            GameObject cornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cornerSphere.transform.parent = pathObject.transform;
            cornerSphere.transform.position = corners[0];
            cornerSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            cornerSphere.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);
        } else
        {
            GameObject start = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            start.transform.parent = pathObject.transform;
            start.transform.position = corners[0];
            start.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            start.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);

            for(int i = 1; i < numCorners - 1; i++)
            {
                GameObject cornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cornerSphere.transform.parent = pathObject.transform;
                cornerSphere.transform.position = corners[i];
                cornerSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                cornerSphere.GetComponent<Renderer>().material.color = new Color(1, 1, 0, 1);
                DrawLine(corners[i - 1], corners[i], pathObject);
            }


            GameObject end = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            end.transform.parent = pathObject.transform;
            end.transform.position = corners[numCorners - 1];
            end.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            end.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
            DrawLine(corners[numCorners - 2], corners[numCorners - 1], pathObject);

        }
    }

    public static void ExportPath(NavMeshPath path, string exportDir, string pathName)
    {
        File.WriteAllText(Path.Combine(exportDir, pathName), PathToJson(path));
    }

    public static string PathToJson(NavMeshPath path)
    {
        float[] points = new float[path.corners.Length * 3];
        int j = 0;
        for (int i = 0; i < path.corners.Length * 3; i += 3)
        {
            points[i] = path.corners[j].x;
            points[i + 1] = path.corners[j].y;
            points[i + 2] = path.corners[j].z;
            j++;

        }
        return "[" + string.Join(", ", points) + "]";
    }
    private static void DrawLine(Vector3 start, Vector3 end, GameObject parentGo)
    {
        GameObject line = new GameObject("PathConnection");
        line.transform.parent = parentGo.transform;
        var lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Standard"));
        lineRenderer.positionCount = 2;
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

}