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
    private Transform initPos;
    [Networked] public Vector3 UpdatedPos { get; set; }

    private bool m_isActive => m_direction != Vector3.zero;
    private bool m_hitTarget = false;

    public void Init(Vector3 dir)
    {
        m_direction = dir;
    }

    public void SetInit(Transform pos)
    {
        initPos = pos;
        //RPC_SetInit();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetInit()
    {
        transform.parent = initPos;
    }

    public override void FixedUpdateNetwork()
    {
        if (m_direction != Vector3.zero)
        {
            transform.localPosition += m_direction * m_speed * Runner.DeltaTime;
        }
        else
        {
            //transform.localPosition = initPos.position;
        }

        m_lifetime = (m_direction != Vector3.zero) ? m_lifetime -Runner.DeltaTime : 3.0f;
        if (m_lifetime <= 0.0f)
        {
            Runner.Despawn(m_bullet);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_hitTarget)
        {
            return;
        }

        if(other.gameObject.tag == "Bullet")
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
            var balloonNetworkObject = other.GetComponent<NetworkBehaviour>();
            if (balloonNetworkObject != null)
            {
                other.GetComponentInParent<PlayerMovement>()
                    .RPC_DestroyBalloon(balloonNetworkObject);
            }
        }

        Debug.Log("NAME: " + other.name + ", " + transform.position);

        m_bulletModel.SetActive(false);
        m_hitTarget = true;
    }

}
