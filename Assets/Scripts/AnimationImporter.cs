using System.Collections;
using System.Collections.Generic;
using System.IO;
//using UnityEditor;
using UnityEngine;
using System.Linq;

public class AnimationImporter : MonoBehaviour
{
    private AnimationClip clip;
    private Animation anim;
    private SMPLX smplx;

    public TextAsset textFile;
    
    
    public GameObject walkPointObject;
    private GameObject[] walkPoints;
    private GameObject path;
    AnimationCurve[][] positionCurves;
    AnimationCurve[][] rotationCurves;
    AnimationCurve[][] scaleCurves;

    string[] childernNames;
    string[] childrenPaths;
    Transform[] childrenTransforms;
    int nChildren;
    int nCurves = 3;


    // Animation parameters
    int nFramesMp = 10;
    float deltaT = 0.1f;
    int nSmplxBodyJoints = 21;
    string[] bodyJointNames = new string[] { "pelvis", "left_hip", "right_hip", "spine1", "left_knee", "right_knee", "spine2", "left_ankle", "right_ankle", "spine3", "left_foot", "right_foot", "neck", "left_collar", "right_collar", "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow", "left_wrist", "right_wrist", "jaw", "left_eye_smplhf", "right_eye_smplhf", "left_index1", "left_index2", "left_index3", "left_middle1", "left_middle2", "left_middle3", "left_pinky1", "left_pinky2", "left_pinky3", "left_ring1", "left_ring2", "left_ring3", "left_thumb1", "left_thumb2", "left_thumb3", "right_index1", "right_index2", "right_index3", "right_middle1", "right_middle2", "right_middle3", "right_pinky1", "right_pinky2", "right_pinky3", "right_ring1", "right_ring2", "right_ring3", "right_thumb1", "right_thumb2", "right_thumb3" };
    string[] ignoredJoints = new string[] { "neck", "head" };
    private void Start()
    {
        smplx = GetComponent<SMPLX>();
        childrenTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true);
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
            childrenPaths[i] = CalculateRelativePath(childrenTransforms[i], transform);
            for(int j = 0; j < nCurves; j++)
            {
                positionCurves[i][j] = new AnimationCurve();
                rotationCurves[i][j] = new AnimationCurve();
                scaleCurves[i][j] = new AnimationCurve();
            }
            rotationCurves[i][3] = new AnimationCurve();
        }

        ImportAnimation();
        anim = gameObject.AddComponent<Animation>();
        anim.AddClip(clip, "test");
        anim.Play("test");
        

    }

    public GammaDataStructure ReadGammExport()
    {
    //    string[] p = new string[] { Application.streamingAssetsPath, "GammaExports", "test.json" };
    //    string p2 = Application.streamingAssetsPath + "/GammaExports" + "/test.json";
    //    Debug.Log(Application.dataPath);
    //    string path = Path.Combine(p2);
    //    Debug.Log(path);
    //    TextAsset jsonFile = Resources.Load(path) as TextAsset;
        string json = textFile.text;
        //Debug.Log(json);
        GammaDataStructure gamma = JsonUtility.FromJson<GammaDataStructure>(json);
        //float[][] betas = gamma.motion[0].betas.GetArray2D();
        //float[][][] transf_rotmat = gamma.motion[0].transf_rotmat.GetArray3D();
        //float[][][][] markers = gamma.motion[0].markers.GetArray4D();

        return gamma;
    }
    public void ImportAnimation()
    {


        GammaDataStructure gamma = ReadGammExport();
        //CreateWalkPoints(gamma.wpath);

        
        SetBetas(gamma.motion[0].betas);
        CreateAnimationClip(gamma.motion);

        //float startTime = Time.time;
        //smplx.SetBodyPose(SMPLX.BodyPose.T);
        //TakeSnapshot(0);


        //smplx.SetBodyPose(SMPLX.BodyPose.A);
        //TakeSnapshot(0.1f);


        //smplx.SetBodyPose(SMPLX.BodyPose.T);
        //TakeSnapshot(0.2f);

        //clip = StopRecording();

        //AssetDatabase.CreateAsset(clip, "Assets/testclip2.anim");
        //AssetDatabase.SaveAssets();

    }
    public void CreateAnimationClip(Motion[] motionPrimitives)
    {
        int frame = 0;
        foreach (Motion motionPrimitive in motionPrimitives)
        {
            Matrix4x4 transfRotmat = ArrayWrapper.ToY(motionPrimitive.transf_rotmat.GetTransfromMatrix());
            Vector3 transfTransl = ArrayWrapper.ToY(motionPrimitive.transf_transl.GetVector3());
            //if(motionPrimitive.timestamp == 0)
            //{
            //   transform.localPosition = transfTransl;
            //}

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
            for(; i < nFramesMp; i++)
            {
                // Update Local Pose
                Vector3[] bodyPose = new Vector3[nSmplxBodyJoints];
                for(int j= 0; j < nSmplxBodyJoints; j++)
                {
                    bodyPose[j] = new Vector3(motionPrimitive.smplx_params.Get(i, 6 + j * 3), motionPrimitive.smplx_params.Get(i, 6 + j*3 + 1), motionPrimitive.smplx_params.Get(i, 6 + j *3 + 2));
                    Quaternion pose = SMPLX.QuatFromRodrigues(bodyPose[j].x, bodyPose[j].y, bodyPose[j].z);
                    if (!ignoredJoints.Any(bodyJointNames[j + 1].Contains))
                    {
                        smplx.SetLocalJointRotation(bodyJointNames[j + 1], pose);
                    }


                }
                smplx.UpdatePoseCorrectives();
                smplx.UpdateJointPositions(false);

                //Update Global Pose
                Vector3 transl = ArrayWrapper.ToY(motionPrimitive.pelvis_loc.GetVector3(i));
                Vector3 globalOrient = new Vector3(motionPrimitive.smplx_params.Get(i, 3), motionPrimitive.smplx_params.Get(i, 4), motionPrimitive.smplx_params.Get(i, 5));


                //transform.localPosition += transl;

                Quaternion q = QuaternionFromMatrix(transfRotmat);
                //transform.rotation = SMPLX.QuatFromRodrigues(globalOrient.x, globalOrient.y, globalOrient.z);


                TakeSnapshot(deltaT * frame);
                frame++;
            }
        }
        clip = StopRecording();
        //AssetDatabase.CreateAsset(clip, "Assets/testclip2.anim");
        //AssetDatabase.SaveAssets();
    }
    public Quaternion QuaternionFromMatrix(Matrix4x4 m) 
    { 
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); 
    }
    public void CreateWalkPoints(ArrayWrapper wPath)
    {
        path = new GameObject("Path");
        path.transform.parent = transform;
        walkPoints = new GameObject[wPath.shape[0]];
        for (int i = 0; i < wPath.shape[0]; i++)
        {
            Vector3 position = new Vector3(wPath.Get(i, 0), wPath.Get(i, 2), wPath.Get(i, 1));
            walkPoints[i] = Instantiate(walkPointObject, position, Quaternion.identity, path.transform);
            walkPoints[i].name = "WalkPoint_" + i;
        }
    }
    public void SetBetas(ArrayWrapper betas)
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
        for(int i = 0; i < nChildren; i++)
        {
            for(int j = 0; j < nCurves; j++)
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
        for (int i = 0; i < nChildren; i++)
        {
            string relPath = childrenPaths[i];
            clip.SetCurve(relPath, typeof(Transform), "localPosition.x", positionCurves[i][0]);
            clip.SetCurve(relPath, typeof(Transform), "localPosition.y", positionCurves[i][1]);
            clip.SetCurve(relPath, typeof(Transform), "localPosition.z", positionCurves[i][2]);
            clip.SetCurve(relPath, typeof(Transform), "localRotation.x", rotationCurves[i][0]);
            clip.SetCurve(relPath, typeof(Transform), "localRotation.y", rotationCurves[i][1]);
            clip.SetCurve(relPath, typeof(Transform), "localRotation.z", rotationCurves[i][2]);
            clip.SetCurve(relPath, typeof(Transform), "localRotation.w", rotationCurves[i][3]);
            clip.SetCurve(relPath, typeof(Transform), "localScale.x", scaleCurves[i][0]);
            clip.SetCurve(relPath, typeof(Transform), "localScale.y", scaleCurves[i][1]);
            clip.SetCurve(relPath, typeof(Transform), "localScale.z", scaleCurves[i][2]);
        }
        clip.wrapMode = WrapMode.Loop;
        return clip;
    }
}
