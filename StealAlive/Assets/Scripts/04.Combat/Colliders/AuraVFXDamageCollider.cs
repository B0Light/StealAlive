using System.Collections.Generic;
using UnityEngine;

public class AuraVFXDamageCollider : DamageCollider
{
    [Header("Claw VFX Settings")]
    [SerializeField] private float activeTime = 0.5f;
    private List<ParticleSystem> _particleSystems = new List<ParticleSystem>();

    private List<CombatSFX> _combatSfxList = new List<CombatSFX>();

    protected override void Start()
    {
        base.Start();
        
        _particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>());
        _combatSfxList.AddRange(GetComponentsInChildren<CombatSFX>());
    }

    public override void EnableDamageCollider()
    {
        base.EnableDamageCollider();
        PlayAllParticles();
        
        Invoke(nameof(SelfDisable), activeTime);
    }
    
    private void SelfDisable()
    {
        DisableDamageCollider();
        StopAllParticles();
    }
    
    private void PlayAllParticles()
    {
        //Debug.Log("particleSystem : " + _particleSystems.Count);
        foreach (ParticleSystem particle in _particleSystems)
        {
            if (particle != null)
                particle.Play();
        }
        
        foreach (var sfx in _combatSfxList)
        {
            if(sfx != null)
                sfx.PlaySfxWithDelay();
        }
    }
    
    private void StopAllParticles()
    {
        foreach (ParticleSystem particle in _particleSystems)
        {
            if (particle != null)
                particle.Stop();
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }
}