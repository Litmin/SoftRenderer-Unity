using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinnPhongVertexOutput : VertexOutput
{

}

public class BlinnPhongShader : MiaoShader<BlinnPhongVertexOutput>
{
    public float ka;
    public float kd;
    public Vector3 normal;

    public override BlinnPhongVertexOutput VertexShade()
    {
        throw new System.NotImplementedException();
    }

    public override Color FragmentShade(BlinnPhongVertexOutput vertexInput)
    {
        throw new System.NotImplementedException();
    }
}
