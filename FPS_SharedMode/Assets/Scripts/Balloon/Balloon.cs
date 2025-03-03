using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : NetworkBehaviour
{

    [SerializeField] private ParticleSystem m_particle;

    private void Start()
    {
        if(m_particle == null)
        {
            return;
        }

        m_particle.transform.parent = null;
    }

    public void PopBalloon()
    {
        if(m_particle == null)
        {
            return;
        }

        if (m_particle.isPlaying)
        {
            return;
        }

        m_particle.transform.position = transform.position;
        m_particle.Play();
    }

}
