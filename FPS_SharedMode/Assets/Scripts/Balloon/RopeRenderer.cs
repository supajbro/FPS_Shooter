using Fusion;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeRenderer : NetworkBehaviour
{
    [SerializeField] private Transform m_objectA;
    [SerializeField] private Transform m_objectB;

    [SerializeField] private int m_segmentCount = 10; // Number of rope segments
    [SerializeField] private float m_ropeWidth = 0.05f; // Thickness of the rope
    [SerializeField] private float m_ropeTension = 10f; // Spring force (higher = tighter rope)

    private LineRenderer m_lineRenderer;
    private List<Rigidbody> m_ropeSegments = new List<Rigidbody>();

    public override void Spawned()
    {
        base.Spawned();
        if (HasStateAuthority)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        GenerateRope();
    }

    void Update()
    {
        UpdateLineRenderer();
    }

    void GenerateRope()
    {
        // Create a LineRenderer
        m_lineRenderer = gameObject.AddComponent<LineRenderer>();
        m_lineRenderer.startWidth = m_ropeWidth;
        m_lineRenderer.endWidth = m_ropeWidth;
        m_lineRenderer.positionCount = m_segmentCount;

        // Create Rope Segments
        Rigidbody previousSegment = m_objectA.transform.GetComponent<Rigidbody>();

        for (int i = 0; i < m_segmentCount; i++)
        {
            GameObject segment = new GameObject("RopeSegment" + i);
            segment.transform.position = Vector3.Lerp(m_objectA.transform.position, m_objectB.transform.position, (float)i / m_segmentCount);

            Rigidbody rb = segment.AddComponent<Rigidbody>();
            rb.mass = 0.2f; // Lower mass for a lighter rope
            rb.drag = 0.1f; // Small drag for damping
            m_ropeSegments.Add(rb);

            SphereCollider col = segment.AddComponent<SphereCollider>();
            col.radius = m_ropeWidth * 2; // Small collider to prevent clipping

            // Connect segments with SpringJoint
            if (previousSegment != null)
            {
                SpringJoint joint = segment.AddComponent<SpringJoint>();
                joint.connectedBody = previousSegment;
                joint.spring = m_ropeTension; // Adjust for stiffness
                joint.damper = 0.5f; // Adjust for wobbliness
                joint.minDistance = 0.1f;
                joint.maxDistance = 0.2f;
            }

            previousSegment = rb;
        }

        // Attach last segment to the end object
        SpringJoint finalJoint = m_ropeSegments[m_ropeSegments.Count - 1].gameObject.AddComponent<SpringJoint>();
        finalJoint.connectedBody = m_objectB.transform.GetComponent<Rigidbody>();
        finalJoint.spring = m_ropeTension;
        finalJoint.damper = 0.5f;
        finalJoint.minDistance = 0.1f;
        finalJoint.maxDistance = 0.2f;
    }

    void UpdateLineRenderer()
    {
        if (m_lineRenderer == null || m_ropeSegments.Count == 0) return;

        m_lineRenderer.positionCount = m_ropeSegments.Count;
        for (int i = 0; i < m_ropeSegments.Count; i++)
        {
            m_lineRenderer.SetPosition(i, m_ropeSegments[i].position);
        }
    }
}
