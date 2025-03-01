using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{

    [SerializeField] protected NetworkObject m_bullet;
    [SerializeField] protected GameObject m_bulletModel;

    [Header("Values")]
    [SerializeField] protected float m_speed = 10.0f;
    [SerializeField] protected Vector3 m_direction = Vector3.zero;
    [SerializeField] protected Vector3 m_startPosition = Vector3.zero;
    [SerializeField] protected float m_lifetime = 3.0f;
    [SerializeField] protected float m_damage = 100f;
    [SerializeField] private LayerMask m_balloonLayer;
    [Networked] public Vector3 UpdatedPos { get; set; }

    private Vector3 m_pos;

    private bool m_isActive => m_direction != Vector3.zero;
    protected bool m_hitTarget = false;

    public void Init(Vector3 dir, Vector3 startPos)
    {
        m_direction = dir;
        m_startPosition = startPos;
    }

    public virtual void Update()
    {
        if (m_direction != Vector3.zero)
        {
            transform.localPosition += m_direction * m_speed * Time.deltaTime;
            //Debug.DrawLine(m_startPosition, transform.localPosition += m_direction * 100, Color.yellow);
            DestroyBalloonLogic();
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        return;

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
        }

        if(other.gameObject.tag == "Balloon")
        {
            var balloonNetworkObject = other.GetComponent<NetworkBehaviour>();
            if (balloonNetworkObject != null)
            {
                other.GetComponentInParent<Movement>().RPC_DestroyBalloon(balloonNetworkObject);
            }
        }

        Debug.Log("NAME: " + other.name + ", " + transform.position);

        m_bulletModel.SetActive(false);
        m_hitTarget = true;
    }

    private void DestroyBalloonLogic()
    {
        if (m_hitTarget)
        {
            return;
        }

        if (Physics.Raycast(m_startPosition, m_direction, out RaycastHit hit, 100, m_balloonLayer))
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
            var balloonNetworkObject = hit.collider.gameObject.GetComponent<NetworkBehaviour>();
            if (balloonNetworkObject != null)
            {
                hit.collider.gameObject.GetComponentInParent<Movement>().RPC_DestroyBalloon(balloonNetworkObject);

                m_bulletModel.SetActive(false);
                m_hitTarget = true;
            }
        }
    }

}
