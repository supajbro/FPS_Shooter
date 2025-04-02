using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEnvironmentSpawner : NetworkBehaviour
{

    public GameObject ground;
    private Vector3 m_initScale = Vector3.zero;
    private Vector3 m_newScale = Vector3.zero;

    //public void SpawnNetworkedEnvironments()
    //{
    //    {
    //        Runner.Spawn(ground);
    //        m_initScale = transform.localScale;
    //        m_newScale = m_initScale;
    //    }
    //}

    public override void Spawned()
    {
        base.Spawned();

        m_initScale = transform.localScale;
        m_newScale = m_initScale;
    }

    public override void FixedUpdateNetwork()
    {
        //if (!HasStateAuthority)
        //{
        //    return;
        //}

        base.FixedUpdateNetwork();

        bool restart = true;
        foreach (var player in GameManager.instance.GetAllPlayers())
        {
            if (!player.IsDead)
            {
                restart = false;
                break;
            }
        }

        if (!restart)
        {
            const float ScaleSpeed = 0.75f;
            //const float ScaleSpeed = 5.0f;

            m_newScale.x = (m_newScale.x > 0.0f) ? m_newScale.x - (Runner.DeltaTime * ScaleSpeed) : 0.0f;
            m_newScale.z = (m_newScale.z > 0.0f) ? m_newScale.z - (Runner.DeltaTime * ScaleSpeed) : 0.0f;
            transform.localScale = m_newScale;
        }
        else
        {
            transform.localScale = m_initScale;
            m_newScale = m_initScale;
        }
    }
}
