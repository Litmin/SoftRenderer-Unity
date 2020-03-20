using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexBuffer
{
    private List<Vertex> m_Vertex;

    public VertexBuffer()
    {
        m_Vertex = new List<Vertex>();
    }

    public void AddVertex(Vertex vertex)
    {
        m_Vertex.Add(vertex);
    }

    public void AddVertices(IEnumerable<Vertex> vertices)
    {
        m_Vertex.AddRange(vertices);
    }
}
