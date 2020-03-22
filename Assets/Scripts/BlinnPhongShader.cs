using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinnPhongShader
{
    public float ka;
    public float kd;
    public Vector3 normal;

    public Color Shade()
    {
        return Color.black;
    }
}
