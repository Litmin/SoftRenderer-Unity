using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexBuffer
{
    private List<int> m_Index;

    public IndexBuffer()
    {
        m_Index = new List<int>();
    }

    public void AddIndex(int index)
    {
        m_Index.Add(index);
    }

    public void AddIndices(IEnumerable<int> indices)
    {
        m_Index.AddRange(indices);
    }
}
