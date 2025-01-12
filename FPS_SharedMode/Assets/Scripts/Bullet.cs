using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{

    [Header("Values")]
    [SerializeField] private float m_speed = 10.0f;
    [SerializeField] private Vector3 m_direction = Vector3.zero;

    public void Init(Vector3 dir)
    {
        m_direction = dir.normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (m_direction != Vector3.zero)
        {
            transform.position += m_direction * m_speed * Runner.DeltaTime;
        }
    }

}
