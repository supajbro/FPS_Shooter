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

    public override void Update()
    {
        if (HasStateAuthority && Input.GetMouseButtonDown(0))
        {
            m_shootPressed = true;
        }

        base.Update();
    }

    public override void ShootBullet()
    {
        if (m_bullet1 == null)
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

        RPC_ShootBullet(shootDirection);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ShootBullet(Vector3 shootDirection)
    {
        m_bullet1.transform.parent = null;
        m_bullet1.transform.position = m_bulletSpawnPoint.position;
        m_bullet1.GetComponent<Bullet>().Init(shootDirection, m_bulletSpawnPoint.position); // Amy I love you xx
        m_bullet1 = null;
        m_ammoCount--;

        GetComponentInParent<Movement>().InitKnockback();
    }

}
