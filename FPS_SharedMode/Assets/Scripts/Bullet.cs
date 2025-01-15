using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{

    [SerializeField] private NetworkObject m_bullet;
    [SerializeField] private GameObject m_bulletModel;

    [Header("Values")]
    [SerializeField] private float m_speed = 10.0f;
    [SerializeField] private Vector3 m_direction = Vector3.zero;
    [SerializeField] private float m_lifetime = 3.0f;
    [SerializeField] private float m_damage = 100f;

    private bool m_isActive = true;

    public void Init(Vector3 dir)
    {
        m_isActive = true;
        m_direction = dir.normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (m_direction != Vector3.zero)
        {
            transform.position += m_direction * m_speed * Runner.DeltaTime;
        }

        m_lifetime -= Runner.DeltaTime;
        if (m_lifetime <= 0.0f)
        {
            Runner.Despawn(m_bullet);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isActive)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerMovement player) && HasStateAuthority)
        {
            // Don't let any of this code run if you hit the local player
            if(player == GameManager.instance.GetLocalPlayer())
            {
                return;
            }

            //player.RPC_TakeDamage(damage: m_damage);
        }

        if(other.gameObject.tag == "Balloon")
        {
            other.GetComponentInParent<PlayerMovement>().DestroyBalloon(other.gameObject);
        }

        m_bulletModel.SetActive(false);
        m_isActive = false;
    }

}
