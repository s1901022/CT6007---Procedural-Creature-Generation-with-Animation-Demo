using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSkeleton : MonoBehaviour
{
    [SerializeField]
    private Material m_bodyMaterial;

    [SerializeField]
    private GameObject m_bone;
    [SerializeField]
    private GameObject m_node;
    [SerializeField]
    private List<GameObject> m_eyes;
    [SerializeField]
    private List<GameObject> m_spines; 
    [SerializeField]
    private List<GameObject> m_mouths;

    [SerializeField]
    private Vector2 m_radiusLimits = new Vector2(0.5f, 1f);
    [SerializeField][Range(-10f, 10f)]
    private float m_spineCurvature = 0;

    [SerializeField]
    private float m_length = 1f;            //total length of the body in world space
    [SerializeField][Range(3, 256)]
    private int m_meshResolutuion = 1;      //how many vertexes the mesh generates per bone
    [SerializeField][Range(3, 256)]
    private int m_boneResolutuion = 3;      //how many bones across the length of the body
    [SerializeField][Range(1, 16)]
    private int m_capsuleSmoothness = 8;

    private List<GameObject> m_attatchedAccessories;

    private int m_bodyAccessoryCount = 0;
    private Vector2 m_bodyAccessoryDeformation;
    private int m_bodyAccessoriesCurrent;

    private int m_headAccessoryCount = 0;
    private int m_headAccessoryDeformation;

    private int m_deformationRandomness = 10;

    float m_vertexCount => m_meshResolutuion * m_boneResolutuion+1;

    private List<SkeletonBone> m_bones;     //bones in the creatures body

    private Transform m_root;
    private GameObject m_model;
    private Mesh m_mesh;
    private List<Triangle> m_meshTriangles;

    private SkinnedMeshRenderer m_skinRenderer;
    private MeshFilter m_meshFilter;

    private Spine m_spine;

    private List<GameObject> m_legNodes;
    private List<GameObject> m_backNodes;
    private List<GameObject> m_faceNodes;

    private void Start() {
        
        m_bodyAccessoriesCurrent = 0;
    }

    private void Update() {
        bool regenerateMesh = false;
        for(int i = 0; i < m_bones.Count; i++)
        {
            if (m_bones[i].RegenerateVerticies() && regenerateMesh == false) { regenerateMesh = true; }
        }
        if (regenerateMesh) { GenerateMesh(); }
    }

    public void Initialise() {
        //initialise Spine
        m_spine = new Spine();
        m_spine.SetControlPoints(transform.GetComponentsInChildren<Transform>());
        m_spineCurvature = Random.Range(-m_length, m_length);
        GenerateSpine();

        m_mesh = new Mesh();

        //model
        m_model = new GameObject("Model");
        m_model.transform.SetParent(transform);
        m_model.transform.localPosition = Vector3.zero;
        m_model.transform.localRotation = Quaternion.identity;
        m_skinRenderer = m_model.AddComponent<SkinnedMeshRenderer>();
        m_meshFilter = m_model.AddComponent<MeshFilter>();

        //bones
        m_root = new GameObject("Root").transform;
        m_root.SetParent(transform);
        m_root.localPosition = Vector3.zero;
        m_root.localRotation = Quaternion.identity;

        //set up skin renderer
        m_skinRenderer.rootBone = m_root.transform;
        m_skinRenderer.updateWhenOffscreen = true;
        m_skinRenderer.sharedMaterial = m_bodyMaterial;

        m_attatchedAccessories = new List<GameObject>();

        GenerateBones();
        GenerateMesh();
    }

    private void GenerateBones() {
        //regenerate the list of bones
        m_bones = new List<SkeletonBone>();

        float t = 1f / m_boneResolutuion;
        float scale = 0f;
        Debug.Log(t);
        int j = 0;
        for (float i = 0; i <= 1; i += t) {
            //initialise each bone along the spine - note that we do not calculate mesh deformation and weights yet as we require all bones first
            GameObject newBone = Instantiate(m_bone, m_root);
            newBone.transform.position = m_spine.GetBezierPoint(i);
            newBone.transform.rotation = m_spine.GetBezierOrientation(i);
            m_bones.Add(newBone.GetComponent<SkeletonBone>());

            if (i == 0 || i >= 1) {
                m_bones[m_bones.Count - 1].Initialise(m_meshResolutuion, 0f, DistributeWeight());
            }
            else {
                if (m_bones.Count > 1 && m_bones.Count < m_boneResolutuion - 2) {
                    if (m_bones[j - 1].metaball.m_radius < m_radiusLimits.x) {
                        scale += Random.Range(0.1f, 0.25f);
                        m_bones[m_bones.Count - 1].Initialise(m_meshResolutuion, scale, DistributeWeight());
                    }
                    else {
                        m_bones[m_bones.Count - 1].Initialise(m_meshResolutuion, Random.Range(m_radiusLimits.x, m_radiusLimits.y), DistributeWeight());
                    }
                }
                else {
                    scale -= 0.1f;
                    m_bones[m_bones.Count - 1].Initialise(m_meshResolutuion, scale, DistributeWeight());
                }
            }
            j++;
        }

        //CalculatePull();
        SmoothRadius(10);

        AttatchLegs(Random.Range(1, 5));
        AttatchEyes(Random.Range(1, 16));
        AttatchBodyAccessories(10);
        AttatchHeadAccessories();
        DeformAccessories();

        PaintCreature();
    }

    public void GenerateSpine() {
        //Start Point
        m_spine.SetPosition(0, new Vector3(transform.position.x, transform.position.y, transform.position.z));

        //CurvatureControl
        m_spine.SetPosition(1, new Vector3(transform.position.x, transform.position.y + m_spineCurvature, transform.position.z + m_length / 4));
        m_spine.SetPosition(2, new Vector3(transform.position.x, transform.position.y + m_spineCurvature/2, transform.position.z + (m_length - m_length / 4)));

        //End Point
        m_spine.SetPosition(3, new Vector3(transform.position.x, transform.position.y + 0f, transform.position.z + m_length));
    }

    private float DistributeWeight() {
        return Random.Range(1f, 32f);
    }

    private void SmoothRadius(int itterations = 1) {
        for (int i = 1; i < itterations; i++) {
            for (int j = 1; j < m_bones.Count - 1; j++) {
                float prevRadius = m_bones[j].metaball.m_radius;
                float NextRadius = m_bones[j].metaball.m_radius;
                float myRadius = m_bones[j].metaball.m_radius;

                if (prevRadius > myRadius && NextRadius  > myRadius) {
                    m_bones[j].metaball.m_radius += NextRadius - prevRadius;
                }
            }
        }
    }

    private void AttatchEyes(int a_amount) {
        int startingPoint = m_bones.Count - 4;
        int endingPoint = m_bones.Count-1;

        int eyeType = Random.Range(0, m_eyes.Count);

        for (int eyeCount = 0; eyeCount < a_amount; eyeCount += 0) {
            for (int i = startingPoint; i < endingPoint; i++) {
                if (eyeCount >= a_amount) { return; }
                GameObject eyeA = Instantiate(m_eyes[eyeType], transform);
                GameObject eyeB = Instantiate(m_eyes[eyeType], transform);

                int eyeAPos = Random.Range(0, m_meshResolutuion / 4);
                Vector3 orientation = new Vector3(0f, Random.Range(0f, 42f), 0f);
                //since -1 isn't going to work for array pos use this to loop round to other end of array
                int eyeBPos = (m_meshResolutuion / 2) - eyeAPos;

                //position and orientate
                eyeA.transform.position = m_bones[i].verticies[eyeAPos].position;
                eyeB.transform.position = m_bones[i].verticies[eyeBPos].position;
                eyeA.transform.Rotate(new Vector3(0, 90, 0) - orientation);
                eyeB.transform.Rotate(new Vector3(0, -90, 0) + orientation);

                //add to list of accessories
                m_attatchedAccessories.Add(eyeA);
                m_attatchedAccessories.Add(eyeB);

                eyeCount += 2;
            }
        }
    }

    private void AttatchLegs(int a_amount) {
        int startingPoint = 0;
        int endingPoint = m_bones.Count - 4;
        
        //work out the threshold in which leg nodes can be validly placed
        for (int i = 0; i < m_bones.Count; i++) {
            if (m_bones[i].metaball.m_radius >= m_radiusLimits.x) { startingPoint = i; i = m_bones.Count; }
        }
        
        int distribution = (endingPoint - startingPoint)/ a_amount;
        Debug.Log(distribution);

        Vector3 orientation = new Vector3(0f, Random.Range(0f, 42f), 0f);

        //place nodes along the body (attempt to eavenly distribute)
        for (int i = startingPoint; i < endingPoint; i += distribution) {
            GameObject legA = Instantiate(m_node, transform);
            GameObject legB = Instantiate(m_node, transform);
            legA.transform.position = m_bones[i].verticies[0].position;
            legB.transform.position = m_bones[i].verticies[m_meshResolutuion / 2].position;

            legA.transform.Rotate(new Vector3(0, 90, 0) - orientation);
            legB.transform.Rotate(new Vector3(0, -90, 0) + orientation);

            
        }

        gameObject.GetComponent<Creature_LegManager>().AssignLegs();
    }

    private void AttatchBodyAccessories(int a_amount) {
        //starting and end points on mesh generated
        int startingPoint = 0;
        int endingPoint = m_bones.Count - 4;

        //work out the threshold in which leg nodes can be validly placed

        for (int i = 0; i < m_bones.Count; i++) {
            if (m_bones[i].metaball.m_radius >= m_radiusLimits.x) { startingPoint = i; i = m_bones.Count; }
        }

        int SpinesAlongSpine = Random.Range(0, 2);
        int nonSpinalSpines = Random.Range(0, 4);

        //generate spines along the spline
        if (SpinesAlongSpine > 0) {
            //how many spines
            int distribution = (endingPoint - startingPoint)/ Random.Range(1, a_amount);
            //generate spines along the creatures spine
            if (distribution > 0) {
                for (int i = 0; i < endingPoint; i += distribution) {
                    GameObject spine = Instantiate(m_spines[Random.Range(0, m_spines.Count - 1)], transform);
                    spine.transform.position = m_bones[i].verticies[m_meshResolutuion / 4].position;
                    m_bodyAccessoriesCurrent += 1;

                    //add to list of accessories
                    m_attatchedAccessories.Add(spine);
                }
            }
        }

        if (nonSpinalSpines > 2) {
            int amount = Random.Range(0, m_bodyAccessoriesCurrent/2);

            for (int j = 0; j < amount; j += 2) {
                for (int i = 0; i < endingPoint; i++)  {
                    int spineAPos = Random.Range(0, m_meshResolutuion / 4);
                    int spineBPos = (m_meshResolutuion / 2) - spineAPos;
                    GameObject spineA = Instantiate(m_spines[Random.Range(0, m_spines.Count - 1)], transform);
                    GameObject spineB = Instantiate(m_spines[Random.Range(0, m_spines.Count - 1)], transform);
                    spineA.transform.position = m_bones[i].verticies[spineAPos].position;
                    spineB.transform.position = m_bones[i].verticies[spineBPos].position;
                    m_bodyAccessoriesCurrent += 1;

                    //add to list of accessories
                    m_attatchedAccessories.Add(spineA);
                    m_attatchedAccessories.Add(spineB);
                }
            }
        }
    }

    private void AttatchHeadAccessories() {
        //attatch mouths
        GameObject mouth = m_mouths[Random.Range(0, m_mouths.Count)];
        if (mouth != null) {
            mouth = Instantiate(mouth, transform);
            mouth.transform.position = m_bones[m_bones.Count-1].verticies[0].position;

            //add to list of accessories
            m_attatchedAccessories.Add(mouth);
        }
    }

    private void GenerateMesh() {
        m_mesh.Clear();

        //create a new mesh
        m_mesh.name = gameObject.name + "_Body";

        //generate triangles across the bones
        int vertexCount = m_meshResolutuion * m_boneResolutuion + 1;

        List<Vector3> verts = new List<Vector3>();
        m_meshTriangles = new List<Triangle>();

        //add verticies position from each bone
        for (int i = 0; i < m_bones.Count; i++) {
            for (int j = 0; j < m_meshResolutuion; j++) {
                verts.Add(m_bones[i].verticies[j].position);
            }
        }

        List<int> triangleIndex = new List<int>();
        int ring = 0;
        //calculate triangles between the bones
        for (int i = 0; i < m_bones.Count - 1; i++) {
            for (int j = 0; j < m_meshResolutuion - 1; j++) {
                triangleIndex.Add(j + ring + 1);
                triangleIndex.Add(j + ring + m_meshResolutuion);
                triangleIndex.Add(j + ring);

                triangleIndex.Add(j + ring + m_meshResolutuion + 1);
                triangleIndex.Add(j + ring + m_meshResolutuion);
                triangleIndex.Add(j + ring + 1);
            }
            triangleIndex.Add(ring + m_meshResolutuion - 1);
            triangleIndex.Add(ring);
            triangleIndex.Add(ring + (m_meshResolutuion * 2) - 1);

            triangleIndex.Add(ring + (m_meshResolutuion * 2) - 1);
            triangleIndex.Add(ring);
            triangleIndex.Add(ring + m_meshResolutuion);

            ring += m_meshResolutuion;
        }

        //UVs
        Vector2[] uv = new Vector2[verts.Count];
        for (int i = 0; i < verts.Count; i++) {
            uv[i] = new Vector2(verts[i].x, verts[i].z);
        }

        m_mesh.SetVertices(verts);
        m_mesh.SetTriangles(triangleIndex, 0);
        m_mesh.RecalculateNormals();
        m_mesh.uv = uv;

        //SetMesh
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        Transform[] boneTransforms = new Transform[m_bones.Count];
        Matrix4x4[] bindPoses = new Matrix4x4[m_bones.Count];
        Vector3[] deltaZeroArray = new Vector3[verts.Count];


        for (int i = 0; i < m_bones.Count; i++) {
            for (int j = 0; j < m_meshResolutuion; j++) {
                boneWeights.Add(new BoneWeight() { boneIndex0 = 0, weight0 = 1 });
            }
        }
        m_mesh.boneWeights = boneWeights.ToArray();

        for (int vertIndex = 0; vertIndex < verts.Count; vertIndex++) {
            deltaZeroArray[vertIndex] = Vector3.zero;
        }

        for (int i = 0; i < m_bones.Count; i++) {
            boneTransforms[i] = m_bones[i].transform;
            boneTransforms[i].localPosition = m_bones[i].transform.localPosition;
            boneTransforms[i].localRotation = m_bones[i].transform.localRotation;
            bindPoses[i] = boneTransforms[i].worldToLocalMatrix * transform.localToWorldMatrix;

            Vector3[] deltaVertices = new Vector3[verts.Count];
            for (int j = 0; j < verts.Count; j++) {
                float maxDistanceAlongBone = i * 2f;
                float maxHeightAboveBone = i* 2f;

                float displacementAlongBone = verts[j].z - boneTransforms[i].localPosition.z;

                float x = Mathf.Clamp(displacementAlongBone / maxDistanceAlongBone, -1, 1);
                float a = maxHeightAboveBone;
                float b = 1f / a;

                float heightAboveBone = (Mathf.Cos(x * Mathf.PI) / b + a) / 2f;

                deltaVertices[j] = new Vector2(verts[j].x, verts[j].y).normalized * heightAboveBone;
            }
            m_mesh.AddBlendShapeFrame("Bone." + i, 0, deltaZeroArray, deltaZeroArray, deltaZeroArray);
            m_mesh.AddBlendShapeFrame("Bone." + i, 100, deltaVertices, deltaZeroArray, deltaZeroArray);
        }

        m_mesh.bindposes = bindPoses;
        m_meshFilter.sharedMesh = m_mesh;
        m_skinRenderer.bones = boneTransforms;
        m_skinRenderer.sharedMesh = m_mesh;

        SetupColliders();
    }

    private void SetupColliders() {
        m_model.layer = 6;
        m_model.AddComponent<MeshCollider>();
        m_model.GetComponent<MeshCollider>().sharedMesh = m_mesh;
        m_model.GetComponent<MeshCollider>().convex = true;

        m_model.AddComponent<Rigidbody>();
        transform.position = new Vector3(0f, 1.25f, 0f);
    }

    private void DeformAccessories() { 
        for (int i = 0; i < m_attatchedAccessories.Count; i++) {
            float deformMax = Random.Range(1f, m_bodyAccessoryDeformation.y);
            Vector3 ls = m_attatchedAccessories[i].transform.localScale;
            Vector3 newLs = new Vector3(ls.x, ls.y, ls.z) * deformMax;
            m_attatchedAccessories[i].transform.localScale = newLs;
        }
    }

    private void PaintCreature() {
        //paint main body
        foreach (Renderer s in transform.GetComponentsInChildren<Renderer>()) {
            if (s.gameObject.tag != "Eye") {
                s.material.SetColor("Colour", new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f));
            }
        }

        Color accessoryColour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        foreach (GameObject accessory in m_attatchedAccessories) {
            if (accessory.tag != "Eye")
            {
                accessory.GetComponent<Renderer>().material.color = accessoryColour;
            }
        }
    }


    //getters and setters
    public void SetLength(int a_length) { m_length = a_length; }
    public void SetSpineVariation(float a_value) { m_spineCurvature = a_value; }
    public void SetAccessoryLimit(int a_value) { m_bodyAccessoryCount = a_value; }
    public void SetDeformation(float a_value) { m_radiusLimits = new Vector3(m_radiusLimits.x, a_value); }
    public void SetAccessoryDefformation(float a_value) { m_bodyAccessoryDeformation = new Vector2(0f, a_value); }
}
