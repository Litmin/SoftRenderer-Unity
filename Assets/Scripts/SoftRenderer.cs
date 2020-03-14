using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class SoftRenderer : MonoBehaviour
{
    private Camera m_Camera;
    private CommandBuffer m_Cmd;
    private RenderTexture m_RenderTexture;
    private Material m_BlitMaterial;
    private Mesh m_FullScreenTriangle;
    private void InitRenderer()
    {
        m_Camera = GetComponent<Camera>();

        m_Cmd = new CommandBuffer() { name = "RasterCmd" };
        m_Camera.AddCommandBuffer(CameraEvent.AfterEverything, m_Cmd);
        m_RenderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        m_BlitMaterial = new Material(Shader.Find("RasterUtility/Blit"));
        m_Cmd.SetGlobalTexture("_MainTex", m_RenderTexture);
        m_FullScreenTriangle = new Mesh();
        m_FullScreenTriangle.SetVertices();
        m_FullScreenTriangle.SetUVs();
    }

    private void Render()
    {
        
    }

    private void ReleaseRender()
    {

    }

    private void Start()
    {
        InitRenderer();
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
