using UnityEngine;

[ExecuteInEditMode]
public class GridObject : MonoBehaviour
{
    [Range(2, 500)]
    public int m_rowPoints = 10;
    [Range(2, 500)]
    public int m_colPoints = 10;

    [Range(0f, 10f)]
    public float m_colSize = 10f;
    [Range(0f, 10f)]
    public float m_rowSize = 10f;

    [Range(0f, 10f)]
    public float m_height = 0f;
    [Range(0, 10)]
    public int m_octaves = 6;
    public float m_noiseSize = 1f;
    [Range(0.5f, 2f)]
    public float m_fractalNoiseAmount = 0.5f;
    [Range(1f, 3f)]
    public float m_fractalSize = 2f;
    public float m_minValue = 0f;
    public float m_maxValue = 1f;
    public int m_terraces = 5;

    private int m_rowPointsP = 0;
    private int m_colPointsP = 0;
    private float m_colSizeP = 0f;
    private float m_rowSizeP = 0f;
    private float m_heightP = 0f;
    private int m_octavesP = 0;
    private float m_noiseSizeP = 0f;
    private float m_fractalNoiseAmountP = 0f;
    private float m_fractalSizeP = 0f;
    private float m_minValueP = 0f;
    private float m_maxValueP = 0f;
    private int m_terracesP = 0;

    public bool m_dirty = true;

    private MeshFilter m_meshFilter = null;
    private MeshRenderer m_meshRenderer = null;

    private Vector3[] m_vertices = null;
    private Vector2[] m_uvs = null;
    private int[] m_triangles = null;

    private Mesh m_mesh = null;

    public Texture2D m_heightMap = null;
    public Material m_material = null;

    private int m_size = 512;
    
    private void CreateComponents()
    {
        if (m_meshFilter == null || m_meshRenderer == null)
        {
            m_meshFilter = gameObject.GetComponent<MeshFilter>();
            if (m_meshFilter == null)
            {
                m_meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (m_meshRenderer == null)
            {
                m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            if (m_material != null)
            {
                m_meshRenderer.sharedMaterial = m_material;
            }
        }
        
        if (m_meshRenderer != null)
        {
            if(m_meshRenderer.sharedMaterial != m_material)
            {
                m_meshRenderer.sharedMaterial = m_material;
            }
        }

        
        m_heightMap = new Texture2D(m_size, m_size, TextureFormat.RGB24, false);
        m_heightMap.wrapMode = TextureWrapMode.Mirror;
        m_material.mainTexture = m_heightMap;
    }

    private void UpdateVariables()
    {
        if (m_colPoints != m_colPointsP)
        {
            m_dirty = true;
        }

        if (m_rowPoints != m_rowPointsP)
        {
            m_dirty = true;
        }

        if(m_colSize != m_colSizeP)
        {
            m_dirty = true;
        }

        if (m_rowSize != m_rowSizeP)
        {
            m_dirty = true;
        }

        if(m_height != m_heightP)
        {
            m_dirty = true;
        }

        if(m_octaves != m_octavesP)
        {
            m_dirty = true;
        }

        if(m_fractalNoiseAmount != m_fractalNoiseAmountP)
        {
            m_dirty = true;
        }

        if (m_fractalSize != m_fractalSizeP)
        {
            m_dirty = true;
        }

        if(m_noiseSize != m_noiseSizeP)
        {
            m_dirty = true;
        }

        if(m_minValue != m_minValueP)
        {
            m_dirty = true;
        }

        if(m_maxValue != m_maxValueP)
        {
            m_dirty = true;
        }

        if (m_terraces != m_terracesP)
        {
            m_dirty = true;
        }

        m_colPointsP = m_colPoints;
        m_rowPointsP = m_rowPoints;
        m_colSizeP = m_colSize;
        m_rowSizeP = m_rowSize;
        m_heightP = m_height;
        m_octavesP = m_octaves;
        m_fractalNoiseAmountP = m_fractalNoiseAmount;
        m_fractalSizeP = m_fractalSize;
        m_noiseSizeP = m_noiseSize;
        m_minValueP = m_minValue;
        m_maxValueP = m_maxValue;
        m_terracesP = m_terraces;
    }

    private float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    private void UpdateHeightmap()
    {
        Color[] data = new Color[m_heightMap.width * m_heightMap.height];
        int ind = 0;
        for (int y = 0; y < m_heightMap.height; ++y)
        {
            for (int x = 0; x < m_heightMap.width; ++x)
            {
                ind = y * m_heightMap.width + x;
                float c = 0;
                float a = 0.5f;
                Vector2 st = new Vector2(
                    (float)x/(float)m_heightMap.width, 
                    (float)y/(float)m_heightMap.height) * m_noiseSize;
                float nA = Mathf.PerlinNoise(
                            st.x,
                            st.y);
                c = nA;
                for (int i = 0; i < m_octaves; ++i)
                {
                    st = st * m_fractalSize;
                    float nB = Mathf.PerlinNoise(
                        st.x,
                        st.y) * (a / (i + 2f*m_fractalNoiseAmount));

                    if (nB > 0.5f)
                    {
                        c *= nB;
                    }
                    else
                    {
                        if (c <= 1f || nB <= 1f)
                            c += nB - c * nB;
                        else
                            c = Mathf.Max(c, nB);
                    }
                }
                c = map(c, m_minValue, m_maxValue, 0f, 1f);
                float t = map(c, m_minValue, m_maxValue, 0f, (float)m_terraces);
                c = Mathf.Floor(t) / m_terraces;
                data[ind] = new Color(c, c, c);
            }
        }
        m_heightMap.SetPixels(data);
        m_heightMap.Apply();
    }

    private void Update()
    {
        UpdateVariables();
        if (m_dirty)
        {
            CreateComponents();
            UpdateHeightmap();
            GenerateGrid(m_colPoints, m_rowPoints);
            m_dirty = false;

        }
        if (m_material != null)
        {
            m_material.SetFloat("_Rows", m_rowPoints);
            m_material.SetFloat("_Cols", m_colPoints);
        }
    }

    private void GenerateGrid(int colPoints, int rowPoints)
    {
        int c = colPoints * rowPoints;
        m_vertices = new Vector3[c];
        m_triangles = new int[colPoints * rowPoints * 6];
        m_uvs = new Vector2[c];

        for (int y=0; y < rowPoints; ++y)
        {
            for(int x=0; x < colPoints; ++x)
            {
                int ind = y * colPoints + x;
                float fx = ((float)x / ((float)colPoints - 1));
                float fy = ((float)y / ((float)rowPoints - 1));
                Vector3 p = new Vector3(
                    fx * m_colSize,
                    m_heightMap.GetPixel(
                        Mathf.FloorToInt(fx * m_heightMap.width),
                        Mathf.FloorToInt(fy * m_heightMap.height)).r * m_height,
                    fy * m_rowSize);
                m_vertices[ind] = p;

                Vector2 t = new Vector2(
                    (float)x / ((float)colPoints - 2),
                    (float)y / ((float)rowPoints - 2));
                m_uvs[ind] = t;
            }
        }

        int i = 0;
        for(int y=0; y < rowPoints-1; ++y)
        {
            for(int x=0; x < colPoints-1; ++x)
            {
                m_triangles[i] = y * m_colPoints + x;
                m_triangles[i + 1] = (y+1) * m_colPoints + x;
                m_triangles[i + 2] = y * m_colPoints + x + 1;
                
                m_triangles[i + 3] = m_triangles[i + 1];
                m_triangles[i + 4] = (y + 1) * m_colPoints + x + 1;
                m_triangles[i + 5] = m_triangles[i + 2];
                i += 6;
            }
        }

        if (m_mesh == null)
        {
            m_mesh = new Mesh();
        }

        m_mesh.Clear();
        m_mesh.vertices = m_vertices;
        m_mesh.triangles = m_triangles;
        m_mesh.uv = m_uvs;
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateBounds();
        m_meshFilter.mesh = m_mesh;
    }
}
