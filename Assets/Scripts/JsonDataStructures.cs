using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class ArrayWrapper
{
    public int[] shape;
    public float[] data;
    private float[][] array2d;
    private float[][][] array3d;

    public float Get(int i)
    {
        Assert.AreEqual(1, shape.Length, "Array size mismatch.");
        return data[i];
    }
    public float Get(int i, int j)
    {
        Assert.AreEqual(2, shape.Length, "Array size mismatch.");

        return data[i * shape[1] + j];
    }
    public float Get(int i, int j, int k)
    {
        Assert.AreEqual(3, shape.Length, "Array size mismatch.");

        return data[i * shape[1] * shape[2] + j * shape[2] + k];
    }
    //public float Get(int i, int j, int k, int l)
    //{
    //    return data[i * shape[1] * shape[2] * shape[3] + j * shape[2] * shape[3] + k * shape[3] + l];
    //}
    public Vector3 GetVector3(int i)
    {
        Assert.AreEqual(2, shape.Length, "Array size mismatch.");
        return new Vector3(Get(i, 0), Get(i, 1), Get(i, 2));
    }
    public Vector3 GetVector3()
    {
        Assert.AreEqual(1, shape.Length, "Array size mismatch.");
        return new Vector3(Get( 0), Get( 1), Get( 2));
    }

    public Matrix4x4 GetTransfromMatrix()
    {
        Assert.AreEqual(2, shape.Length, "Array size mismatch.");

        Matrix4x4 matrix = new Matrix4x4();
        Vector4 row0 = new Vector4(Get(0,0), Get(0, 1), Get(0, 2), 0);
        Vector4 row1 = new Vector4(Get(1,0), Get(1, 1), Get(1, 2), 0);
        Vector4 row2 = new Vector4(Get(2, 0), Get(2, 1), Get(2, 2), 0);
        Vector4 row3 = new Vector4(0, 0, 0, 1);
        matrix.SetRow(0, row0);
        matrix.SetRow(1, row1);
        matrix.SetRow(2, row2);
        matrix.SetRow(3, row3);
        return matrix;
    }

    public static Vector3 ToY(Vector3 zUpVector)
    {
        return new Vector3(zUpVector.x, zUpVector.z, zUpVector.y);
    }

    //TODO
    public static Matrix4x4 ToY(Matrix4x4 zUpMatrix)
    {
        return zUpMatrix;
    }
    public float[] GetArray1D()
    {
       return data;
    }
    public float[][] GetArray2D()
    {
            array2d = new float[shape[0]][];
            for (int i = 0; i < shape[0]; i++)
            {
                array2d[i] = new float[shape[1]];
                for (int j = 0; j < shape[1]; j++)
                {
                    array2d[i][j] = Get(i, j);
                }
            }
            return array2d;

    }
    public float[][][] GetArray3D()
    {

            array3d = new float[shape[0]][][];
            for (int i = 0; i < shape[0]; i++)
            {
                array3d[i] = new float[shape[1]][];
                for (int j = 0; j < shape[1]; j++)
                {
                    array3d[i][j] = new float[shape[2]];
                    for (int k = 0; k < shape[2]; k++)
                    {
                        array3d[i][j][k] = Get(i, j, k);
                    }
                }
            }
            return array3d;
        

    }
}

[Serializable]
public class GammaDataStructure
{
    public Motion[] motion;
    public ArrayWrapper wpath;

}

[Serializable]
public class Motion
{
    public string gender;
    public ArrayWrapper betas;
    public ArrayWrapper transf_rotmat;
    public ArrayWrapper transf_transl;
    public string mp_type;
    public ArrayWrapper markers;
    public ArrayWrapper markers_proj;
    public ArrayWrapper pelvis_loc;
    public ArrayWrapper joints;
    public ArrayWrapper smplx_params;
    public ArrayWrapper mp_latent;
    public int timestamp;
    public WPathPosition curr_target_wpath;

}
[Serializable]
public class WPathPosition
{
    public int index;
    public float[] position;
}