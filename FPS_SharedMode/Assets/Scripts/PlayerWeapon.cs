using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{

    public GameObject weaponModel;

    [SerializeField] private NetworkPrefabRef m_bulletPrefab;
    [SerializeField] private Transform m_bulletSpawnPoint;
    [SerializeField] private float m_bulletSpeed = 20f;

    [SerializeField] private Transform m_spawnPos;
    public Transform WeaponSpawnPos { get { return m_spawnPos; } }

    private bool m_shootPressed;

    public void SpawnWeapon()
    {
        Runner.Spawn(weaponModel, transform.position, Quaternion.identity);
    }

    public override void Spawned()
    {
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
        if (m_shootPressed)
        {
            m_shootPressed = false;
            Debug.Log("Has shot bullet");
            // Spawn the bullet
            SpawnBullet();
        }
    }

    NetworkObject bullet;
    private void SpawnBullet()
    {
        if (HasStateAuthority)
        {
            // Spawn the bullet at the spawn point with the correct rotation
            bullet = Runner.Spawn(
                m_bulletPrefab,
                m_bulletSpawnPoint.position,
                m_bulletSpawnPoint.rotation,
                Object.InputAuthority, // Owner of the bullet (optional)
                (runner, obj) =>
                {
                    // Initialize bullet properties after spawning
                    //obj.GetComponent<Bullet>().Initialize(m_bulletSpawnPoint.forward * m_bulletSpeed);
                });

            RPC_SetInitialBulletValues();

            Debug.Log("Bullet spawned by server");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetInitialBulletValues()
    {
        bullet.transform.position = m_bulletSpawnPoint.position;
        bullet.transform.rotation = m_bulletSpawnPoint.rotation;
    }

    public void UpdateWeaponPosition(Camera cam)
    {
        Quaternion camRotY = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, 0, 0);
        Debug.Log("CAM: " + camRotY);
        weaponModel.transform.rotation = camRotY;
    }

}
