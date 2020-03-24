using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static Color tex2D(Texture2D texture, Vector2 uv)
    {
        return texture.GetPixel((int)(uv.x * texture.width), (int)(uv.y * texture.height));
    }

    public static Vector3 ColorToVector3(Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }

    public static Vector3 UnpackNormal(Vector3 normal)
    {
        return new Vector3(normal.x * 2 - 1, normal.y * 2 - 1, normal.z * 2 - 1);
    }
}
