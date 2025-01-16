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
    public int AmmoCount { get { return m_ammoCount; } }
    public int MaxAmmoCount { get { return m_maxAmmoCount; } }

    [Header("Reload")]
    private bool m_reloading = false;
    private float m_reloadTime = 0f;
    private float m_maxReloadTime = 1f;

    private void Awake()
    {
        m_reloading = true;
    }

    public override void Spawned()
    {
        m_ammoCount = m_maxAmmoCount;
        Reload();
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
            ShootBullet();
        }

        if(m_ammoCount == 0)
        {
            m_reloading = true;
        }

        Reload();
    }

    private void ShootBullet()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        RaycastHit hit;
        Vector3 targetPoint;
        const float maxShootDistance = 100f;

        if (Physics.Raycast(ray, out hit, maxShootDistance))
        {
            // If the ray hits something, use the hit point
            targetPoint = hit.point;
        }
        else
        {
            // If nothing is hit, shoot forward from the camera
            targetPoint = ray.origin + ray.direction * maxShootDistance;
        }

        Vector3 shootDirection = (targetPoint - m_bulletSpawnPoint.position).normalized;

        m_bullet.transform.parent = null;
        m_bullet.GetComponent<Bullet>().Init(shootDirection/*Camera.main.transform.forward*/); // Amy I love you xx
        m_bullet = null;
        m_ammoCount--;
    }

    NetworkObject m_bullet;
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
        if(m_reloadTime >= m_maxReloadTime)
        {
            m_reloading = false;
            m_reloadTime = 0f;
            m_ammoCount = m_maxAmmoCount;
            SpawnBullet();
        }
    }

}
