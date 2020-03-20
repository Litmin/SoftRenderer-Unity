using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    private Vector3 position;
    private Vector2 uv;
    private Vector3 normal;

    public Vertex(Vector3 _position)
    {
        position = _position;
    }

    public Vertex(Vector3 _position, Vector2 _uv)
    {
        position = _position;
        uv = _uv;
    }

    public Vertex(Vector3 _position, Vector2 _uv, Vector3 _normal)
    {
        position = _position;
        uv = _uv;
        normal = _normal;
    }
}
