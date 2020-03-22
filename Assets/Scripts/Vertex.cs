using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    public Vector3 position;
    public Vector2 uv;
    public Vector3 normal;
    public Color color;

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

    public Vertex(Vector3 _position, Vector2 _uv, Vector3 _normal, Color _color)
    {
        position = _position;
        uv = _uv;
        normal = _normal;
        color = _color;
    }
}
