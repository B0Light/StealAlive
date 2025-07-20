using System;
using UnityEngine;
using UnityEngine.Serialization;

public class AICharacterCombatManager_Melee_Aura : AICharacterCombatManager_Melee
{
    [SerializeField] private AuraVFXDamageCollider auraVFXDamageCollider01;
    [SerializeField] private AuraVFXDamageCollider auraVFXDamageCollider02;

    [SerializeField] private float auraAttack01Mod = 1.1f;
    [SerializeField] private float auraAttack02Mod = 1.3f;
    private void Start()
    {
        if(auraVFXDamageCollider01)
            SetDamageAura01();
        if (auraVFXDamageCollider02)
            SetDamageAura02();
    }

    public override void OpenLeftHandDamageCollider()
    {
        base.OpenLeftHandDamageCollider();

        AttackAura01();
    }

    public override void OpenRightHandDamageCollider()
    {
        base.OpenRightHandDamageCollider();

        AttackAura02();
    }

    private void SetDamageAura01()
    {
        auraVFXDamageCollider01.physicalDamage = baseDamage * auraAttack01Mod;
        auraVFXDamageCollider01.poiseDamage = 20;
    }
    
    private void SetDamageAura02()
    {
        auraVFXDamageCollider02.physicalDamage = baseDamage * auraAttack02Mod;
        auraVFXDamageCollider02.poiseDamage = 20;
    }
    
    public void AttackAura01()
    {
        if(auraVFXDamageCollider01 == null) return;
        auraVFXDamageCollider01.ownerCharacter = character; 
        auraVFXDamageCollider01.EnableDamageCollider(); 
    }
    
    public void AttackAura02()
    {
        if(auraVFXDamageCollider02 == null) return;
        auraVFXDamageCollider02.ownerCharacter = character; 
        auraVFXDamageCollider02.EnableDamageCollider(); 
    }
}
