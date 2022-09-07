using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;

public class AnimationController : MonoBehaviour
{
    // SMPLX objects
    public GameObject smplxMaleDefault;
    public GameObject smplxFemaleDefault;

    public GameObject walkPointObject;

    // SpatialMesh path properties
    public bool visualizespatialMeshPath = true;
    public Material spatialMeshMat;

    // GAMMA walking path properties
    public bool visualizeGammaWalkingPath = true;
    public bool pathVisibilitySwitchIsToggle = true;

    // Debug
    public bool usePredefindedGammaAnswer = false;
    public TextAsset debugJsonGammaResponse;
    public bool exportAnimationClipEditorOnly = true;

    // GAMMA server
    public GameObject serverSettingsGo;
    private ServerSettings serverSettings;
    private bool isWaitingForGammaResponse;

    // UX
    public DialogController dialogController;
    public ConfigController configController;

    // Private members
    private GameObject animations;
    private RequestHandler requestHandler;
    private System.Action<string> requestResponseCallback;
    private System.Action requestFailureCallback;
    private System.Action<DialogResult> destroyAllAnimationsDialogCallback;

    private IMixedRealitySpatialAwarenessMeshObserver meshObserver;
    private List<GameObject> humans;
    private List<float[]> betas;
    private List<GameObject> spatialMeshPaths;
    private List<GameObject> gammaPaths;
    private Vector3[] currentPath;
    private bool spatialMeshVisWasActive;
    private bool loadedSavedAnimations = false;
    private PersistanceController persistanceController;

    // Path Setter
    private PathSetter pathSetter;

    // Animation parameters
    private readonly int nFramesMp = 10;
    private readonly float deltaT = 1 / 30f;

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

    private float floorY = float.NaN;
    private float[,] femaleBetas = new float[,] {
        { -1.135557f, 0.4900467f, -1.571586f, 0.324353f, 0.6154498f, 1.360919f, -0.4199827f, 0.68612f, -0.5775192f, 0.09844136f},
        {-3.77f, 0.07f, 1.188608f, 3.63f, -4.37f, 2.73f, -3.92f, -1.99f, 5f, 1.536026f},
        { -0.05453706f, 5f, 0.3141453f, -1.396356f, -0.2505138f, 0.9503168f, 1.143413f, -1.807971f, -0.9851396f, -1.033214f}};
    private float[,] maleBetas = new float[,] {
        {-0.9842331f, -0.9684765f, -1.111603f, 1.859693f, -1.533256f, 0.8656253f, -0.542197f, -0.4216726f, 0.1279395f, 0.4362581f},
        {-2.02f, 1.44f, 0.6797122f, -1.945877f, -0.4878228f, 1.552606f, -0.9787962f, 0.04247403f, 1.987148f, 0.4292719f},
        { 1.652401f, 3.57f, 0.261369f, -1.349104f, 0.858787f, -1.169071f, 0.4518526f, -1.11841f, -0.1033189f, 1.916406f}};


    void Start()
    {
        requestHandler = gameObject.AddComponent<RequestHandler>();
        requestResponseCallback = new System.Action<string>(GammaSuccessCallback);
        requestFailureCallback = new System.Action(CleanUpAfterFailedRequest);
        destroyAllAnimationsDialogCallback = new System.Action<DialogResult>(DestroyAllAnimationsDialogCallback);
        humans = new List<GameObject>();
        betas = new List<float[]>();
        spatialMeshPaths = new List<GameObject>();
        gammaPaths = new List<GameObject>();
        animations = new GameObject("animations");
        pathSetter = gameObject.AddComponent<PathSetter>();
        pathSetter.SetParent(animations);
        dialogController = GameObject.Find("DialogController").GetComponent<DialogController>();
        serverSettings = serverSettingsGo.GetComponent<ServerSettings>();
        animations.transform.parent = GameObject.Find("MixedRealitySceneContent").transform;
        configController = GameObject.Find("ConfigController").GetComponent<ConfigController>();
        persistanceController = PersistanceController.GetInstance();

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
        ShowSpatialMesh();
    }

    void Update()
    {
        if (!loadedSavedAnimations && persistanceController.HasValidWltFragmentId())
        {
            LoadSavedAnimations();
            loadedSavedAnimations = true;
        }
    }

    public void CreateWalkingPathAnimation()
    {
        Debug.Log("[Animation Controller][GUI] Creating path walking animation.");

        if (usePredefindedGammaAnswer)
        {
            Debug.Log("[Animation Controller] Using predefinded debug Gamma response.");
            GammaDataStructure gamma = JsonUtility.FromJson<GammaDataStructure>(debugJsonGammaResponse.text);
            ImportAnimation(gamma);
            return;
        }

        if (isWaitingForGammaResponse)
        {
            Debug.Log("[Animation Controller] Waiting for previous path to finish computing.");
            dialogController.OpenDialog("Error", "Waiting for current path to finish computing.");
            return;
        }
        currentPath = pathSetter.GetWayPoints();
        if (currentPath.Length < 2)
        {
            Debug.Log("[Animation Controller] Current path not long enough. Path length: "+ currentPath.Length);
            dialogController.OpenDialog("Error", "Current path is not long enough. Add more way points.");
            return;
        }
        floorY = currentPath[0].y;
        spatialMeshPaths.Add(pathSetter.path);
        pathSetter.ResetPath(!visualizespatialMeshPath);
        
        string jsonPath = PathToJson(UnityPathToGamma(currentPath));
        Debug.Log("[Animation Controller] Sending request to GAMMA.");
        configController.ReadConfigFile();
        isWaitingForGammaResponse = true;
        ChangePathColor(spatialMeshPaths[^1], Color.blue);
        requestHandler.PostRequest(configController.config.gammaServer, jsonPath, requestResponseCallback, requestFailureCallback);
    }

    private void GammaSuccessCallback(string jsonString)
    {
        GammaDataStructure gamma = JsonUtility.FromJson<GammaDataStructure>(jsonString);
        SaveAnimation(gamma);
        ImportAnimation(gamma);

    }
    public static string PathToJson(Vector3[] pathCorners)
    {
        float[] points = new float[pathCorners.Length * 3];
        int j = 0;
        for (int i = 0; i < pathCorners.Length * 3; i += 3)
        {
            points[i] = pathCorners[j].x;
            points[i + 1] = pathCorners[j].y;
            points[i + 2] = pathCorners[j].z;
            j++;

        }
        return "[" + string.Join(", ", points) + "]";
    }
    private void CleanUpAfterFailedRequest()
    {
        GameObject lastPath = spatialMeshPaths[^1];
        spatialMeshPaths.Remove(lastPath);
        Destroy(lastPath);
        ResetPath();
        isWaitingForGammaResponse=false;
    }

    private void ImportAnimation(GammaDataStructure gamma)
    {
        Debug.Log("[Animation Controller] Importing Animation.");

        GameObject human;
        Animation anim;
        SMPLX.ModelType gender;
        if(gamma.motion[0].gender == "female")
        {
            gender = SMPLX.ModelType.Female;
        }
        else
        {
            gender = SMPLX.ModelType.Male;
        }
        int betaIndex = FindBetaShapeIndex(gamma.motion[0].betas.data, gender);
        if( betaIndex != -1)
        {
            Debug.Log("[Animation Controller] Importing pre-defined Body.");
            human = Instantiate(humans[betaIndex], animations.transform);
            anim = human.GetComponent<Animation>();
            anim.RemoveClip("pathWalking");
        }
        else
        {
            if (gamma.motion[0].gender == "female")
            {
                Debug.Log("[Animation Controller] Importing new female Body.");
                human = Instantiate(smplxFemaleDefault, animations.transform);
            }
            else
            {
                Debug.Log("[Animation Controller] Importing new male Body.");
                human = Instantiate(smplxMaleDefault, animations.transform);
            }
            anim = human.AddComponent<Animation>();
            anim.cullingType = AnimationCullingType.BasedOnRenderers;

            SMPLX smplx = human.AddComponent<SMPLX>();
            smplx.modelType = gender;
            smplx.SetHandPose(SMPLX.HandPose.Relaxed);
            SetBetas(gamma.motion[0].betas, smplx);
        }
        if (visualizeGammaWalkingPath)
        {
            Debug.Log("[Animation Controller] Visualizing Gamma walking path.");
            gammaPaths.Add(CreateWalkPoints(gamma.wpath, animations));
        }
        humans.Add(human);
        betas.Add(gamma.motion[0].betas.data);
        human.name = "human_" + humans.Count;
        

        InitAnimationCurves(human);

        AnimationClip clip = CreateAnimationClip(gamma.motion, human.GetComponent<SMPLX>(), human);


        anim.AddClip(clip, "pathWalking");
        anim.Play("pathWalking");
        Debug.Log("[Animation Controller] Playing animation.");
        isWaitingForGammaResponse = false;
        if (!pathVisibilitySwitchIsToggle)
        {
            spatialMeshPaths[^1].SetActive(false);
            gammaPaths[^1].SetActive(false);
        }
    }

    private void HideSpatialMesh()
    {
        if (Application.isEditor)
        {
            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
        }
        else
        {
            meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
        }
    }

    private void ShowSpatialMesh()
    {
        configController.ReadConfigFile();
        float extent = configController.config.spatialMeshObervationExtent;
        meshObserver.ObservationExtents = new Vector3(extent, extent, extent);
        meshObserver.UpdateInterval = configController.config.spatialMeshUpdateInterval;
        meshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;

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
        Debug.Log("[Animation Controller] Creating animation clip.");
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

                FloorAlignment(human);
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
            Debug.Log("[Animation Controller] Exported animation clip.");
        }
#endif
        return clip;
    }
    private void FloorAlignment(GameObject human)
    {
        if (usePredefindedGammaAnswer) return;
        GameObject rightFoot = GetChildGameObject(human, "right_foot");
        GameObject leftFoot = GetChildGameObject(human, "left_foot");
        Vector3 rightPos = rightFoot.transform.position;
        Vector3 leftPos = leftFoot.transform.position;
        Vector3 correction; 

        if (rightPos.y < leftPos.y)
        {
            correction = new Vector3(0, floorY - rightPos.y, 0);
        }
        else
        {
            correction = new Vector3(0, floorY - leftPos.y, 0);
        }
        human.transform.position += correction;

    }
    
    public GameObject CreateWalkPoints(ArrayWrapper wPath, GameObject parentGo)
    {
        GameObject walkingPath = new GameObject("GammaPath");
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
        Debug.Log("[Animation Controller] Animation clip created with " + nAminCurves + " animation curves.");
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
    private Vector3[] UnityPathToGamma(Vector3[] pathCorners)
    {
        Vector3[] res = new Vector3[pathCorners.Length];
        for (int i = 0; i < pathCorners.Length; i++)
        {
            res[i] = CoordinateHelper.ToGamma(pathCorners[i]);
        }
        return res;
    }
    private GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        //Author: Isaac Dart, June-13.
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }

    
    public void DestroyAnimation()
    {
        Debug.Log("[Animation Controller][GUI] Destroying animation.");
        if(humans.Count != 0)
        {
            int id = humans.Count - 1;
            GameObject human = humans.Last();

            float[] beta = betas.Last();
            if(FindBetaShapeIndex(beta, human.GetComponent<SMPLX>().modelType) == betas.Count - 1 && !IsZero(beta))
            {
                // Destroy cloned mesh if this is the last instance using it and if its not the default mesh from the prefab.
                Debug.Log("[Animation Controller] Destroying cloned shared mesh.");
                Destroy(human.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh);
            }

            humans.Remove(human);
            Destroy(human);
            betas.Remove(beta);
            
            if (visualizeGammaWalkingPath)
            {
                GameObject p = gammaPaths.Last();
                gammaPaths.Remove(p);
                Destroy(p);
            }
            if (visualizespatialMeshPath)
            {
                GameObject p = spatialMeshPaths.Last();
                spatialMeshPaths.Remove(p);
                Destroy(p);
            }
            persistanceController.DeleteAnimation(id);

        }
    }

    public void DestroyAllAnimations()
    {
        Debug.Log("[Animation Controller][GUI] Destroying all animations.");

        dialogController.OpenConfirmDialog("Warning", "Do you want to destroy all current animations?", destroyAllAnimationsDialogCallback);
    }
    private void DestroyAllAnimationsDialogCallback(DialogResult dialog)
    {
        if (dialog.Result == DialogButtonType.Yes)
        {
            int humansCount = humans.Count;
            for (int i = 0; i < humansCount; i++)
            {
                DestroyAnimation();
            }
        }
    }

    private void ChangePathColor(GameObject path, Color color)
    {
        for (int i = 0; i < path.transform.childCount; i++)
        {
            GameObject child = path.transform.GetChild(i).gameObject;
            child.GetComponent<Renderer>().material.color = color;
        }
    }
    public void SetExportedMeshVisibility(GameObject switchGo)
    {
        bool isToggled = switchGo.GetComponent<Interactable>().IsToggled;
        if (isToggled)
        {
            Debug.Log("[Animation Controller][GUI] Showing spatial mesh.");
            ShowSpatialMesh();
        }
        else
        {
            Debug.Log("[Animation Controller][GUI] Hiding spatial mesh.");
            HideSpatialMesh();           
        }
    }

    public void SetPathVisibility(GameObject switchGo)
    {
        pathVisibilitySwitchIsToggle = switchGo.GetComponent<Interactable>().IsToggled;

        if (pathVisibilitySwitchIsToggle)
        {
            Debug.Log("[Animation Controller][GUI] Showing paths.");
        }
        else
        {
            Debug.Log("[Animation Controller][GUI] Hiding paths.");
        }

        if (visualizeGammaWalkingPath && pathVisibilitySwitchIsToggle)
        {
            foreach(GameObject p in gammaPaths)
            {
                p.SetActive(true);
            }
        }else if(visualizeGammaWalkingPath && !pathVisibilitySwitchIsToggle)
        {
            foreach (GameObject p in gammaPaths)
            {
                p.SetActive(false);
            }
        }
        if (visualizespatialMeshPath && pathVisibilitySwitchIsToggle)
        {
            foreach (GameObject p in spatialMeshPaths)
            {
                p.SetActive(true);
            }
        }
        else if (visualizespatialMeshPath && !pathVisibilitySwitchIsToggle)
        {
            foreach (GameObject p in spatialMeshPaths)
            {
                p.SetActive(false);
            }
        }
    }


    public void ResetPath()
    {
        Debug.Log("[Animation Controller][GUI] Resetting current path.");
        pathSetter.ResetPath(true);
    }
    public void RemoveLastWaypoint()
    {
        Debug.Log("[Animation Controller][GUI] Removing last waypoint");
        pathSetter.RemoveLastWaypoint();

    }
    public void EnableServerSettingsMenu()
    {
        Debug.Log("[Animation Controller][GUI] Opening server settings.");
        pathSetter.enabled = false;
        if (meshObserver.DisplayOption == SpatialAwarenessMeshDisplayOptions.Visible)
        {
            spatialMeshVisWasActive = true;
        }
        else
        {
            spatialMeshVisWasActive = false;
        }
        HideSpatialMesh();
        serverSettingsGo.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
        serverSettingsGo.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        serverSettingsGo.SetActive(true);
    }
    public void DisableServerSettingsMenu()
    {
        Debug.Log("[Animation Controller][GUI] Closing server settings.");
        serverSettingsGo.SetActive(false);

        pathSetter.enabled = true;
        if (spatialMeshVisWasActive)
        {
            ShowSpatialMesh();
        }
    }

    public void SaveAnimation(GammaDataStructure gamma)
    {
        string path = Path.Combine(Application.persistentDataPath, "animations", "1", "test_anim");
        ArrayWrapper p = new ArrayWrapper();

        float[] data = new float[currentPath.Length * 3];
        int idx = 0;
        for(int i = 0; i < currentPath.Length; i++)
        {
            data[idx] = currentPath[i].x;
            data[idx + 1] = currentPath[i].y;
            data[idx + 2] = currentPath[i].z;
            idx += 3;
        }
        p.shape = new int[] { currentPath.Length, 3 };
        p.data = data;
        gamma.spatial_mesh_path = p;
        persistanceController.StoreAnimation(gamma);
    }
    public void LoadSavedAnimations()
    {
        string[] savedAnimations = persistanceController.LoadAnimations();
        for(int i = 0; i < savedAnimations.Length; i++)
        {
            GammaDataStructure gamma = JsonUtility.FromJson<GammaDataStructure>(savedAnimations[i]);
            Vector3[] spatialMeshPath = new Vector3[gamma.spatial_mesh_path.shape[0]];
            for (int j = 0; j < spatialMeshPath.Length; j++)
            {
                if(j == 0)
                {
                    floorY = gamma.spatial_mesh_path.Get(j, 1);
                }

                spatialMeshPath[j] = new Vector3(gamma.spatial_mesh_path.Get(j, 0), gamma.spatial_mesh_path.Get(j, 1), gamma.spatial_mesh_path.Get(j, 2));
            }
            spatialMeshPaths.Add(pathSetter.VisualizePath(spatialMeshPath, animations));

            ImportAnimation(gamma);
        }
       
    }
    public void CloseApp()
    {
        Debug.Log("Quitting App");
        Application.Quit();
    }

    private int FindBetaShapeIndex(float[] newBetas, SMPLX.ModelType gender)
    {
        for (int i = 0; i < betas.Count(); i++)
        {
            for(int j = 0; j < 10; j++)
            {
                if (newBetas[j] != betas.ElementAt(i)[j])
                {
                    break;
                }
                if (newBetas[j] == betas.ElementAt(i)[j] && j == 9 && humans.ElementAt(i).GetComponent<SMPLX>().modelType == gender)
                {
                    return i;
                }
            }
        }
        return -1;
    }
    private bool IsZero(float[] arr)
    {
        bool isZero = true;
        for(int i = 0; i < arr.Length; i++)
        {
            isZero &= arr[i] == 0.0f;
        }
        return isZero;
    }
}
