using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArrayWrapper
{
    public int[] shape;
    public float[] data;
    private float[][] array2d;
    private float[][][] array3d;
    private float[][][][] array4d;

    public float Get(int i)
    {
        return data[i];
    }
    public float Get(int i, int j)
    {
        return data[i * shape[0] + j]; ;
    }
    public float Get(int i, int j, int k)
    {
        return data[i * shape[0] + j * shape[1] + k];
    }
    public float Get(int i, int j, int k, int l)
    {
        return data[i * shape[0] + j * shape[1] + k * shape[2] + l];
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
    public float[][][][] GetArray4D()
    {

            array4d = new float[shape[0]][][][];
            for (int i = 0; i < shape[0]; i++)
            {
                array4d[i] = new float[shape[1]][][];
                for (int j = 0; j < shape[1]; j++)
                {
                    array4d[i][j] = new float[shape[2]][];
                    for (int k = 0; k < shape[2]; k++)
                    {
                        array4d[i][j][k] = new float[shape[3]];

                        for (int l = 0; l < shape[3]; l++)
                        {
                            array4d[i][j][k][l] = Get(i, j, k, l);
                        }
                    }
                }
            }
            return array4d;

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