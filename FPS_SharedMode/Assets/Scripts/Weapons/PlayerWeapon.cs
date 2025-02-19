using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : Weapon
{

    private void Update()
    {
        if (HasStateAuthority && Input.GetMouseButtonDown(0))
        {
            m_shootPressed = true;
        }
    }

    public override void ShootBullet()
    {
        if(m_bullet == null)
        {
            return;
        }

        base.ShootBullet();
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

        GetComponentInParent<Movement>().InitKnockback();
    }

}
