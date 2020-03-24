using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Unity中的向量是当成一行的矩阵
// 世界空间和观察空间是左手坐标系
// Unity中三角形方向是顺时针
[RequireComponent(typeof(Camera))]
public class SoftRenderer : MonoBehaviour
{
    struct BufferMeshMap
    {
        public int vertexBufferHandle;
        public int indexBufferHandle;
        public MeshFilter mesh;
        public ModelProperty modelProperty;
        public BufferMeshMap(int vertexBufferHandle, int indexBufferHandle, MeshFilter mesh, ModelProperty modelProperty)
        {
            this.vertexBufferHandle = vertexBufferHandle;
            this.indexBufferHandle = indexBufferHandle;
            this.mesh = mesh;
            this.modelProperty = modelProperty;
        }
    }

    private Camera m_Camera;
    private CommandBuffer m_Cmd;
    private Material m_BlitMaterial;
    private Mesh m_FullScreenTriangle;
    private MeshFilter[] m_Meshes;
    private Light[] m_Lights;

    private Rasterizer m_Rasterizer;
    private List<BufferMeshMap> m_Models;
    private BlinnPhongShader m_BlinnPhongShader;

    private void InitRenderer()
    {
        m_Rasterizer = new Rasterizer(Screen.width, Screen.height);
        m_Models = new List<BufferMeshMap>();

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
        m_Cmd.SetGlobalTexture("_MainTex", m_Rasterizer.GetOutputTexture());
        m_Cmd.DrawMesh(m_FullScreenTriangle, Matrix4x4.identity, m_BlitMaterial);
        m_Camera.AddCommandBuffer(CameraEvent.AfterEverything, m_Cmd);
    }

    private void LoadMesh()
    {
        foreach(var meshFilter in m_Meshes)
        {
            Mesh mesh = meshFilter.mesh;
            ModelProperty property = meshFilter.GetComponent<ModelProperty>();

            int VBO = m_Rasterizer.GenVertexBuffer();
            int IBO = m_Rasterizer.GenIndexBuffer();
            m_Rasterizer.BindVertexBuffer(VBO);
            m_Rasterizer.BindIndexBuffer(IBO);

            Vector3[] vertices = mesh.vertices;
            int[] indices = mesh.triangles;
            Vector2[] uvs = mesh.uv;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;
            Color[] colors = mesh.colors;
            

            Vertex[] myVertices = new Vertex[vertices.Length];
            if(colors.Length > 0)
            {
                for(int i = 0;i < myVertices.Length;i++)
                {
                    myVertices[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], colors[i]);
                }
            }
            else
            {
                for (int i = 0; i < myVertices.Length; i++)
                {
                    myVertices[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], property.color);
                }
            }

            m_Rasterizer.SetVertexBufferData(myVertices);
            m_Rasterizer.SetIndexBufferData(indices);
            m_Rasterizer.UnBindVertexBuffer();
            m_Rasterizer.UnBindIndexBuffer();

            m_Models.Add(new BufferMeshMap(VBO, IBO, meshFilter, property));
        }
    }

    private void CreateShader()
    {
        m_BlinnPhongShader = new BlinnPhongShader();
    }

    private void Render()
    {
        // Clear
        m_Rasterizer.Clear(ClearMask.COLOR | ClearMask.DEPTH);

        // View 
        m_Rasterizer.SetView(m_Camera.transform.worldToLocalMatrix);

        // Projection
        if(m_Camera.orthographic)
        {
            Matrix4x4 orthographicProjection = m_Rasterizer.Orthographic(m_Camera.nearClipPlane, m_Camera.farClipPlane, m_Camera.orthographicSize * 2, m_Camera.aspect);
            m_Rasterizer.SetProjection(orthographicProjection);
        }
        else
        {
            Matrix4x4 perspectiveProjection = m_Rasterizer.Perspective(m_Camera.nearClipPlane, m_Camera.farClipPlane, m_Camera.fieldOfView, m_Camera.aspect);
            m_Rasterizer.SetProjection(perspectiveProjection);
        }

        Global.ambientColor = new Color(0.1f, 0.1f, 0.1f);
        // 使用Blinn Phong Shader
        m_Rasterizer.SetShader(m_BlinnPhongShader);
        m_BlinnPhongShader.viewPos = m_Camera.transform.position;

        foreach (var model in m_Models)
        {
            // Blinn Phong光照模型需要的参数
            m_BlinnPhongShader.albedoMap = model.modelProperty.albedo;
            m_BlinnPhongShader.normalMap = model.modelProperty.normal;
            m_BlinnPhongShader.ka = model.modelProperty.ka;
            m_BlinnPhongShader.kd = model.modelProperty.kd;
            m_BlinnPhongShader.ks = model.modelProperty.ks;

            m_Rasterizer.BindVertexBuffer(model.vertexBufferHandle);
            m_Rasterizer.BindIndexBuffer(model.indexBufferHandle);
            m_Rasterizer.SetModel(model.mesh.transform.localToWorldMatrix);
            m_Rasterizer.DrawElements(PrimitiveType.TRIANGLES);
        }
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

    private void CollectMeshes()
    {
        m_Meshes = GetComponentsInChildren<MeshFilter>();
    }

    private void CollectLights()
    {
        m_Lights = GetComponentsInChildren<Light>();
        m_Rasterizer.SetLights(m_Lights);
    }

    private void Start()
    {
        InitRenderer();
        CollectMeshes();
        CollectLights();
        LoadMesh();
        CreateShader();
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
