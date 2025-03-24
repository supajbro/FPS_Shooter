using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBullet : Bullet
{

    public override void Update()
    {
    }

    public override void FixedUpdateNetwork()
    {
        if (m_direction != Vector3.zero)
        {
            transform.localPosition += m_direction * m_speed * Runner.DeltaTime;
        }

        m_lifetime = (m_direction != Vector3.zero) ? m_lifetime - Runner.DeltaTime : 3.0f;
        if (m_lifetime <= 0.0f)
        {
            Runner.Despawn(m_bullet);
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (m_hitTarget)
        {
            return;
        }

        if (other.gameObject.tag == "Bullet")
        {
            return;
        }

        if (other.TryGetComponent(out PlayerMovement player) && HasStateAuthority)
        {
            // Don't let any of this code run if you hit the local player
            if (player == GameManager.instance.GetLocalPlayer())
            {
                return;
            }
        }

        if (other.gameObject.tag == "Balloon")
        {
            var balloonNetworkObject = other.GetComponent<NetworkBehaviour>();
            if (balloonNetworkObject != null)
            {
                var movement = other.GetComponentInParent<Movement>();
                if (movement != null)
                {
                    movement.RPC_DestroyBalloon(balloonNetworkObject);
                }
            }
        }

        Debug.Log("NAME: " + other.name + ", " + transform.position);

        m_bulletModel.SetActive(false);
        m_hitTarget = true;
    }

}
