using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationImporter : MonoBehaviour
{
    private AnimationClip clip;
    public TextAsset textFile;
    private SMPLX smplx;

    AnimationCurve[][] positionCurves;
    AnimationCurve[][] rotationCurves;
    AnimationCurve[][] scaleCurves;

    string[] childernNames;
    string[] childrenPaths;
    Transform[] childrenTransforms;
    int nChildren;
    int nCurves = 3;


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

        //ImportAnimation();
        ReadGammExport();

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
        GammaDataStructure gamma = JsonUtility.FromJson<GammaDataStructure>(json);
        //float[][] betas = gamma.motion[0].betas.GetArray2D();
        //float[][][] transf_rotmat = gamma.motion[0].transf_rotmat.GetArray3D();
        //float[][][][] markers = gamma.motion[0].markers.GetArray4D();

        return gamma;
    }
    public void ImportAnimation()
    {
        


        float startTime = Time.time;
        smplx.SetBodyPose(SMPLX.BodyPose.T);
        TakeSnapshot(0);


        smplx.SetBodyPose(SMPLX.BodyPose.A);
        TakeSnapshot(0.1f);


        smplx.SetBodyPose(SMPLX.BodyPose.T);
        TakeSnapshot(0.2f);

        clip = StopRecording();

        AssetDatabase.CreateAsset(clip, "Assets/testclip2.anim");
        AssetDatabase.SaveAssets();

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
        Debug.Log(res); 

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
