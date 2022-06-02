using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Leg : MonoBehaviour {
    public Transform m_raycastPoint;
    public Transform m_target;
    public Vector3 m_restingPosition;
    public LayerMask m_mask;
    public Vector3 m_newPosition;
    public Transform m_steppingPoint;
    public bool m_legGrounded;
    public GameObject m_player;
    public float offset;
    public float moveDistance = 1f;
    public float speed = 10f;

    public bool m_canMove;
    public bool hasMoved;
    public bool moving;
    public bool movingDown;

    public GameObject prefabTransform;
    public int leg;

    private void Start() {
        m_steppingPoint = Instantiate(prefabTransform, transform.parent).transform;
        m_restingPosition = m_target.position;
        m_steppingPoint.position = new Vector3(m_restingPosition.x + offset, m_restingPosition.y, m_restingPosition.z);
    }

    private void Update() {
        m_newPosition = calculateStepPoint(m_steppingPoint.position);
        //m_steppingPoint.transform.position = calculateStepPoint(m_steppingPoint.position);
        calculateStepHeight();
        //m_steppingPoint.transform.position = new Vector3(m_steppingPoint.transform.position.x, m_target.transform.position.y, m_steppingPoint.transform.position.z);
        if (Vector3.Distance(m_restingPosition, m_newPosition) > moveDistance || moving && m_legGrounded) {
            Step(m_newPosition);
        }
        UpdateIK();
    }

    private void Step(Vector3 a_position) {
        if (m_canMove) {
            m_legGrounded = false;
            hasMoved = false;
            moving = true;

            //move leg upwards
            m_target.position = Vector3.MoveTowards(m_target.position, a_position + Vector3.up, speed * Time.deltaTime);
            m_restingPosition = Vector3.MoveTowards(m_target.position, a_position + Vector3.up, speed * Time.deltaTime);
            //move leg downwards
            if (m_target.position == a_position + Vector3.up) {
                movingDown = true;
            }
            if (movingDown)  {
                m_target.position = Vector3.MoveTowards(m_target.position, a_position, speed * Time.deltaTime);
                m_restingPosition = Vector3.MoveTowards(m_target.position, a_position, speed * Time.deltaTime);
            }

            if (m_target.position == a_position) {
                m_legGrounded = true;
                hasMoved = true;
                moving = false;
                movingDown = false;
            }
        }
    }

    public void UpdateIK() {
        m_target.position = m_restingPosition;
    }

    public Vector3 calculateStepPoint(Vector3 a_position) {
        Vector3 dir = a_position - m_raycastPoint.position;
        RaycastHit hit;
        if (Physics.SphereCast(m_raycastPoint.position, 0.1f, dir, out hit, 10f, m_mask)) {
            a_position = hit.point;
            m_legGrounded = true;
        }
        else {
            a_position = m_restingPosition;
            m_legGrounded = false;
        }
        return a_position;
    }

    public void calculateStepHeight() {
        RaycastHit hitDown;        

        if(Physics.Raycast(m_steppingPoint.position, -Vector3.up, out hitDown, 10f, m_mask)) {
            //move up
            if (hitDown.point.y == m_steppingPoint.position.y) {
                m_steppingPoint.position -= new Vector3(0f,1f, 0f);
            }
            else  {
                m_steppingPoint.position -= new Vector3(0f, 0.1f, 0f);
            }
        }

        if (m_legGrounded && !moving) {
            float height = m_steppingPoint.position.y;
            float clampedHight = Mathf.Clamp(height, m_target.position.y, height);
            m_steppingPoint.position = new Vector3(m_steppingPoint.transform.position.x, clampedHight, m_steppingPoint.transform.position.z);
        }
    }

}
