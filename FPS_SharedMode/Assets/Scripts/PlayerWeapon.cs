using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{

    public GameObject weaponModel;
    public GameObject bullet;

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
        }
    }

    public void UpdateWeaponPosition(Camera cam)
    {
        Quaternion camRotY = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, 0, 0);
        Debug.Log("CAM: " + camRotY);
        weaponModel.transform.rotation = camRotY;
    }

}
