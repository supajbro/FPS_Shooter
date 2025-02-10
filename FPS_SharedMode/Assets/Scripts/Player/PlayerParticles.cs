using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{

    [Header("Particles")]
    [SerializeField] private ParticleSystem m_knockbackParticle;
    [SerializeField] private PooledParticles m_groundStompParticle;

    public ParticleSystem KnockbackParticle => m_knockbackParticle;
    public PooledParticles GroundStompParticle => m_groundStompParticle;

    public void PlayParticle(ParticleSystem particle)
    {
        if (particle.isPlaying)
        {
            return;
        }

        particle.Play();
    }

    public void StopParticle(ParticleSystem particle)
    {
        if (!particle.isPlaying)
        {
            return;
        }

        particle.Stop();
    }

}

[System.Serializable]
public class PooledParticles
{
    public List<ParticleSystem> particles;
    public List<ParticleSystem> particlesInUse;
    public Transform parent;

    public void PlayParticle(Vector3 pos, Transform _parent)
    {
        if (particles.Count == 0 && particlesInUse.Count > 0)
        {
            // Recycle the oldest active particle
            particles.Add(particlesInUse[0]);
            particlesInUse.RemoveAt(0);
        }

        if (particles.Count == 0) return; // No available particles

        // Retrieve and activate a particle
        ParticleSystem particle = particles[0];
        particles.RemoveAt(0);
        particlesInUse.Add(particle);

        particle.transform.SetParent(_parent, false);
        particle.transform.position = pos;
        particle.Stop();
        particle.Play();
    }

}
