using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField][Range(0f, 100f)]
    private float m_speed;
    [SerializeField][Range(0f, 1f)]
    private float m_mouseSensetivity;
    private Vector3 m_lastMousePos;
    private Vector3 m_lastPosition;
    private Quaternion m_lastRotation;

    private CharacterController m_target;

    private void Start() {
        m_lastMousePos = new Vector3(255, 255, 255);
    }

    void Update() {
        //drag camera rotation
        if (Input.GetMouseButton(1)) {
            m_lastMousePos = Input.mousePosition - m_lastMousePos;
            m_lastMousePos = new Vector3(-m_lastMousePos.y * m_mouseSensetivity, m_lastMousePos.x * m_mouseSensetivity, 0);
            m_lastMousePos = new Vector3(transform.eulerAngles.x + m_lastMousePos.x, transform.eulerAngles.y + m_lastMousePos.y, 0);
            transform.eulerAngles = m_lastMousePos;
        }
        m_lastMousePos = Input.mousePosition;

        if (m_target == null) {
            //camera movement
            Vector3 velocity = new Vector3();
            if (Input.GetKey(KeyCode.W)) {
                velocity += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(KeyCode.S)) {
                velocity += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(KeyCode.A)) {
                velocity += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(KeyCode.D)) {
                velocity += new Vector3(1, 0, 0);
            }
            transform.Translate(velocity * m_speed * Time.deltaTime);
        }
        else {
            //fbs
            Vector3 targetDirection = (m_target.transform.forward *2) - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, m_target.transform.forward, 100 * Time.deltaTime, 0.0f);
            Vector3 oldRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
            transform.rotation = Quaternion.LookRotation(newDirection);
            transform.rotation = new Quaternion(oldRotation.x, transform.rotation.y, oldRotation.z, transform.rotation.w);

            transform.position = (m_target.transform.position + new Vector3(0f, 1f, 0f)) + m_target.transform.forward;

            if (Input.GetKeyDown(KeyCode.Escape))  { 
                m_target.m_playerPossessed = false;
                transform.position = m_lastPosition;
                transform.rotation = m_lastRotation;
            }
            if (m_target.m_playerPossessed == false) { m_target = null; }
        }

        //Select creature
        Ray ray;
        RaycastHit hit;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            if (Input.GetMouseButtonDown(0)) {
                Debug.Log(hit.collider.name);
                if (hit.transform.gameObject.GetComponent<CharacterController>()) {
                    if (m_target != null) { m_target.m_playerPossessed = false; }
                    m_target = hit.transform.gameObject.GetComponent<CharacterController>();
                    m_target.m_playerPossessed = true;
                    m_lastPosition = transform.position;
                    m_lastRotation = transform.rotation;
                }
            }
        }
    }

}
