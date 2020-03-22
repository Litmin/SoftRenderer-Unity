using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexOutput
{

}

public abstract class MiaoShader<T> where T : VertexOutput
{
    public abstract T VertexShade();

    public abstract Color FragmentShade(T vertexInput);
}
