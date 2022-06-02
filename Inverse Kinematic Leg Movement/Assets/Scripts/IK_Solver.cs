using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class IK_Solver : MonoBehaviour
{
    [SerializeField]
    protected int m_chainLength = 2;  //number of bones in chain

    [SerializeField]
    protected Transform m_targetTransform;  //target position the chain should bend towards
    [SerializeField]
    protected Transform m_pole;

    [Header("IK Solver Paramaters")]
    [SerializeField]
    protected int m_itterations = 16;   //times ran per update

    [SerializeField]
    protected float m_delta = 0.001f; // Distance at which the solver stops

    [SerializeField] [Range(0f, 2f)]
    protected float m_snapStrength = 1f; //Strength of going back to the start position.

    //bones
    protected Transform[] m_bones;
    protected Vector3[] m_positions;

    //length of Target to Origin transform
    protected float[] m_bonesLength;
    protected float m_completeLength;
    
    //Individual bone rotations
    protected Vector3[] m_startDirectionSucc;
    protected Quaternion[] m_startRotationBone;

    //root
    protected Quaternion m_startRotationTarget;
    protected Transform m_RootBone;

    #region Initialisation
    private bool IK_Initialise_Bones() {
        //initialise Arrays
        m_bones =   new Transform[m_chainLength + 1];
        m_positions = new Vector3[m_chainLength + 1];
        m_bonesLength = new float[m_chainLength + 1];
        m_startDirectionSucc = new Vector3[m_chainLength + 1];
        m_startRotationBone = new Quaternion[m_chainLength + 1];

        //find root bone
        m_RootBone = transform;
        for (var i = 0; i <= m_chainLength; i++) {
            if (m_RootBone == null) {
                //if the number of bones stated is less than actual bones then error out and readjust the length of chain
                Debug.LogError("IK Solver for " + gameObject.name + ": " + "Chain of bones is longer than number of bones!");
                SetChainLength(i);
                IK_Initialise_Bones();
                return false;                                                                                                                       //this might break;
            }
            m_RootBone = m_RootBone.parent;
        }
        return true;
    }

    private bool IK_Initialise_Target(Transform a_targetTransform) {
        //Initialise the IK target
        bool result = true;
        if (a_targetTransform == null) {
            //Create an IK target if one does not exist and throw error at player
            Debug.LogError("IK Solver for " + gameObject.name + ": " + "Target Transform not assigned");
            a_targetTransform = new GameObject(gameObject.name + " Target_Transform").transform;
            SetPositionRootSpace(a_targetTransform, GetPositionRootSpace(transform));
            result = false;
        }
        m_startRotationTarget = GetRotationRootSpace(a_targetTransform);
        return result;
    }

    private void IK_Initialise_Data(Transform a_targetTransform) {
        var current = transform;
        m_completeLength = 0;
        for (var i = m_bones.Length - 1; i >= 0; i--) {
            m_bones[i] = current;
            m_startRotationBone[i] = GetRotationRootSpace(current);

            if (i == m_bones.Length - 1) {
                //leaf
                m_startDirectionSucc[i] = GetPositionRootSpace(a_targetTransform) - GetPositionRootSpace(current);
            }
            else {
                //mid bone
                m_startDirectionSucc[i] = GetPositionRootSpace(m_bones[i + 1]) - GetPositionRootSpace(current);
                m_bonesLength[i] = m_startDirectionSucc[i].magnitude;
                m_completeLength += m_bonesLength[i];
            }
            current = current.parent;
        }
    }

    private void Ik_Initialise() {
        if (IK_Initialise_Bones()) {
            IK_Initialise_Target(m_targetTransform);
            IK_Initialise_Data(m_targetTransform);
        }
    }
    #endregion

    private void Awake()
    {
        Ik_Initialise();
    }

    private void Update()
    {
        IK_Resolve();
    }

    private void IK_Resolve() {
        //error Handle - just in case this is resolved without correct initialisation
        if (m_targetTransform == null) { Debug.LogError("IK Solver for " + gameObject.name + ": " + "Target Transform not assigned"); return; }
        if (m_bonesLength.Length != m_chainLength) { Ik_Initialise(); }

        //Get Position
        for (int i = 0; i < m_bones.Length; i++) {
            m_positions[i] = GetPositionRootSpace(m_bones[i]);
        }

        //target position and rotation in space of root bone
        Vector3    v_targetPosition = GetPositionRootSpace(m_targetTransform);
        Quaternion v_targetRotation = GetRotationRootSpace(m_targetTransform);

        //check if the base (or foot) is capable of reaching the target position
        Vector3 v_difference = v_targetPosition - GetPositionRootSpace(m_bones[0]);
        if (v_difference.sqrMagnitude >= m_completeLength * m_completeLength) {
            //cannot reach so stretch leg out in direction
            Vector3 v_direction = (v_targetPosition - m_positions[0]).normalized;
            //set everything from root transform
            for (int i = 1; i < m_positions.Length; i++) {
                //+1 because we are not adjusting root
                m_positions[i] = m_positions[i - 1] + v_direction * m_bonesLength[i -1]; 
            }
        }
        else {
            //can be reached
            for (int i = 0; i < m_positions.Length-1; i++) {
                Vector3 v_newPos = Vector3.Lerp(m_positions[i + 1], m_positions[i] + m_startDirectionSucc[i], m_snapStrength);
                m_positions[i + 1] = v_newPos;
            }

            //itterations
            for (int iter = 0; iter < m_itterations; iter++) {
                //move back
                for (int i = m_positions.Length - 1; i > 0; i--) {
                    if (i == m_positions.Length - 1) { m_positions[i] = v_targetPosition; }
                    else { m_positions[i] = m_positions[i + 1] + (m_positions[i] - m_positions[i + 1]).normalized * m_bonesLength[i]; }
                }


                //move forward
                for (int i = 1; i < m_positions.Length; i++) {
                    m_positions[i] = m_positions[i - 1] + (m_positions[i] - m_positions[i - 1]).normalized * m_bonesLength[i - 1];
                }

                //position within tolerence
                if ((m_positions[m_positions.Length - 1] - v_targetPosition).sqrMagnitude < m_delta * m_delta) { break; }

            }

            //move towards pole
            if (m_pole != null) {
                Vector3 v_polePosition = GetPositionRootSpace(m_pole);
                for (int i = 1; i < m_positions.Length - 1; i++) {
                    Plane v_plane = new Plane(m_positions[i + 1] - m_positions[i - 1], m_positions[i - 1]);
                    Vector3 v_projectedPole = v_plane.ClosestPointOnPlane(v_polePosition);
                    Vector3 v_projectedBone = v_plane.ClosestPointOnPlane(m_positions[i]);
                    float v_angle = Vector3.SignedAngle(v_projectedBone - m_positions[i - 1], v_projectedPole - m_positions[i - 1], v_plane.normal);
                    m_positions[i] = Quaternion.AngleAxis(v_angle, v_plane.normal) * (m_positions[i] - m_positions[i - 1]) + m_positions[i - 1];
                }
            }

            //finally set position and rotation
            for (int i = 0; i < m_positions.Length; i++) {
                if (i == m_positions.Length - 1)   {
                    SetRotationRootSpace(m_bones[i], Quaternion.Inverse(v_targetRotation) * m_startRotationTarget * Quaternion.Inverse(m_startRotationBone[i]));
                }
                else {
                    SetRotationRootSpace(m_bones[i], Quaternion.FromToRotation(m_startDirectionSucc[i], m_positions[i + 1] - m_positions[i]) * Quaternion.Inverse(m_startRotationBone[i]));
                    SetPositionRootSpace(m_bones[i], m_positions[i]);
                }
            }
        }
    }

    public void UpdateTarget(Transform a_target) {
        m_targetTransform = a_target;
        IK_Initialise_Target(a_target);
    }

    #region Getters and Setters
    //getters and setters

    public void SetChainLength(int a_length) { m_chainLength = a_length; }

    private Vector3 GetPositionRootSpace(Transform current) {
        if (m_RootBone == null) { Debug.LogWarning("IK Solver for " + gameObject.name + ": " + "Root Bone is null"); return current.position; }
        return Quaternion.Inverse(m_RootBone.rotation) * (current.position - m_RootBone.position);
    }

    private void SetPositionRootSpace(Transform current, Vector3 position) {
        if (m_RootBone == null) { Debug.LogWarning("IK Solver for " + gameObject.name + ": " + "Root Bone is null"); current.position = position; return; }
        current.position = m_RootBone.rotation * position + m_RootBone.position;
    }

    private Quaternion GetRotationRootSpace(Transform current) {
        if (m_RootBone == null) { return current.rotation; }
        return Quaternion.Inverse(current.rotation) * m_RootBone.rotation;
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation) {
        if (m_RootBone == null) { current.rotation = rotation; return; }
        current.rotation = m_RootBone.rotation * rotation;
    }
    #endregion



}
