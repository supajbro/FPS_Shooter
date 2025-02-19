using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : NetworkBehaviour
{

    [Header("Main Components")]
    [SerializeField] protected NetworkPrefabRef m_bulletPrefab;
    [SerializeField] protected Transform m_bulletSpawnPoint;
    protected NetworkObject m_bullet;

    protected bool m_shootPressed;

    [Header("Ammo")]
    protected int m_ammoCount = 0;
    protected int m_maxAmmoCount = 1;
    public int AmmoCount { get { return m_ammoCount; } }
    public int MaxAmmoCount { get { return m_maxAmmoCount; } }

    [Header("Reload")]
    protected bool m_reloading = false;
    protected float m_reloadTime = 0f;
    protected float m_maxReloadTime = 1f;

    public virtual void Awake()
    {
        m_reloading = true;
    }

    public override void Spawned()
    {
        m_ammoCount = m_maxAmmoCount;
        Reload();
    }

    public override void FixedUpdateNetwork()
    {
        if (m_shootPressed && m_ammoCount > 0)
        {
            m_shootPressed = false;
            ShootBullet();
        }

        if (m_ammoCount == 0)
        {
            m_reloading = true;
        }

        Reload();
    }

    public virtual void ShootBullet(){}

    private void SpawnBullet()
    {
        // Spawn the bullet at the spawn point with the correct rotation
        NetworkObject bullet = Runner.Spawn(
            m_bulletPrefab,
            m_bulletSpawnPoint.position,
            m_bulletSpawnPoint.rotation,
            Object.InputAuthority,
            (runner, obj) =>
            {
                m_bullet = obj;
                m_bullet.GetComponent<Bullet>().SetInit(m_bulletSpawnPoint); // Amy I love you xx
                m_bullet.transform.parent = m_bulletSpawnPoint;
            });
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

}
