using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinnPhongShader : MiaoShader
{
    // public property
    public float ka { get; set; }
    public float kd { get; set; }
    public float ks { get; set; }
    public Color lightColor { get; set; }
    public float lightIntensity { get; set; }
    public Texture2D albedoMap { get; set; }
    public Texture2D normalMap { get; set; }

    public Vector3 worldPos { get; set; }
    public Vector3 viewPos { get; set; }
    public Vector3 lightPos { get; set; }


    public override Color FragmentShade()
    {
        Vector3 lightDir = (worldPos - lightPos).normalized;
        Vector3 viewDir = (viewPos - worldPos).normalized;

        // Ambient
        Color ambient = Global.ambientColor * ka;

        Color diffuse;
        Color specular;

        Color albedoColor;
        if (albedoMap != null)
            albedoColor = Utility.tex2D(albedoMap, uv);
        else
            albedoColor = vertexColor;

        if(normalMap == null)
        {
            // Diffuse
            diffuse = kd * albedoColor * Vector3.Dot(normal, -lightDir);

            // Specular
            // 计算半程向量h
            Vector3 h = (-lightDir + viewDir).normalized;
            specular = Color.white * Mathf.Pow(Vector3.Dot(h, normal), ks);
        }
        else
        {
            Vector3 normal_NormalMap = Utility.ColorToVector3(Utility.tex2D(normalMap, uv));
            normal_NormalMap = Utility.UnpackNormal(normal_NormalMap);

            // 从切线空间变换到世界空间,使用切线和法线求出副切线
            Vector3 bitTangent = Vector3.Cross(normal, tangent).normalized;
            Matrix4x4 TBN = new Matrix4x4(new Vector4(tangent.x, tangent.y, tangent.z, 0),
                                          new Vector4(bitTangent.x, bitTangent.y, bitTangent.z, 0),
                                          new Vector4(normal.x, normal.y, normal.z, 0),
                                          new Vector4(0, 0, 0, 1));
            normal_NormalMap = TBN.MultiplyPoint(normal_NormalMap);

            // Diffuse
            diffuse = kd * albedoColor * Vector3.Dot(normal_NormalMap, -lightDir);

            // Specular
            // 计算半程向量h
            Vector3 h = (-lightDir + viewDir).normalized;
            specular = Color.white * Mathf.Pow(Vector3.Dot(h, normal_NormalMap), ks);
        }

        return ambient + diffuse + specular;
    }
}
