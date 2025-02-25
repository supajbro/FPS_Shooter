using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBullet : Bullet
{

    public override void Update()
    {
    }

    public override void FixedUpdateNetwork()
    {
        if (m_direction != Vector3.zero)
        {
            transform.localPosition += m_direction * m_speed * Runner.DeltaTime;
        }

        m_lifetime = (m_direction != Vector3.zero) ? m_lifetime - Runner.DeltaTime : 3.0f;
        if (m_lifetime <= 0.0f)
        {
            Runner.Despawn(m_bullet);
        }
    }

}
