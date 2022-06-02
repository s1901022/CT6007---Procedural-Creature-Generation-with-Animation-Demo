using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonBone : MonoBehaviour {
    public Metaball metaball;
    public Vertex[] verticies;

    private Vector3 m_lastPosition;

    public void Initialise(int a_resolution, float a_radius, float a_weight = 1f) {
        metaball = new Metaball();
        metaball.m_weight = a_weight;
        metaball.m_resolution = a_resolution;
        metaball.m_radius = a_radius;

        m_lastPosition = transform.localPosition;
        GenerateVerticies();
    }

    public bool RegenerateVerticies() {
        if (m_lastPosition != transform.localPosition) {
            m_lastPosition = transform.localPosition;
            GenerateVerticies();
            return true;
        }
        return false;
    }

    public void GenerateVerticies() {
        verticies = new Vertex[metaball.m_resolution];
        for (int i = 0; i < metaball.m_resolution; i++) {
            //calculate points around circumfrance
            float t = i / (float)metaball.m_resolution;
            float angRad = t * MathsEX.TAU;
            Vector2 vec2 = MathsEX.GetVectorFromAngle(angRad) * metaball.m_radius;

            verticies[i] = new Vertex();
            verticies[i].position = transform.position + transform.rotation * vec2;
        }

    }

    private void OnDrawGizmos() {
        foreach(Vertex vertex in verticies) {
            Gizmos.DrawSphere(vertex.position, 0.02f);
        }
    }
}
