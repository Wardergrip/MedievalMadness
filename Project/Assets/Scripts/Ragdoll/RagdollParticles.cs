using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollParticles : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private GameObject _dustObject;
    [SerializeField] private GameObject[] _fireParticles;

    // Player variables
    // ----------------
    public bool IsMovementInput
    {
        set
        {
            if (value)
            {
                if (_dustParticleSystem.isStopped) _dustParticleSystem.Play();
            }
            else
            {
                if (_dustParticleSystem.isPlaying) _dustParticleSystem.Stop();
            }
        }
    }

    public bool PassiveRagdollActivated
    {   
        set
        {
            if (value)
            {
                if (_dustParticleSystem.isPlaying) _dustParticleSystem.Stop();
            }
        }
    }

    // Particles
    // ---------
    ParticleSystem _dustParticleSystem;
    ParticleSystem[] _fireParticleSystem;

    // Start
    // -----
    void Start()
    {
        // Check Particles
        // -------------------
        Debug.Assert(_dustObject != null, "Error: no dustParticles found on the ragdoll");
        Debug.Assert(_fireParticles != null, "Error: no fireParticles found on the ragdoll");

        // Dust
        _dustParticleSystem = _dustObject.GetComponentInChildren<ParticleSystem>();
        _dustParticleSystem.Stop();

        // Fire
        _fireParticleSystem = new ParticleSystem[_fireParticles.Length];
        for (int idx = 0; idx < _fireParticles.Length; ++idx)
        {
            _fireParticleSystem[idx] = _fireParticles[idx].GetComponentInChildren<ParticleSystem>();
            _fireParticleSystem[idx].Stop();
        }
    }

    // Fire
    // ----
    public void OnFire()
    {
        // Loop through particles
        foreach (var fireParticle in _fireParticleSystem)
        {
            // If is playing, continue
            if (fireParticle.isPlaying) continue;

            // Else, play
            fireParticle.Play();
        }
    }
}
