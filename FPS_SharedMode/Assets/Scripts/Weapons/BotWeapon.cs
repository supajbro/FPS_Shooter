using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotWeapon : Weapon
{

    [Header("Bot Values")]
    [SerializeField] private GameObject m_parent;
    [SerializeField] private float m_detectionRadius = 5.0f;
    [SerializeField] private LayerMask m_playerLayer;
    private BotMovement m_movement;

    private Transform m_target;

    public Transform Target => m_target;
    public bool ShootPressed => m_shootPressed;

    public override void Awake()
    {
        base.Awake();
        m_movement = m_parent.GetComponent<BotMovement>();
    }

    private void Update()
    {
        DetectionUpdate();
    }

    // Checks if another player is within radius
    private void DetectionUpdate()
    {
        // Don't run this if the bot is in the air
        if (!m_movement.IsGrounded)
        {
            m_target = null;
            return;
        }

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, m_detectionRadius, Vector3.up, 0, m_playerLayer);
        List<Transform> targets = new();

        // Detecting if there are any players in the bots radius
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player") && hit.collider.gameObject != m_parent)
            {
               Debug.Log($"Detected player: {hit.collider.gameObject.name}");
               targets.Add(hit.collider.transform);
            }
        }

        // If we didn't find any players in the radius, don't continue
        if(targets.Count == 0)
        {
            m_target = null;
            return;
        }

        // Find the closest target
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (var target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
                m_target = closestTarget;
                m_shootPressed = true;
            }
        }

        return;
    }

    public override void ShootBullet()
    {
        if (m_bullet == null)
        {
            return;
        }

        base.ShootBullet();

        Vector3 targetPoint;
        const float maxShootDistance = 100f;

        Vector3 directionToTarget = (m_target.transform.position - m_bulletSpawnPoint.position).normalized;
        targetPoint = m_bulletSpawnPoint.position + directionToTarget * maxShootDistance;

        // Calculate the shoot direction
        Vector3 shootDirection = (targetPoint - m_bulletSpawnPoint.position).normalized;

        m_bullet.transform.parent = null;
        m_bullet.GetComponent<Bullet>().Init(shootDirection); // Amy I love you xx
        m_bullet = null;
        m_ammoCount--;

        GetComponentInParent<Movement>().InitKnockback();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_detectionRadius);
    }

}
