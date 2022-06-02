using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature_LegManager : MonoBehaviour {
    //make sure to add legs to list in a zig zag pattern from front to back
    public List<IK_Leg> m_legs;
    private bool m_stepping;

    [SerializeField]
    private Transform m_body;

    private int m_legSide;

    private Vector3 m_targetBodyPos;

    public LayerMask m_layerMask;

    private void Start() {
        m_legSide = 0;
        SwapLegs();
    }

    private void Update() {
        foreach (IK_Leg leg in m_legs) { leg.UpdateIK(); }
        for (int i = 0; i < m_legs.Count; i++) {
            if (m_legs[i].moving) { m_stepping = true; }
            if (m_stepping && !m_legs[i].m_legGrounded)  {
                //we are still taking a step we shouldn't swap legs in the middle of a step
                return;
            }
        }

        if (m_stepping) {
            Debug.Log("wow");
            m_legSide += 1;
            m_legSide %= 2;
            //position body
            m_body.position = FindAverageLegPosition();
            SwapLegs();
            m_stepping = false;
        }
    }

    public void AssignLegs() {
        m_legs = new List<IK_Leg>();
        int listOffset = 0;
        foreach (IK_Leg leg in GetComponentsInChildren<IK_Leg>())   {
            m_legs.Add(leg);
        }

        for (int i = 1; i < m_legs.Count; i++) {
            if (i % 4 == 0) {
                IK_Leg swap = m_legs[i];
                m_legs[i] = m_legs[i - 1];
                m_legs[i - 1] = swap;
            }
        }
    }

    private void SwapLegs() {
        for (int i = 0; i < m_legs.Count; i++) {
            if (i % 2 == m_legSide) { m_legs[i].m_canMove = true; }
            else { m_legs[i].m_canMove = false; }
        }
    }

    private Vector3 FindAverageLegPosition() {
        float vTotalX = 0f, vTotalY = 0f, vTotalZ = 0f;
        foreach (IK_Leg leg in m_legs) {
            vTotalX += leg.GetComponentInChildren<IK_Solver>().transform.position.x;
            vTotalY += leg.GetComponentInChildren<IK_Solver>().transform.position.y;
            vTotalZ += leg.GetComponentInChildren<IK_Solver>().transform.position.z;
        }
        float vMeanX = 0f, vMeanY = 0f, vMeanZ = 0f;
        //calculate average position of legs
        vMeanX = vTotalX / m_legs.Count;
        vMeanY = vTotalY / m_legs.Count;
        vMeanZ = vTotalZ / m_legs.Count;

        return new Vector3(m_body.position.x, vMeanY+2f, m_body.position.z);
    }
}
