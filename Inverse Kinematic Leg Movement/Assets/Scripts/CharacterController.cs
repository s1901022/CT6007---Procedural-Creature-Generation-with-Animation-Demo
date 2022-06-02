using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {
    public bool m_playerPossessed = false;
    private Vector3 m_targetPoint;

    private float m_speed;

    private void Start() {
        m_targetPoint = new Vector3(Random.Range(-90f, 90f), 0f, Random.Range(-90f, 90f));
        m_speed = Random.Range(2.5f, 10f);
    }

    private void FixedUpdate() {
        //check to make sure the generation is valid and the creature won't start flying
        if (transform.position.y > 5f) { Destroy(gameObject); }

        //movement
        if (m_playerPossessed) { PlayerControl(); return; }
        Movement();
    }

    private void Movement() {
        if ((transform.position.x < m_targetPoint.x + 2f && transform.position.x > m_targetPoint.x - 2f) && (transform.position.z < m_targetPoint.z + 2f && transform.position.z > m_targetPoint.z - 2f)) {
            m_targetPoint = new Vector3(Random.Range(-90f, 90f), 0f, Random.Range(-90f, 90f));
        }

        Vector3 targetDirection = m_targetPoint - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, 1f * Time.deltaTime, 0.0f);
        Vector3 oldRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.rotation = new Quaternion(oldRotation.x, transform.rotation.y, oldRotation.z, transform.rotation.w);
        transform.position += m_speed * Time.deltaTime * transform.forward;
    }

    private void PlayerControl() {
        //Camera camera = Camera.main;
        //camera.transform.position = new Vector3(transform.position.x, camera.transform.position.y, transform.position.z);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            transform.position += m_speed * Time.deltaTime * transform.forward;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  {
            transform.Rotate(0f, -60f * Time.deltaTime, 0f);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            transform.Rotate(0f, 60f * Time.deltaTime, 0f);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(m_targetPoint, 1f);
    }
}
