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
    private MiaoShader m_CurShader;
    private Light[] m_Lights;


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

    #region Shader
    public void SetShader(MiaoShader shader)
    {
        m_CurShader = shader;
    }

    public void SetLights(Light[] lights)
    {
        m_Lights = lights;
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

    // Note:Unity的向量是行矩阵，注意投影矩阵的排列
    public Matrix4x4 Perspective(float near, float far, float fov, float aspect)
    {
        // 右手坐标系的投影矩阵
        //float height = 2 * near * Mathf.Tan(Mathf.Deg2Rad * (fov / 2));
        //float width = aspect * height;
        //near = -near;
        //far = -far;

        //Matrix4x4 perspectiveMatrix = new Matrix4x4(new Vector4(2 * near / width, 0, 0, 0),
        //                                            new Vector4(0, 2 * near / height, 0, 0),
        //                                            new Vector4(0, 0, (near + far) / (near - far), 1),
        //                                            new Vector4(0, 0, -(2 * near * far) / (near - far), 0));

        // 因为Unity世界空间和观察空间都是左手坐标系，用左手坐标系越远深度值越大，比较方便，这里使用左手坐标系下推导出的投影矩阵
        float height = 2 * near * Mathf.Tan(Mathf.Deg2Rad * (fov / 2));
        float width = aspect * height;

        Matrix4x4 perspectiveMatrix = new Matrix4x4(new Vector4(2 * near / width, 0, 0, 0),
                                                    new Vector4(0, 2 * near / height, 0, 0),
                                                    new Vector4(0, 0, (near + far) / (far - near), 1),
                                                    new Vector4(0, 0, -(2 * near * far) / (far - near), 0));

        return perspectiveMatrix;
    }

    public Matrix4x4 Orthographic(float near, float far, float height, float aspect)
    {
        float width = height * aspect;
        Matrix4x4 orthographicMatrix = new Matrix4x4(new Vector4(2f / width, 0, 0, 0),
                                                     new Vector4(0, 2f / height, 0, 0),
                                                     new Vector4(0, 0, 2f / (far - near),0),
                                                     new Vector4(0, 0, -(far + near) / (far - near), 1));
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
        // MVP
        Vector3 v0_NDC = m_Projection.MultiplyPoint(m_View.MultiplyPoint(m_Model.MultiplyPoint(v0.position)));
        Vector3 v1_NDC = m_Projection.MultiplyPoint(m_View.MultiplyPoint(m_Model.MultiplyPoint(v1.position)));
        Vector3 v2_NDC = m_Projection.MultiplyPoint(m_View.MultiplyPoint(m_Model.MultiplyPoint(v2.position)));

        // Clip
        if (v0_NDC.x < -1 || v0_NDC.x > 1 || v0_NDC.y < -1 || v0_NDC.y > 1 || v0_NDC.z < -1 || v0_NDC.z > 1
         || v1_NDC.x < -1 || v1_NDC.x > 1 || v1_NDC.y < -1 || v1_NDC.y > 1 || v1_NDC.z < -1 || v1_NDC.z > 1
         || v2_NDC.x < -1 || v2_NDC.x > 1 || v2_NDC.y < -1 || v2_NDC.y > 1 || v2_NDC.z < -1 || v2_NDC.z > 1)
            return;

        // Cull,Unity中三角形顺时针为正面,向量叉乘的方向在右手坐标系中使用右手定则确定，在左手坐标系中使用左手定则确定。
        // 这里需要注意不管是在右手坐标系还是左手坐标系，叉乘向量的数值结果是一样的，只不过表示的绝对向量方向不同，Unity中是左手坐标系，遵循左手定则。
        if (m_CullType == CullType.Back)
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
            UnityEngine.Assertions.Assert.IsTrue((m_CurVertexBufferHandle >= 0) && (m_CurVertexBufferHandle < m_IndexBuffers.Count), "IndexBuffer out of Range");

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

    enum ClipPlane
    {
        Near,
        Far,
        Left,
        Right,
        Top,
        Bottom
    }

    private List<Vector4> ClipWithPlane(List<Vector4> vertices, ClipPlane clipPlane)
    {
        List<Vector4> clippedVertices = new List<Vector4>();

        for(int i = 0;i < vertices.Count;++i)
        {
            int startIndex = i;
            int endIndex = (i + 1 + vertices.Count) % vertices.Count;

            // 边的起点
            Vector4 startVertex = vertices[startIndex];
            // 边的终点
            Vector4 endVertex = vertices[endIndex];

            bool startVertexIn = false;
            bool endVertexIn = false;


            switch(clipPlane)
            {
                case ClipPlane.Near:
                    if (startVertex.z > -startVertex.w)
                        startVertexIn = true;
                    if (endVertex.z > -endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Far:
                    if (startVertex.z < startVertex.w)
                        startVertexIn = true;
                    if (endVertex.z < endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Left:
                    if (startVertex.x > -startVertex.w)
                        startVertexIn = true;
                    if (endVertex.x > -endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Right:
                    if (startVertex.x < startVertex.w)
                        startVertexIn = true;
                    if (endVertex.x < endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Top:
                    if (startVertex.y < startVertex.w)
                        startVertexIn = true;
                    if (endVertex.y < endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Bottom:
                    if (startVertex.y > -startVertex.w)
                        startVertexIn = true;
                    if (endVertex.y > -endVertex.w)
                        endVertexIn = true;
                    break;
            }

            // 如果这条边跟裁剪平面相交，就把交点添加到裁剪后的结果中
            if(startVertexIn != endVertexIn)
            {
                float t = 0;
                switch(clipPlane)
                {
                    case ClipPlane.Near:
                        // 交点在 w=-z 这个平面上，所以交点的w和z的相反数相等
                        t = (startVertex.w + startVertex.z) / (-(endVertex.z - startVertex.z) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Far:
                        t = (startVertex.w - startVertex.z) / ((endVertex.z - startVertex.z) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Left:
                        t = (startVertex.w + startVertex.x) / (-(endVertex.x - startVertex.x) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Right:
                        t = (startVertex.w - startVertex.x) / ((endVertex.x - startVertex.x) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Top:
                        t = (startVertex.w - startVertex.y) / ((endVertex.y - startVertex.y) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Bottom:
                        t = (startVertex.w + startVertex.y) / (-(endVertex.y - startVertex.y) - (endVertex.w - startVertex.w));
                        break;
                }

                Vector4 intersection = Vector4.Lerp(startVertex, endVertex, t);
                clippedVertices.Add(intersection);
            }

            // 如果这条边的终点在内侧，就把结果添加到裁剪后的结果中
            if(endVertexIn)
            {
                clippedVertices.Add(endVertex);
            }
        }

        return clippedVertices;
    }

    private void RasterTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        // 模型变换:记录世界空间的坐标，计算光照时使用
        Vector3 v0_world = m_Model.MultiplyPoint(v0.position);
        Vector3 v1_world = m_Model.MultiplyPoint(v1.position);
        Vector3 v2_world = m_Model.MultiplyPoint(v2.position);
        // View变换:记录观察空间中的深度，Unity中向量和矩阵相乘直接就变换成三个分量的向量了
        Vector4 v0_view = m_View.MultiplyPoint(v0_world);
        Vector4 v1_view = m_View.MultiplyPoint(v1_world);
        Vector4 v2_view = m_View.MultiplyPoint(v2_world);
        v0_view.w = v1_view.w = v2_view.w = 1.0f;

        Vector4 v0_Clip = m_Projection * v0_view;
        Vector4 v1_Clip = m_Projection * v1_view;
        Vector4 v2_Clip = m_Projection * v2_view;

        // Frustum Clip
        List<Vector4> vertices = new List<Vector4> { v0_Clip, v1_Clip, v2_Clip };
        List<Vector4> clippedVertices1 = ClipWithPlane(vertices, ClipPlane.Near);
        List<Vector4> clippedVertices2 = ClipWithPlane(clippedVertices1, ClipPlane.Far);
        List<Vector4> clippedVertices3 = ClipWithPlane(clippedVertices2, ClipPlane.Left);
        List<Vector4> clippedVertices4 = ClipWithPlane(clippedVertices3, ClipPlane.Right);
        List<Vector4> clippedVertices5 = ClipWithPlane(clippedVertices4, ClipPlane.Top);
        List<Vector4> clippedVertices6 = ClipWithPlane(clippedVertices5, ClipPlane.Bottom);

        if (clippedVertices6.Count < 3)
            return;

        // 组装三角形
        for(int iClippedVertex = 0; iClippedVertex < clippedVertices6.Count - 2; iClippedVertex++)
        {
            int index0 = 0;
            int index1 = iClippedVertex + 1;
            int index2 = iClippedVertex + 2;

            Vector4 tempV0_Clip = clippedVertices6[index0];
            Vector4 tempV1_Clip = clippedVertices6[index1];
            Vector4 tempV2_Clip = clippedVertices6[index2];

            // 透视除法
            Vector3 v0_NDC = new Vector3(tempV0_Clip.x / tempV0_Clip.w, tempV0_Clip.y / tempV0_Clip.w, tempV0_Clip.z / tempV0_Clip.w);
            Vector3 v1_NDC = new Vector3(tempV1_Clip.x / tempV1_Clip.w, tempV1_Clip.y / tempV1_Clip.w, tempV1_Clip.z / tempV1_Clip.w);
            Vector3 v2_NDC = new Vector3(tempV2_Clip.x / tempV2_Clip.w, tempV2_Clip.y / tempV2_Clip.w, tempV2_Clip.z / tempV2_Clip.w);


            // Cull,Unity中三角形顺时针为正面,向量叉乘的方向在右手坐标系中使用右手定则确定，在左手坐标系中使用左手定则确定。
            // 这里需要注意不管是在右手坐标系还是左手坐标系，叉乘向量的数值结果是一样的，只不过表示的绝对向量方向不同，Unity中是左手坐标系，遵循左手定则。
            if (m_CullType == CullType.Back)
            {
                Vector3 v0v1 = v1_NDC - v0_NDC;
                Vector3 v0v2 = v2_NDC - v0_NDC;

                if (Vector3.Cross(v0v1, v0v2).z > 0)
                    return;
            }
            if (m_CullType == CullType.Front)
            {
                Vector3 v0v1 = v1_NDC - v0_NDC;
                Vector3 v0v2 = v2_NDC - v0_NDC;

                if (Vector3.Cross(v0v1, v0v2).z < 0)
                    return;
            }

            // Viewport
            Vector3 v0_screen = new Vector3((v0_NDC.x + 1) / 2 * width, (v0_NDC.y + 1) / 2 * height, 0);
            Vector3 v1_screen = new Vector3((v1_NDC.x + 1) / 2 * width, (v1_NDC.y + 1) / 2 * height, 0);
            Vector3 v2_screen = new Vector3((v2_NDC.x + 1) / 2 * width, (v2_NDC.y + 1) / 2 * height, 0);

            // Triagnle Bounding Box
            Vector2Int bboxMin = new Vector2Int((int)Mathf.Min(Mathf.Min(v0_screen.x, v1_screen.x), v2_screen.x),
                                                (int)Mathf.Min(Mathf.Min(v0_screen.y, v1_screen.y), v2_screen.y));
            Vector2Int bboxMax = new Vector2Int((int)(Mathf.Max(Mathf.Max(v0_screen.x, v1_screen.x), v2_screen.x) + 0.5f),
                                                (int)(Mathf.Max(Mathf.Max(v0_screen.y, v1_screen.y), v2_screen.y) + 0.5f));

            for (int i = bboxMin.x; i < bboxMax.x; i++)
            {
                for (int j = bboxMin.y; j < bboxMax.y; j++)
                {
                    // Edge Function 
                    if (IsInsideTriangle(i + 0.5f, j + 0.5f, v0_screen, v1_screen, v2_screen))
                    {
                        // 计算重心坐标
                        Vector3 barycentricCoordinate = BarycentricCoordinate(i + 0.5f, j + 0.5f, v0_screen, v1_screen, v2_screen);

                        // 计算该像素在观察空间的深度值:观察空间中的z的倒数在屏幕空间是线性的，所以用重心坐标可以插值z的倒数，再进行转换求出该像素的观察空间中的深度
                        float z_view = 1.0f / (barycentricCoordinate.x / tempV0_Clip.w + barycentricCoordinate.y / tempV1_Clip.w + barycentricCoordinate.z / tempV2_Clip.w);
                        // 插值投影后的z，首先投影后的z除以观察空间的z，用重心坐标插值后，再乘该像素观察空间的z
                        float z_interpolated = z_view * (v0_NDC.z / tempV0_Clip.w * barycentricCoordinate.x +
                                                         v1_NDC.z / tempV1_Clip.w * barycentricCoordinate.y +
                                                         v2_NDC.z / tempV2_Clip.w * barycentricCoordinate.z);


                        // Early-Z :)
                        // 存储到Depth Buffer，从[-1,1]变换到[0,1]
                        float z01 = (z_interpolated + 1) / 2f;
                        if (z01 > m_FrameBuffer.GetDepth(i, j))
                            continue;
                        else
                            m_FrameBuffer.SetDepth(i, j, z01);

                        // TODO: 裁剪后的顶点属性还没有插值
                        // 插值顶点属性：颜色、uv、法线、副切线、世界坐标
                        Color color = z_view * (v0.color / v0_view.z * barycentricCoordinate.x +
                                                v1.color / v1_view.z * barycentricCoordinate.y +
                                                v2.color / v2_view.z * barycentricCoordinate.z);
                        Vector2 uv = z_view * (v0.uv / v0_view.z * barycentricCoordinate.x +
                                               v1.uv / v1_view.z * barycentricCoordinate.y +
                                               v2.uv / v2_view.z * barycentricCoordinate.z);
                        Vector3 normal = z_view * (v0.normal / v0_view.z * barycentricCoordinate.x +
                                                   v1.normal / v1_view.z * barycentricCoordinate.y +
                                                   v2.normal / v2_view.z * barycentricCoordinate.z);
                        Vector4 tangent = z_view * (v0.tangent / v0_view.z * barycentricCoordinate.x +
                                                    v1.tangent / v1_view.z * barycentricCoordinate.y +
                                                    v2.tangent / v2_view.z * barycentricCoordinate.z);
                        Vector3 worldPos = z_view * (v0_world / v0_view.z * barycentricCoordinate.x +
                                                     v1_world / v1_view.z * barycentricCoordinate.y +
                                                     v2_world / v2_view.z * barycentricCoordinate.z);

                        Color col = Color.black;
                        if (m_CurShader != null)
                        {
                            m_CurShader.modelMatrix = m_Model;

                            m_CurShader.vertexColor = color;
                            m_CurShader.uv = uv;
                            m_CurShader.normal = normal;
                            m_CurShader.tangent = tangent;

                            if (m_CurShader is BlinnPhongShader)
                            {
                                BlinnPhongShader blinnPhongShader = (BlinnPhongShader)m_CurShader;
                                blinnPhongShader.worldPos = worldPos;
                                foreach (var light in m_Lights)
                                {
                                    blinnPhongShader.lightColor = light.color;
                                    blinnPhongShader.lightIntensity = light.intensity;
                                    blinnPhongShader.lightPos = light.transform.position;

                                    col += blinnPhongShader.FragmentShade();
                                }
                            }

                        }

                        m_FrameBuffer.SetColor(i, j, col);
                    }
                }
            }
        }
    }

    // 使用叉乘，判断点都在三条边的右侧
    // 这里需要注意不管是在右手坐标系还是左手坐标系，叉乘向量的方向都是用右手定则来确定，也就是叉乘得到的向量是同一个向量，只不过在不同坐标系中的表示不同。
    private bool IsInsideTriangle(float x, float y, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 v0v1 = new Vector3(v1.x - v0.x, v1.y - v0.y, 0);
        Vector3 v1v2 = new Vector3(v2.x - v1.x, v2.y - v1.y, 0);
        Vector3 v2v0 = new Vector3(v0.x - v2.x, v0.y - v2.y, 0);


        Vector3 v0p = new Vector3(x - v0.x, y - v0.y, 0);
        Vector3 v1p = new Vector3(x - v1.x, y - v1.y, 0);
        Vector3 v2p = new Vector3(x - v2.x, y - v2.y, 0);

        if (Vector3.Cross(v0v1, v0p).z < 0 
            && Vector3.Cross(v1v2, v1p).z < 0
            && Vector3.Cross(v2v0, v2p).z < 0)
            return true;
        else
            return false;
    }

    private Vector3 BarycentricCoordinate(float x, float y, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 v0v1 = new Vector3(v1.x - v0.x, v1.y - v0.y, 0);
        Vector3 v1v2 = new Vector3(v2.x - v1.x, v2.y - v1.y, 0);
        Vector3 v2v0 = new Vector3(v0.x - v2.x, v0.y - v2.y, 0);


        Vector3 v0p = new Vector3(x - v0.x, y - v0.y, 0);
        Vector3 v1p = new Vector3(x - v1.x, y - v1.y, 0);
        Vector3 v2p = new Vector3(x - v2.x, y - v2.y, 0);

        // 因为v0v1v2,p的z坐标都是0，叉乘向量的x、y是0，直接取z当作模长
        float area_v1v2p = Mathf.Abs(Vector3.Cross(v1v2, v1p).z) / 2;
        float area_v2v0p = Mathf.Abs(Vector3.Cross(v2v0, v2p).z) / 2;
        float area_v0v1p = Mathf.Abs(Vector3.Cross(v0v1, v0p).z) / 2;
        float area_v0v1v2 = Mathf.Abs(Vector3.Cross(v0v1, v2v0).z) / 2;


        return new Vector3(area_v1v2p / area_v0v1v2, area_v2v0p / area_v0v1v2, area_v0v1p / area_v0v1v2);
    }

    // Bresenham's line algorithm
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
