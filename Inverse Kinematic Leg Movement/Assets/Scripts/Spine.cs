using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Spine {
    private Transform[] m_controlPoints;

    public void SetControlPoints(Transform[] a_transforms) { 
        m_controlPoints = new Transform[4] { a_transforms[1], a_transforms[2], a_transforms[3], a_transforms[4] }; 
    }
    public Vector3 GetPosition(int i) { return m_controlPoints[i].position; }

    public void SetPosition(int i, Vector3 a_pos) { m_controlPoints[i].position = a_pos; }

    public Vector3 GetBezierPoint(float t) {
        Vector3 p0 = GetPosition(0);
        Vector3 p1 = GetPosition(1);
        Vector3 p2 = GetPosition(2);
        Vector3 p3 = GetPosition(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(d, e, t);
    }

    public Quaternion GetBezierOrientation(float t) {
        Vector3 tangent = GetBezierTangent(t);

        return Quaternion.LookRotation(tangent);
    }

    public Vector3 GetBezierTangent(float t) {
        Vector3 p0 = GetPosition(0);
        Vector3 p1 = GetPosition(1);
        Vector3 p2 = GetPosition(2);
        Vector3 p3 = GetPosition(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return (e - d).normalized;
    }

    public void DrawGizmosSpine() {
        for (int i = 0; i < 4; i++) {
            Gizmos.DrawSphere(GetPosition(i), 0.05f);
        }

        Handles.DrawBezier(GetPosition(0), GetPosition(3), GetPosition(1), GetPosition(2), Color.white, EditorGUIUtility.whiteTexture, 1f);
    }
}
