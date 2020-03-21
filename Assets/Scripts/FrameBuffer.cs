﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameBuffer
{
    public int m_Width;
    public int m_Height;
    private Texture2D m_ColorAttachment0;
    private Texture2D m_DepthBuffer;

    public FrameBuffer(int width, int height, int colorAttachmentCount = 1)
    {
        m_Width = width;
        m_Height = height;

        m_ColorAttachment0 = new Texture2D(width, height, TextureFormat.RGBA32, false);
        m_DepthBuffer = new Texture2D(width, height, TextureFormat.RFloat, false);
    }

    public void Release()
    {
        if (m_ColorAttachment0 != null)
            Object.Destroy(m_ColorAttachment0);
        if (m_DepthBuffer != null)
            Object.Destroy(m_DepthBuffer);
    }

    public void SetColor(int x, int y, Color color)
    {
        m_ColorAttachment0.SetPixel(x, y, color);
    }

    public void SetDepth(int x, int y, float depth)
    {
        m_DepthBuffer.SetPixel(x, y, new Color(depth,0,0));
    }

    public Texture2D GetPixels()
    {
        return m_ColorAttachment0;
    }
}
