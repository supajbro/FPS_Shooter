using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : NetworkBehaviour
{

    [SerializeField] private ParticleSystem m_particle;
    [SerializeField] private AudioSource m_audio;

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
        PlayParticle();
        PlaySound();
    }

    private void PlayParticle()
    {
        if (m_particle == null)
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

    private void PlaySound()
    {
        if (m_audio == null)
        {
            return;
        }

        if (m_audio.isPlaying)
        {
            return;
        }

        m_audio.Play();
    }

}
