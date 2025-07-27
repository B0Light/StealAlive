using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EffectPlayer))]
public class AuraVFXDamageCollider : DamageCollider
{
    private EffectPlayer _effectPlayer;

    protected override void Start()
    {
        base.Start();
        _effectPlayer = GetComponent<EffectPlayer>();
        _effectPlayer.SetResource();
    }

    public override void EnableDamageCollider()
    {
        base.EnableDamageCollider();
        _effectPlayer.PlayAllParticles();
    }

    public void EnableDamageColliderAfterDelay(float delayTime)
    {
        // 먼저 이펙트를 실행
        _effectPlayer.PlayAllParticles();
        
        // delayTime 후에 DamageCollider 활성화
        Invoke(nameof(EnableDamageColliderDelayed), delayTime);
    }
    
    private void EnableDamageColliderDelayed()
    {
        // DamageCollider만 활성화 (이펙트는 이미 실행됨)
        base.EnableDamageCollider();
    }
    
}