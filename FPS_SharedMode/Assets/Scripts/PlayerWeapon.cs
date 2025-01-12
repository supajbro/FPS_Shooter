using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{

    [SerializeField] private NetworkPrefabRef m_bulletPrefab;
    [SerializeField] private Transform m_bulletSpawnPoint;

    private bool m_shootPressed;

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
                // Initialize bullet properties after spawning
                obj.GetComponent<Bullet>().Init(Camera.main.transform.forward);
            });
    }

}
