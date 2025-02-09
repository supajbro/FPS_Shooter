using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Lava : NetworkBehaviour
{

    [Header("Values")]
    [SerializeField] private float m_speed = 5.0f;
    private Vector3 m_position;

    private void Awake()
    {
        m_position = transform.position;
    }

    public override void FixedUpdateNetwork()
    {
        m_position.y += Runner.DeltaTime * m_speed;
        transform.position = m_position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement player) && HasStateAuthority)
        {
            player.RPC_PlayerDie();
        }
    }

}
