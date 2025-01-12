using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{

    [SerializeField] private NetworkPrefabRef m_bulletPrefab;
    [SerializeField] private Transform m_bulletSpawnPoint;

    private bool m_shootPressed;

    [Header("Ammo")]
    private int m_ammoCount = 0;
    private int m_maxAmmoCount = 1;

    [Header("Reload")]
    private bool m_reloading = false;
    private float m_reloadTime = 0f;
    private float m_maxReloadTime = 1f;

    public override void Spawned()
    {
        m_ammoCount = m_maxAmmoCount;
    }

    private void Update()
    {
        if (HasStateAuthority && Input.GetMouseButtonDown(0))
        {
            m_shootPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (m_shootPressed && m_ammoCount > 0)
        {
            m_shootPressed = false;
            SpawnBullet();
        }

        if(m_ammoCount == 0)
        {
            m_reloading = true;
        }

        Reload();
    }

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
                obj.GetComponent<Bullet>().Init(Camera.main.transform.forward);
            });

        m_ammoCount--;
    }

    private void Reload()
    {
        if (!m_reloading)
        {
            return;
        }

        m_reloadTime += Runner.DeltaTime;
        if(m_reloadTime >= m_maxReloadTime)
        {
            m_reloading = false;
            m_reloadTime = 0f;
            m_ammoCount = m_maxAmmoCount;
        }
    }

}
