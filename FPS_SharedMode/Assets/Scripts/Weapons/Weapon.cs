using DG.Tweening;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : NetworkBehaviour
{

    [Header("Main Components")]
    [SerializeField] protected NetworkPrefabRef m_bulletPrefab;
    [SerializeField] protected GameObject m_bulletPrefab1;
    [SerializeField] protected Transform m_bulletSpawnPoint;
    protected NetworkObject m_bullet;
    protected GameObject m_bullet1;

    protected bool m_shootPressed;

    [Header("Ammo")]
    protected int m_ammoCount = 0;
    protected int m_maxAmmoCount = 1;
    public int AmmoCount { get { return m_ammoCount; } }
    public int MaxAmmoCount { get { return m_maxAmmoCount; } }

    [Header("Reload")]
    protected bool m_reloading = false;
    protected float m_reloadTime = 0f;
    protected const float m_maxReloadTime = 0.25f;

    public virtual void Awake()
    {
        m_reloading = true;
    }

    public override void Spawned()
    {
        m_ammoCount = m_maxAmmoCount;
        Reload();
    }

    public virtual void Update()
    {
        // Shoot the bullet locally
        if (m_shootPressed && m_ammoCount > 0)
        {
            m_shootPressed = false;
            ShootBullet();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (m_ammoCount == 0)
        {
            m_reloading = true;
        }

        Reload();
    }

    public virtual void ShootBullet(){}

    public virtual void SpawnBullet()
    {
        RPC_SpawBullet();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SpawBullet()
    {
        var bullet = Instantiate(m_bulletPrefab1, m_bulletSpawnPoint.position,
            m_bulletSpawnPoint.rotation);
        bullet.transform.parent = m_bulletSpawnPoint;
        m_bullet1 = bullet;
    }

    private void Reload()
    {
        if (!m_reloading)
        {
            return;
        }

        m_reloadTime += Runner.DeltaTime;
        if (m_reloadTime >= m_maxReloadTime)
        {
            m_reloading = false;
            m_reloadTime = 0f;
            m_ammoCount = m_maxAmmoCount;
            SpawnBullet();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_bulletSpawnPoint.position, .25f);
    }

}
