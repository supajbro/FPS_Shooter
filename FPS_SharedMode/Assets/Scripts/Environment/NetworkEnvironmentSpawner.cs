using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEnvironmentSpawner : NetworkBehaviour
{

    public GameObject ground;
    private NetworkObject m_ground;
    private Vector3 m_initScale = Vector3.zero;
    private Vector3 m_newScale = Vector3.zero;

    public void SpawnNetworkedEnvironments()
    {
        base.Spawned();

        if(HasStateAuthority && GameManager.instance.GetLocalPlayer().Boss)
        {
            m_ground = Runner.Spawn(ground);
            m_initScale = m_ground.transform.localScale;
            m_newScale = m_initScale;
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (m_ground != null)
        {
            const float ScaleSpeed = 0.75f;

            m_newScale.x = (m_newScale.x > 0.0f) ? m_newScale.x - (Runner.DeltaTime * ScaleSpeed) : 0.0f;
            m_newScale.z = (m_newScale.z > 0.0f) ? m_newScale.z - (Runner.DeltaTime * ScaleSpeed) : 0.0f;
            m_ground.transform.localScale = m_newScale;
        }
    }
}
