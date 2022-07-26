using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordinateHelper
{
    private static Matrix4x4 gamaToUnityRotMat = new Matrix4x4(new Vector4(1, 0, 0, 0),  new Vector4(0, 0, 1, 0), new Vector4(0, 1, 0, 0),  new Vector4(0, 0, 0, 1));

    public static Matrix4x4 ToUnity(Matrix4x4 gammaM)
    {
        return gamaToUnityRotMat * gammaM;
    }
    public static Vector3 ToUnity(Vector3 gammaV)
    {
        return gamaToUnityRotMat.MultiplyVector(gammaV);
    }
    public static Vector3 ToGamma(Vector3 unityV)
    {
        return gamaToUnityRotMat.MultiplyVector(unityV);
    }
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }
    public static Quaternion RodriguesToQuaternion(Vector3 rod)
    {
        float angle_rad = rod.magnitude;
        float angle_deg = angle_rad * Mathf.Rad2Deg;
        Vector3 axis = rod.normalized;
        return Quaternion.AngleAxis(angle_deg, axis);
    }
    public static Matrix4x4 QuaternionToMatrix(Quaternion q)
    {
        Matrix4x4 m = new Matrix4x4();
        m.SetTRS(Vector3.zero, q, Vector3.one);
        return m;
    }
    public static Matrix4x4 RodriguesToMatrix(Vector3 rod)
    {
        Quaternion w = RodriguesToQuaternion(rod);
        return QuaternionToMatrix(w);
    }

}
