using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))] [RequireComponent(typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour {
    [SerializeField]
    private int m_size;

    private void Awake() {
        GetComponent<MeshFilter>().sharedMesh = CreateMesh();
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    private Mesh CreateMesh() {
        Mesh mesh = new Mesh();

        Vector3[] Verticies = new Vector3[(m_size + 1) * (m_size + 1)];
        for (int i = 0, z = 0; z <= m_size; z++) {
            for (int x = 0; x <= m_size; x++) {
                float y = Mathf.PerlinNoise(x * 0.2f, z * 0.2f) * 2f;
                Verticies[i] = new Vector3(x, y, z);
                i++;
            }
        }

        int[] triangles = new int[m_size * m_size * 6];
        int t = 0;
        int v = 0;
        for (int z = 0; z < m_size; z++) {
            for (int x = 0; x < m_size; x++) {
                triangles[t + 0] = v + 0;
                triangles[t + 1] = v + m_size + 1;
                triangles[t + 2] = v + 1;

                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + m_size + 1;
                triangles[t + 5] = v + m_size + 2;

                v++;
                t += 6;
            }
            v++;
        }

        mesh.SetVertices(Verticies);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, Verticies);
        mesh.RecalculateNormals();
        return mesh;
    }
}
