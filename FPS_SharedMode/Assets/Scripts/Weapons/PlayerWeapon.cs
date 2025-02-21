using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerWeapon : Weapon
{

    [SerializeField] private Transform m_target = null;

    public void Init(Transform target)
    {
        m_target = target;
    }

    private void Update()
    {
        if (HasStateAuthority && Input.GetMouseButtonDown(0))
        {
            m_shootPressed = true;
        }
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
        m_bullet.transform.position = m_bulletSpawnPoint.position;
        m_bullet.GetComponent<Bullet>().Init(shootDirection); // Amy I love you xx
        m_bullet = null;
        m_ammoCount--;

        GetComponentInParent<Movement>().InitKnockback();
    }

}
