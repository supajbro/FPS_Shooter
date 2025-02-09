using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePlatform : NetworkBehaviour
{

    private Collider m_col;
    private MeshRenderer m_renderer;
    private bool m_isEnabled = false;

    [Header("Toggle Values")]
    [SerializeField] private float m_toggleTime = 0.0f;
    [SerializeField] private float m_maxToggleTime = 5.0f;

    private void Awake()
    {
        m_col = GetComponent<Collider>();
        m_renderer = GetComponent<MeshRenderer>();
        m_isEnabled = true;
    }

    public override void FixedUpdateNetwork()
    {
        ToggleUpdate();
    }

    private void ToggleUpdate()
    {
        if(m_col == null || m_renderer == null)
        {
            Debug.LogError("Missing required components");
            return;
        }

        m_toggleTime += Runner.DeltaTime;
        if(m_toggleTime > m_maxToggleTime)
        {
            bool toggle = (m_isEnabled) ? false : true;
            RPC_Toggle(toggle);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Toggle(bool on)
    {
        m_renderer.enabled = on;
        m_col.enabled = on;
        m_toggleTime = 0.0f;
        m_isEnabled = on;
    }

}
