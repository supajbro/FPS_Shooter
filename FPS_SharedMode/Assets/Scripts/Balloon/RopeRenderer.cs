using Fusion;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeRenderer : NetworkBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public GameObject ropeSegmentPrefab;
    public int segmentCount = 10;
    public float segmentSpacing = 0.5f;

    private GameObject[] segments;

    void Start()
    {
        GenerateRope();
    }

    void FixedUpdate()
    {
        // Update first and last segment positions dynamically
        if (segments.Length > 0)
        {
            Rigidbody firstRB = segments[0].GetComponent<Rigidbody>();
            Rigidbody lastRB = segments[segments.Length - 1].GetComponent<Rigidbody>();

            // Forcefully move the first and last segment to follow the targets
            firstRB.MovePosition(startPoint.position);
            lastRB.MovePosition(endPoint.position);
        }
    }

    void GenerateRope()
    {
        segments = new GameObject[segmentCount];
        GameObject previousSegment = null;

        for (int i = 0; i < segmentCount; i++)
        {
            // Create evenly spaced rope segments
            Vector3 position = Vector3.Lerp(startPoint.position, endPoint.position, (float)i / (segmentCount - 1));
            GameObject segment = Instantiate(ropeSegmentPrefab, position, Quaternion.identity);
            Rigidbody rb = segment.GetComponent<Rigidbody>();

            if (rb == null)
            {
                Debug.LogError("Rope segment prefab must have a Rigidbody!");
                return;
            }

            segments[i] = segment;

            // Connect with joints
            if (previousSegment != null)
            {
                ConfigurableJoint joint = segment.AddComponent<ConfigurableJoint>();
                joint.connectedBody = previousSegment.GetComponent<Rigidbody>();

                // Allow rope-like movement
                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit limit = new SoftJointLimit { limit = segmentSpacing };
                joint.linearLimit = limit;
            }

            previousSegment = segment;
        }
    }
}
