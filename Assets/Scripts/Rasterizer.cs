using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rasterizer
{
    private Matrix4x4 m_Model;
    private Matrix4x4 m_View;
    private Matrix4x4 m_Projection;

    private FrameBuffer m_FrameBuffer;

    private Dictionary<int, VertexBuffer> m_VertexBuffers;
    private Dictionary<int, IndexBuffer> m_IndexBuffers;

    public Rasterizer()
    {

    }

    #region Transform
    public void SetModel(Matrix4x4 model)
    {
        m_Model = model;
    }

    public void SetView(Matrix4x4 view)
    {
        m_View = view;
    }

    public void SetProjection(Matrix4x4 projection)
    {
        m_Projection = projection;
    }

    public void SetViewport()
    {

    }

    public Matrix4x4 Perspective(float near, float far, float fov, float aspect)
    {
        return Matrix4x4.identity;
    }

    public Matrix4x4 Orthographic(float near, float far, float height, float aspect)
    {
        return Matrix4x4.identity;
    }
    #endregion

    public void Clear(int mask, Color? clearColor = null, float depth = float.PositiveInfinity)
    {

    }



    public void DrawElements(PrimitiveType primitiveType)
    {

    }
}
