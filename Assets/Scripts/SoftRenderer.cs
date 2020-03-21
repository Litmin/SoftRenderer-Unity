﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class SoftRenderer : MonoBehaviour
{
    private Camera m_Camera;
    private CommandBuffer m_Cmd;
    private Material m_BlitMaterial;
    private Mesh m_FullScreenTriangle;
    private Mesh[] m_Meshes;

    private Rasterizer m_Rasterizer;

    private void InitRenderer()
    {
        m_Rasterizer = new Rasterizer();

        m_Camera = GetComponent<Camera>();
        
        // Generate Resources 
        m_BlitMaterial = new Material(Shader.Find("RasterUtility/Blit"));
        m_FullScreenTriangle = new Mesh();
        // Drawing a big triangle is faster than drawing a quad.
        m_FullScreenTriangle.SetVertices(new List<Vector3>() 
        {
            new Vector3(-1, -1, 0),
            new Vector3(-1, 3, 0),
            new Vector3(3, -1, 0)
        });
        m_FullScreenTriangle.SetUVs(0, new List<Vector2>()
        { 
            new Vector2(0,0),
            new Vector2(0,2),
            new Vector2(2,0)
        });
        m_FullScreenTriangle.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
        m_FullScreenTriangle.UploadMeshData(false);
        
        // Init CommandBuffer
        m_Cmd = new CommandBuffer() { name = "RasterCmd" };
        m_Cmd.SetGlobalTexture("_MainTex", m_FrameBuffer.GetPixels());
        m_Cmd.DrawMesh(m_FullScreenTriangle, Matrix4x4.identity, m_BlitMaterial);
        m_Camera.AddCommandBuffer(CameraEvent.AfterEverything, m_Cmd);
    }

    private void Render()
    {
        
    }

    private void ReleaseRender()
    {
        if (m_BlitMaterial != null)
            Destroy(m_BlitMaterial);
        if (m_FullScreenTriangle != null)
            Destroy(m_FullScreenTriangle);
        if(m_Cmd != null)
        {
            m_Camera.RemoveCommandBuffer(CameraEvent.AfterEverything, m_Cmd);
            m_Cmd.Dispose();
            m_Cmd = null;
        }
    }

    private void CollectMeshed()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        m_Meshes = new Mesh[meshFilters.Length];
        for(int i = 0;i < meshFilters.Length;i++)
        {
            m_Meshes[i] = meshFilters[i].mesh;
        }
    }

    private void Start()
    {
        InitRenderer();
        CollectMeshed();
    }

    private void Update()
    {
        Render();
    }

    private void OnDestroy()
    {
        ReleaseRender();
    }
}
