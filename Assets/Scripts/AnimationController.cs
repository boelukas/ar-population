using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;

public class AnimationController : MonoBehaviour
{
    // SMPLX objects
    public GameObject smplxFemale;
    public GameObject smplxMale;
    public GameObject walkPointObject;

    // NavMesh path properties
    public bool visualizeNavMeshPath = true;
    public int minNavMeshPathLength = 3;
    public Material spatialMeshMat;
    // GAMMA walking path properties
    public bool visualizeGammaWalkingPath = true;

    //Debug
    public bool usePredefindedGammaAnswer = false;
    public TextAsset debugJsonGammaResponse;
    public bool exportAnimationClipEditorOnly = true;

    // Private members
    private GameObject sceneContent;
    private GameObject spatialMeshGo = null;
    private RequestHandler requestHandler;
    private System.Action<string> requestResponseCallback;
    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;
    private List<GameObject> humans;
    private List<GameObject> spatialMeshPaths;
    private List<GameObject> gammaPaths;

    // Editing start and end position
    private GameObject startPos;
    private GameObject endPos;
    private GameObject startPosButton = null;
    private GameObject endPosButton = null;

    //Animation parameters
    private readonly int nFramesMp = 10;
    private readonly float deltaT = 0.05f;

    private readonly int nSmplxBodyJoints = 21;
    private readonly string[] bodyJointNames = new string[] { "pelvis", "left_hip", "right_hip", "spine1", "left_knee", "right_knee", "spine2", "left_ankle", "right_ankle", "spine3", "left_foot", "right_foot", "neck", "left_collar", "right_collar", "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow", "left_wrist", "right_wrist", "jaw", "left_eye_smplhf", "right_eye_smplhf", "left_index1", "left_index2", "left_index3", "left_middle1", "left_middle2", "left_middle3", "left_pinky1", "left_pinky2", "left_pinky3", "left_ring1", "left_ring2", "left_ring3", "left_thumb1", "left_thumb2", "left_thumb3", "right_index1", "right_index2", "right_index3", "right_middle1", "right_middle2", "right_middle3", "right_pinky1", "right_pinky2", "right_pinky3", "right_ring1", "right_ring2", "right_ring3", "right_thumb1", "right_thumb2", "right_thumb3" };
    private readonly string[] ignoredJoints = new string[] { "neck", "head" };

    private readonly int nCurves = 3;
    private AnimationCurve[][] positionCurves;
    private AnimationCurve[][] rotationCurves;
    private AnimationCurve[][] scaleCurves;

    private string[] childernNames;
    private string[] childrenPaths;
    private Transform[] childrenTransforms;
    private int nChildren;


    void Start()
    {
        requestHandler = gameObject.AddComponent<RequestHandler>();
        requestResponseCallback = new System.Action<string>(ImportAnimation);
        humans = new List<GameObject>();
        spatialMeshPaths = new List<GameObject>();
        gammaPaths = new List<GameObject>();

        sceneContent = GameObject.FindGameObjectsWithTag("SceneContent")[0];

        
        var spatialAwarenessService = CoreServices.SpatialAwarenessSystem;
        var dataProviderAccess = spatialAwarenessService as IMixedRealityDataProviderAccess;
        var fakeMeshObserverName = "Spatial Object Mesh Observer";
        var hololensMeshObserverName = "OpenXR Spatial Mesh Observer";
        if (Application.isEditor)
        {
            meshObserver = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>(fakeMeshObserverName);
        }
        else
        {
            meshObserver = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>(hololensMeshObserverName);
        }


    }

    public void PingServer()
    {
        Debug.Log("Sending GET");
        StartCoroutine(requestHandler.GetRequest());
    }

    public void CreateWalkingPathAnimation()
    {
        Application.targetFrameRate = 60;
        if (spatialMeshGo == null)
        {
            BuildNavMeshOfSpatialMesh();
        }
        if (usePredefindedGammaAnswer)
        {
            ImportAnimation(debugJsonGammaResponse.text);
            return;
        }
        NavMeshPath samplePath = new NavMeshPath();
        bool pathFound = SampleNavMeshPath(out samplePath);
        if (!pathFound)
        {
            return;
        }
        if (visualizeNavMeshPath)
        {
            Debug.Log("Visualizing path");
            spatialMeshPaths.Add(NavMeshHelper.VisualizePath(samplePath, spatialMeshGo));
            Debug.Log("Visualizing path - done");

        }
        UnityPathToGamma(samplePath);
        string jsonPath = NavMeshHelper.PathToJson(samplePath);
        Debug.Log("Sending request to GAMMA.");
        requestHandler.Request(jsonPath, requestResponseCallback);
    }
    private void ImportAnimation(string jsonString)
    {
        Debug.Log("Importing Animation");

        GammaDataStructure gamma = JsonUtility.FromJson<GammaDataStructure>(jsonString);
        GameObject human;
        if (gamma.motion[0].gender == "female")
        {
            human = Instantiate(smplxFemale, spatialMeshGo.transform);
        }
        else
        {
            human = Instantiate(smplxMale, spatialMeshGo.transform);
        }
        if (visualizeGammaWalkingPath)
        {
            Debug.Log("Visualizing walking path");
            gammaPaths.Add(CreateWalkPoints(gamma.wpath, spatialMeshGo));
            Debug.Log("Visualizing walking path - done");

        }
        humans.Add(human);
        human.name = "human_" + humans.Count;
        SMPLX smplx = human.AddComponent<SMPLX>();
        if(gamma.motion[0].gender == "female")
        {
            smplx.modelType = SMPLX.ModelType.Female;
        }
        else
        {
            smplx.modelType = SMPLX.ModelType.Male;
        }
        smplx.SetHandPose(SMPLX.HandPose.Relaxed);
        //SetBetas(gamma.motion[0].betas, smplx);

        InitAnimationCurves(human);

        //visualizeAnimationTranslations(gamma.motion, spatialMeshGo);
        AnimationClip clip = CreateAnimationClip(gamma.motion, smplx, human);
        Debug.Log("Importing Animation - done");

        Animation anim = human.AddComponent<Animation>();
        anim.cullingType = AnimationCullingType.BasedOnRenderers;

        anim.AddClip(clip, "pathWalking");
        anim.Play("pathWalking");
        Debug.Log("Playing Animation");
    }

    private void BuildNavMeshOfSpatialMesh()
    {
        //var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        Debug.Log("Building Navigation Mesh.");
        var observer = meshObserver;

        // Loop through all known Meshes
        CombineInstance[] combine = new CombineInstance[observer.Meshes.Count];
        int i = 0;
        foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values)
        {
            Mesh mesh = meshObject.Filter.mesh;
            combine[i].mesh = meshObject.Filter.sharedMesh;
            combine[i].transform = meshObject.Filter.transform.localToWorldMatrix;
            i++;
        }
        spatialMeshGo = new GameObject("SpatialMesh");
        spatialMeshGo.transform.parent = sceneContent.transform;
        spatialMeshGo.AddComponent<MeshFilter>();
        spatialMeshGo.AddComponent<MeshRenderer>();
        spatialMeshGo.GetComponent<MeshRenderer>().material = spatialMeshMat;
        spatialMeshGo.AddComponent<MeshCollider>();
        spatialMeshGo.GetComponent<MeshFilter>().mesh = new Mesh();
        spatialMeshGo.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        spatialMeshGo.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        spatialMeshGo.SetActive(true);
        spatialMeshGo.AddComponent<NavMeshSurface>();
        spatialMeshGo.GetComponent<NavMeshSurface>().BuildNavMesh();

        // Deactivate spatial mesh
        if (Application.isEditor)
        {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
        }
        else
        {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
        }

        Debug.Log("Building Navigation Mesh - done");

    }

    private bool SampleNavMeshPath(out NavMeshPath path)
    {
        Debug.Log("Sample path.");
        NavMeshPath sampPath = new NavMeshPath();
        bool found = false;
        int maxTries = 50;
        if (startPos != null && endPos != null)
        {
            Debug.Log("All positions are set");
            found = NavMeshHelper.CreatePath(startPos.transform.localPosition, endPos.transform.localPosition, out sampPath);
        }
        else if(startPos != null)
        {
            Debug.Log("Start pos is set");
            int count = 0;
            while (!found && count < maxTries)
            {
                count++;
                found = NavMeshHelper.SamplePathFixedPoint(startPos.transform.localPosition, true, spatialMeshGo.GetComponent<MeshFilter>().mesh.bounds.size, out sampPath);
                float pathLength = PathLength(sampPath);
                if (pathLength < minNavMeshPathLength)
                {
                    found = false;
                }
            }
            if (found) {
                Debug.Log("Sampled path with length: " + PathLength(sampPath) + ", in " + count + " attempts.");
            }
            else
            {
                Debug.Log("No path found");
            }
        }
        else if(endPos != null)
        {
            Debug.Log("End pos is set");
            int count = 0;
            while (!found && count < maxTries)
            {
                count++;
                found = NavMeshHelper.SamplePathFixedPoint(endPos.transform.localPosition, false, spatialMeshGo.GetComponent<MeshFilter>().mesh.bounds.size, out sampPath);
                float pathLength = PathLength(sampPath);
                if (pathLength < minNavMeshPathLength)
                {
                    found = false;
                }
            }
            if (found)
            {
                Debug.Log("Sampled path with length: " + PathLength(sampPath) + ", in " + count + " attempts.");
            }
            else
            {
                Debug.Log("No path found");
            }
        }
        else
        {
            Debug.Log("No pos is set");
            int count = 0;
            while (!found && count < maxTries)
            {
                count++;
                found = NavMeshHelper.SamplePath(spatialMeshGo.GetComponent<MeshFilter>().mesh.bounds.center, spatialMeshGo.GetComponent<MeshFilter>().mesh.bounds.size, out sampPath);
                //float pathLength = PathLength(sampPath);
                if (found && PathLength(sampPath) < minNavMeshPathLength)
                {
                    found = false;
                }
            }
            if (found)
            {
                Debug.Log("Sampled path with length: " + PathLength(sampPath) + ", in " + count + " attempts.");
            }
            else
            {
                Debug.Log("No path found");
            }
        }
        if (startPosButton != null)
        {
            startPosButton.GetComponent<Interactable>().IsToggled = false;
        }
        if (endPosButton != null)
        {
            endPosButton.GetComponent<Interactable>().IsToggled = false;
        }
        if (startPos != null)
        {
            Destroy(startPos);
            startPos = null;
        }
        if (endPos != null)
        {
            Destroy(endPos);
            endPos = null;
        }

        if (!found)
        {
            Debug.Log("No path found");
        }

        
        Debug.Log("Sample path - done");
        path = sampPath;
        return found;
    }

    private float PathLength(NavMeshPath path)
    {
        float length = 0f;
        Vector3[] corners = path.corners;
        for (int i = 1; i < corners.Length; i++)
        {
            float segmentLength = (corners[i] - corners[i - 1]).magnitude;
            length += segmentLength;
        }
        return length;

    }
    private void InitAnimationCurves(GameObject human)
    {
        childrenTransforms = human.transform.GetComponentsInChildren<Transform>(true);
        nChildren = childrenTransforms.Length;

        positionCurves = new AnimationCurve[nChildren][];
        rotationCurves = new AnimationCurve[nChildren][];
        scaleCurves = new AnimationCurve[nChildren][];

        childernNames = new string[nChildren];
        childrenPaths = new string[nChildren];


        for (int i = 0; i < nChildren; i++)
        {
            positionCurves[i] = new AnimationCurve[nCurves];
            rotationCurves[i] = new AnimationCurve[nCurves + 1];
            scaleCurves[i] = new AnimationCurve[nCurves];

            childernNames[i] = childrenTransforms[i].name;
            childrenPaths[i] = CalculateRelativePath(childrenTransforms[i], human.transform);
            for (int j = 0; j < nCurves; j++)
            {
                positionCurves[i][j] = new AnimationCurve();
                rotationCurves[i][j] = new AnimationCurve();
                scaleCurves[i][j] = new AnimationCurve();
            }
            rotationCurves[i][3] = new AnimationCurve();
        }
    }

    public AnimationClip CreateAnimationClip(Motion[] motionPrimitives, SMPLX smplx, GameObject human)
    {
        int frame = 0;
        foreach (Motion motionPrimitive in motionPrimitives)
        {
            Matrix4x4 transfRotmat = ArrayWrapper.ToY(motionPrimitive.transf_rotmat.GetTransfromMatrix());
            Vector3 transfTransl = ArrayWrapper.ToY(motionPrimitive.transf_transl.GetVector3());

            int i = 0;
            string mpType = motionPrimitive.mp_type;
            switch (mpType)
            {
                case "1-frame":
                    i = 1;
                    break;
                case "2-frame":
                    i = 2;
                    break;
                case "start-frame":
                    i = 0;
                    break;
                default:
                    break;
            }
            for (; i < nFramesMp; i++)
            {
                // Update Local Pose
                Vector3[] bodyPose = new Vector3[nSmplxBodyJoints];
                for (int j = 0; j < nSmplxBodyJoints; j++)
                {
                    bodyPose[j] = new Vector3(motionPrimitive.smplx_params.Get(i, 6 + j * 3), motionPrimitive.smplx_params.Get(i, 6 + j * 3 + 1), motionPrimitive.smplx_params.Get(i, 6 + j * 3 + 2));
                    Quaternion pose = SMPLX.QuatFromRodrigues(bodyPose[j].x, bodyPose[j].y, bodyPose[j].z);
                    if (!ignoredJoints.Any(bodyJointNames[j + 1].Contains))
                    {
                        smplx.SetLocalJointRotation(bodyJointNames[j + 1], pose);
                    }
                }

                //Update Global Pose
                // Translation
                Vector3 transl = motionPrimitive.pelvis_loc.GetVector3(i);
                Vector3 translUnity = ArrayWrapper.ToY(transfRotmat.MultiplyPoint3x4(transl));
                GameObject pelvis = GetChildGameObject(human, "pelvis");
                Vector3 pelvisPos = pelvis.transform.localPosition;
                human.transform.localPosition = translUnity + transfTransl - pelvisPos;


                //Orientation
                Vector3 globalOrientRod = new Vector3(motionPrimitive.smplx_params.Get(i, 3), motionPrimitive.smplx_params.Get(i, 4), motionPrimitive.smplx_params.Get(i, 5));
                Matrix4x4 globalOrientMatrix = CoordinateHelper.RodriguesToMatrix(globalOrientRod);
                Quaternion finalRot = CoordinateHelper.QuaternionFromMatrix(CoordinateHelper.ToUnity(transfRotmat * globalOrientMatrix));
                human.transform.rotation = finalRot;


                //smplx.UpdatePoseCorrectives();
                //smplx.UpdateJointPositions(false);

                TakeSnapshot(deltaT * frame);
                frame++;
            }
        }
        AnimationClip clip = StopRecording();
#if UNITY_EDITOR
        if (Application.isEditor && exportAnimationClipEditorOnly)
        {
            AssetDatabase.CreateAsset(clip, "Assets/AnimationClips/testclip2.anim");
            AssetDatabase.SaveAssets();
            Debug.Log("Exported Animation Clip");
        }
#endif
        return clip;
    }



    public GameObject CreateWalkPoints(ArrayWrapper wPath, GameObject parentGo)
    {
        GameObject walkingPath = new GameObject("Path");
        walkingPath.transform.parent = parentGo.transform;
        GameObject[] walkPoints = new GameObject[wPath.shape[0]];
        for (int i = 0; i < wPath.shape[0]; i++)
        {
            Vector3 position = new Vector3(wPath.Get(i, 0), wPath.Get(i, 1), wPath.Get(i, 2));
            position = CoordinateHelper.ToUnity(position);
            walkPoints[i] = Instantiate(walkPointObject, position, Quaternion.identity, walkingPath.transform);
            walkPoints[i].name = "WalkPoint_" + i;
        }
        return walkingPath;
    }

    public void SetBetas(ArrayWrapper betas, SMPLX smplx)
    {
        smplx.betas = betas.data;
        smplx.SetBetaShapes();
    }
    public string CalculateRelativePath(Transform target, Transform root)
    {
        Transform current = target;
        string res = "";
        bool first = true;
        while (!current.Equals(root))
        {
            if (first)
            {
                res = current.name;
                first = false;
            }
            else
            {
                res = current.name + "/" + res;

            }
            current = current.parent;
        }
        return res;
    }
    public void TakeSnapshot(float startTime)
    {
        float sn_time = Time.time - startTime;
        for (int i = 0; i < nChildren; i++)
        {
            for (int j = 0; j < nCurves; j++)
            {
                positionCurves[i][j].AddKey(startTime, childrenTransforms[i].localPosition[j]);
                rotationCurves[i][j].AddKey(startTime, childrenTransforms[i].localRotation[j]);
                scaleCurves[i][j].AddKey(startTime, childrenTransforms[i].localScale[j]);
            }
            rotationCurves[i][3].AddKey(startTime, childrenTransforms[i].localRotation.w);

        }
    }

    public AnimationClip StopRecording()
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        int nAminCurves = 0;
        for (int i = 0; i < nChildren; i++)
        {
            string relPath = childrenPaths[i];
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localPosition.x", positionCurves[i][0]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localPosition.y", positionCurves[i][1]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localPosition.z", positionCurves[i][2]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localRotation.x", rotationCurves[i][0]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localRotation.y", rotationCurves[i][1]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localRotation.z", rotationCurves[i][2]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localRotation.w", rotationCurves[i][3]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localScale.x", scaleCurves[i][0]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localScale.y", scaleCurves[i][1]);
            nAminCurves += SetCurveFilterConstants(clip, relPath, "localScale.z", scaleCurves[i][2]);
        }
        clip.wrapMode = WrapMode.Loop;
        Debug.Log("Clip created with " + nAminCurves + " Animation Curves");
        return clip;
    }

    private int SetCurveFilterConstants(AnimationClip clip, string relativePath, string propertyName, AnimationCurve curve)
    {
        float threshold = 0.000001f;
        float startValue = curve.keys[0].value;
        float endValue = curve.keys[^1].value;
        if(Mathf.Abs(startValue - endValue) >= threshold)
        {
            clip.SetCurve(relativePath, typeof(Transform), propertyName, curve);
            return 1;
        }
        else
        {
            return 0;
        }
    }
    private void UnityPathToGamma(NavMeshPath path)
    {
        for (int i = 0; i < path.corners.Length; i++)
        {
            path.corners[i] = CoordinateHelper.ToGamma(path.corners[i]);
        }
    }
    private GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        //Author: Isaac Dart, June-13.
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }

    public void SetNavMeshStartPos(GameObject setStartPosButton)
    {
        startPosButton = setStartPosButton;
        bool toggled = setStartPosButton.GetComponent<Interactable>().IsToggled;
        if (toggled)
        {
            if(spatialMeshGo == null)
            {
                BuildNavMeshOfSpatialMesh();
            }
            Vector3 cameraPos = Camera.main.transform.position;
            cameraPos += new Vector3(0, -1.5f, 0);
            Vector3 meshPos = NavMeshHelper.ClosestPointOnMesh(cameraPos);

            startPos = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            startPos.name = "StartPosMarker";
            startPos.transform.parent = spatialMeshGo.transform;
            startPos.transform.position = meshPos;
            startPos.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            startPos.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);
        }
        else
        {
            Destroy(startPos);
            startPos = null;
        }

    }
    public void SetNavMeshEndPos(GameObject setEndPosButton)
    {
        endPosButton = setEndPosButton;
        bool toggled = setEndPosButton.GetComponent<Interactable>().IsToggled;
        if (toggled)
        {
            if (spatialMeshGo == null)
            {
                BuildNavMeshOfSpatialMesh();
            }
            Vector3 cameraPos = Camera.main.transform.position;
            cameraPos += new Vector3(0, -1.5f, 0);

            Vector3 meshPos = NavMeshHelper.ClosestPointOnMesh(cameraPos);

            endPos = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            endPos.name = "EndPosMarker";
            endPos.transform.parent = spatialMeshGo.transform;
            endPos.transform.position = meshPos;
            endPos.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            endPos.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
        }
        else
        {
            Destroy(endPos);
            endPos = null;
        }

    }
    public void DestroyAnimation()
    {
        if(humans.Count != 0)
        {
            GameObject human = humans.Last();
            humans.Remove(human);
            Destroy(human);
            if (visualizeGammaWalkingPath)
            {
                GameObject p = gammaPaths.Last();
                gammaPaths.Remove(p);
                Destroy(p);
            }
            if (visualizeNavMeshPath)
            {
                GameObject p = spatialMeshPaths.Last();
                spatialMeshPaths.Remove(p);
                Destroy(p);
            }
        }
    }

    public void SetExportedMeshVisibility(GameObject switchGo)
    {
        bool isToggled = switchGo.GetComponent<Interactable>().IsToggled;
        if (isToggled)
        {
            if (spatialMeshGo != null)
            {
                spatialMeshGo.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        else
        {
            if (spatialMeshGo != null)
            {
                spatialMeshGo.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void SetPathVisibility(GameObject switchGo)
    {
        bool isToggled = switchGo.GetComponent<Interactable>().IsToggled;

        if (visualizeGammaWalkingPath && isToggled)
        {
            foreach(GameObject p in gammaPaths)
            {
                p.SetActive(true);
            }
        }else if(visualizeGammaWalkingPath && !isToggled)
        {
            foreach (GameObject p in gammaPaths)
            {
                p.SetActive(false);
            }
        }
        if (visualizeNavMeshPath && isToggled)
        {
            foreach (GameObject p in spatialMeshPaths)
            {
                p.SetActive(true);
            }
        }
        else if (visualizeNavMeshPath && !isToggled)
        {
            foreach (GameObject p in spatialMeshPaths)
            {
                p.SetActive(false);
            }
        }
    }
}
