﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rasterizer
{
    private int width;
    private int height;
    private Matrix4x4 m_Model;
    private Matrix4x4 m_View;
    private Matrix4x4 m_Projection;

    private FrameBuffer m_FrameBuffer;

    private List<VertexBuffer> m_VertexBuffers = new List<VertexBuffer>();
    private List<IndexBuffer> m_IndexBuffers = new List<IndexBuffer>();
    private int m_CurVertexBufferHandle;
    private int m_CurIndexBufferHandle;

    // Render State
    private CullType m_CullType = CullType.Back;

    public Rasterizer(int width, int height)
    {
        this.width = width;
        this.height = height;
        m_FrameBuffer = new FrameBuffer(width, height);
    }

    #region Buffer
    public int GenVertexBuffer()
    {
        m_VertexBuffers.Add(new VertexBuffer());
        return m_VertexBuffers.Count - 1;
    }

    public void BindVertexBuffer(int handle)
    {
        m_CurVertexBufferHandle = handle;
    }

    public void UnBindVertexBuffer()
    {
        m_CurVertexBufferHandle = -1;
    }

    public void SetVertexBufferData(IEnumerable<Vertex> vertices)
    {
        if(m_CurVertexBufferHandle >= 0 && m_CurVertexBufferHandle < m_VertexBuffers.Count)
            m_VertexBuffers[m_CurIndexBufferHandle].AddVertices(vertices);
    }

    public int GenIndexBuffer()
    {
        m_IndexBuffers.Add(new IndexBuffer());
        return m_IndexBuffers.Count - 1;
    }

    public void BindIndexBuffer(int handle)
    {
        m_CurIndexBufferHandle = handle;
    }

    public void UnBindIndexBuffer()
    {
        m_CurIndexBufferHandle = -1;
    }

    public void SetIndexBufferData(IEnumerable<int> indices)
    {
        if (m_CurIndexBufferHandle >= 0 && m_CurIndexBufferHandle < m_IndexBuffers.Count)
            m_IndexBuffers[m_CurIndexBufferHandle].AddIndices(indices);
    }
    #endregion

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

    public Matrix4x4 Perspective(float near, float far, float fov, float aspect)
    {
        float height = 2 * near * Mathf.Tan(Mathf.Deg2Rad * (fov / 2));
        float width = aspect * height;
        near = -near;
        far = -far;

        Matrix4x4 perspectiveMatrix = new Matrix4x4(new Vector4(2 * near / width, 0, 0, 0),
                                                    new Vector4(0, 2 * near / height, 0, 0),
                                                    new Vector4(0, 0, (near + far) / (near - far), 1),
                                                    new Vector4(0, 0, -(2 * near * far) / (near - far), 0));
        return perspectiveMatrix;
    }

    public Matrix4x4 Orthographic(float near, float far, float height, float aspect)
    {
        float width = height * aspect;
        Matrix4x4 orthographicMatrix = new Matrix4x4(new Vector4(2f / width, 0, 0, 0),
                                                     new Vector4(0, 2f / height, 0, 0),
                                                     new Vector4(0, 0, 2f / far - near),
                                                     new Vector4(0, 0, 0, 1));
        return orthographicMatrix;
    }
    #endregion

    public Texture2D GetOutputTexture()
    {
        return m_FrameBuffer.GetOutputTexture();
    }

    public void Clear(ClearMask mask, Color? clearColor = null, float depth = float.PositiveInfinity)
    {
        Color realClearColor = clearColor == null ? Color.clear : clearColor.Value;

        for(int i = 0;i < m_FrameBuffer.m_Width;i++)
        {
            for(int j = 0;j < m_FrameBuffer.m_Height;j++)
            {
                if((mask & ClearMask.COLOR) > 0)
                {
                    m_FrameBuffer.SetColor(i, j, realClearColor);
                }
                if((mask & ClearMask.DEPTH) > 0)
                {
                    m_FrameBuffer.SetDepth(i, j, depth);
                }
            }
        }

        m_FrameBuffer.Apply();
    }

    public void DrawElements(PrimitiveType primitiveType)
    {
        UnityEngine.Assertions.Assert.IsTrue(m_CurVertexBufferHandle != -1, "No vertex buffer binding");

        switch(primitiveType)
        {
            case PrimitiveType.LINES:
                DrawWireFrame();
                break;
            case PrimitiveType.TRIANGLES:
                DrawTriangles();
                break;
        }

        m_FrameBuffer.Apply();
    }

    private void DrawWireFrame()
    {
        VertexBuffer curVertexBuffer = m_VertexBuffers[m_CurVertexBufferHandle];
        if (m_CurIndexBufferHandle == -1)
        {
            for (int i = 0; i < curVertexBuffer.Count(); i += 3)
            {
                Vertex v0 = curVertexBuffer[i + 0];
                Vertex v1 = curVertexBuffer[i + 1];
                Vertex v2 = curVertexBuffer[i + 2];

                WireFrameTriangle(v0, v1, v2);
            }
        }
        else
        {
            UnityEngine.Assertions.Assert.IsTrue((m_CurVertexBufferHandle >= 0) && (m_CurVertexBufferHandle < m_IndexBuffers.Count), "IndexBuffer out of Range");

            IndexBuffer curIndexBuffer = m_IndexBuffers[m_CurIndexBufferHandle];
            for (int i = 0; i < curIndexBuffer.Count(); i += 3)
            {
                Vertex v0 = curVertexBuffer[curIndexBuffer[i + 0]];
                Vertex v1 = curVertexBuffer[curIndexBuffer[i + 1]];
                Vertex v2 = curVertexBuffer[curIndexBuffer[i + 2]];

                WireFrameTriangle(v0, v1, v2);
            }
        }
    }

    private void WireFrameTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        // MVP,Unity中世界空间和观察空间是左手坐标系，这里转成右手
        Vector3 v0_NDC = m_View.MultiplyPoint(m_Model.MultiplyPoint(v0.position));
        Vector3 v1_NDC = m_View.MultiplyPoint(m_Model.MultiplyPoint(v1.position));
        Vector3 v2_NDC = m_View.MultiplyPoint(m_Model.MultiplyPoint(v2.position));
        v0_NDC.z = -v0_NDC.z;
        v1_NDC.z = -v1_NDC.z;
        v2_NDC.z = -v2_NDC.z;
        v0_NDC = m_Projection.MultiplyPoint(v0_NDC);
        v1_NDC = m_Projection.MultiplyPoint(v1_NDC);
        v2_NDC = m_Projection.MultiplyPoint(v2_NDC);


        // Clip
        if (v0_NDC.x < -1 || v0_NDC.x > 1 || v0_NDC.y < -1 || v0_NDC.y > 1
            || v1_NDC.x < -1 || v1_NDC.x > 1 || v1_NDC.y < -1 || v1_NDC.y > 1
            || v2_NDC.x < -1 || v2_NDC.x > 1 || v2_NDC.y < -1 || v2_NDC.y > 1)
            return;

        // Cull,Unity中三角形顺时针为正面
        if(m_CullType == CullType.Back)
        {
            Vector3 v0v1 = v1_NDC - v0_NDC;
            Vector3 v0v2 = v2_NDC - v0_NDC;

            if (Vector3.Cross(v0v1, v0v2).z > 0)
                return;
        }
        if(m_CullType == CullType.Front)
        {
            Vector3 v0v1 = v1_NDC - v0_NDC;
            Vector3 v0v2 = v2_NDC - v0_NDC;

            if (Vector3.Cross(v0v1, v0v2).z < 0)
                return;
        }

        // Viewport
        Vector2Int v0_screen = new Vector2Int((int)((v0_NDC.x + 1) / 2 * width), (int)((v0_NDC.y + 1) / 2 * height));
        Vector2Int v1_screen = new Vector2Int((int)((v1_NDC.x + 1) / 2 * width), (int)((v1_NDC.y + 1) / 2 * height));
        Vector2Int v2_screen = new Vector2Int((int)((v2_NDC.x + 1) / 2 * width), (int)((v2_NDC.y + 1) / 2 * height));


        DrawLine(v0_screen.x, v0_screen.y, v1_screen.x, v1_screen.y);
        DrawLine(v1_screen.x, v1_screen.y, v2_screen.x, v2_screen.y);
        DrawLine(v2_screen.x, v2_screen.y, v0_screen.x, v0_screen.y);
    }

    private void DrawTriangles()
    {
        VertexBuffer curVertexBuffer = m_VertexBuffers[m_CurVertexBufferHandle];
        if (m_CurIndexBufferHandle == -1)
        {
            for (int i = 0; i < curVertexBuffer.Count(); i += 3)
            {
                Vertex v0 = curVertexBuffer[i + 0];
                Vertex v1 = curVertexBuffer[i + 1];
                Vertex v2 = curVertexBuffer[i + 2];

                RasterTriangle(v0, v1, v2);
            }
        }
        else
        {
            UnityEngine.Assertions.Assert.IsTrue((m_CurVertexBufferHandle > 0)
                    && (m_CurVertexBufferHandle < m_IndexBuffers.Count), "IndexBuffer out of Range");

            IndexBuffer curIndexBuffer = m_IndexBuffers[m_CurIndexBufferHandle];
            for (int i = 0; i < curIndexBuffer.Count(); i += 3)
            {
                Vertex v0 = curVertexBuffer[curIndexBuffer[i + 0]];
                Vertex v1 = curVertexBuffer[curIndexBuffer[i + 1]];
                Vertex v2 = curVertexBuffer[curIndexBuffer[i + 2]];

                RasterTriangle(v0, v1, v2);
            }
        }
    }

    private void RasterTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        // MVP
        Vector3 v0_NDC = m_Projection.MultiplyPoint(m_View.MultiplyPoint(m_Model.MultiplyPoint(v0.position)));
        Vector3 v1_NDC = m_Projection.MultiplyPoint(m_View.MultiplyPoint(m_Model.MultiplyPoint(v1.position)));
        Vector3 v2_NDC = m_Projection.MultiplyPoint(m_View.MultiplyPoint(m_Model.MultiplyPoint(v2.position)));

        // Viewport
        Vector2Int v0_screen = new Vector2Int((int)((v0_NDC.x + 1) / 2 * width), (int)((v0_NDC.y + 1) / 2 * height));
        Vector2Int v1_screen = new Vector2Int((int)((v1_NDC.x + 1) / 2 * width), (int)((v1_NDC.y + 1) / 2 * height));
        Vector2Int v2_screen = new Vector2Int((int)((v2_NDC.x + 1) / 2 * width), (int)((v2_NDC.y + 1) / 2 * height));

        // Triagnle Bounding Box
        Vector2Int bboxMin = new Vector2Int(Mathf.Min(Mathf.Min(v0_screen.x, v1_screen.x), v2_screen.x), 
                                            Mathf.Min(Mathf.Min(v0_screen.y, v1_screen.y), v2_screen.y));
        Vector2Int bboxMax = new Vector2Int(Mathf.Max(Mathf.Max(v0_screen.x, v1_screen.x), v2_screen.x),
                                            Mathf.Max(Mathf.Max(v0_screen.y, v1_screen.y), v2_screen.y));

        // Edge Function
        for(int i = bboxMin.x;i < bboxMax.x;i++)
        {
            for(int j = bboxMin.y;j < bboxMax.y;j++)
            {
                if(IsInsideTriangle(i,j, v0_screen, v1_screen, v2_screen))
                {
                    
                }
            }
        }
    }

    private bool IsInsideTriangle(int x, int y, Vector2Int v0, Vector2Int v1, Vector2Int v2)
    {
        return true;
    }

    private void DrawLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2;

        do
        {
            m_FrameBuffer.SetColor(x0, y0, Color.white);
            int e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        } while (x0 != x1 || y0 != y1);
    }
}
