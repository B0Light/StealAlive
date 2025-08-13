using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public abstract class DamageLogic : MonoBehaviour
{
    [HideInInspector] public CharacterManager ownerCharacter;

    [HideInInspector] public float physicalDamage = 0;
    [HideInInspector] public float magicalDamage = 0;
    [HideInInspector] public float poiseDamage = 0;

    [Header("Contact Point")]
    protected Vector3 contactPoint;
    
    [Header("BLOCK")]
    protected float _dotValueFromAttackToDamageTarget;

    [Header("Characters Damaged")]
    protected readonly List<CharacterManager> charactersDamaged = new List<CharacterManager>();

    protected virtual void Awake()
    {
        ownerCharacter = GetComponentInParent<CharacterManager>();
    }

    protected void SetBlockingDotValues(CharacterManager damageTarget)
    {
        if (damageTarget == null || ownerCharacter == null) return;
        
        Vector3 directionFromAttackToDamageTarget = ownerCharacter.transform.position - damageTarget.transform.position;
        _dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
    }
    
    protected bool CheckForParried(CharacterManager damageTarget)
    {
        if (_dotValueFromAttackToDamageTarget < 0.3f) return false;
        // 패링했는지 확인
        if (damageTarget.characterVariableManager.isParring.Value == false) return false;
        
        charactersDamaged.Add(damageTarget);
        
        WorldCharacterEffectsManager.Instance.OnParrySuccess(contactPoint);
        damageTarget.characterVariableManager.SuccessParry(ownerCharacter); // 무적효과 
        damageTarget.characterAnimatorManager.PlayTargetActionAnimation("Parry", true);
        damageTarget.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.Instance.ChooseParriedSfx());
        return true;
    }
    
    protected bool CheckForBlock(CharacterManager damageTarget)
    {
        // 블록했는지 확인
        if (damageTarget.characterVariableManager.isBlock.Value == false) return false;
        // 블록이 가능한지 확인
        if (_dotValueFromAttackToDamageTarget < 0.5f) return false;
        // 블록할 액션포인트가 있는지 확인 -> 있음 
        if (damageTarget.characterStatsManager.UseActionPoint()) return true;
        // 없음 -> 블록 실패 
        damageTarget.characterAnimatorManager.PlayTargetActionAnimation("Break", true);
        return false;
    }

    protected void DamageTarget(CharacterManager damageTarget, bool isBlock = false)
    {
        if (charactersDamaged.Contains(damageTarget)) return;
        charactersDamaged.Add(damageTarget);

        // bkTools 경량 데미지로 즉시 처리 + 흡수/크리/블록 보정
        var targetDamageable = damageTarget.GetComponent<bkTools.Damageable>();
        if (targetDamageable == null) targetDamageable = damageTarget.gameObject.AddComponent<bkTools.Damageable>();

        // 기본 합산 피해 + 공격자 추가 피해
        float baseExtra = ownerCharacter?.characterStatsManager?.extraDamage?.Value ?? 0f;

        // 흡수율 계산 (블록 시 블록 흡수, 아니면 기본+추가 흡수)
        float physAbs = 0f, magAbs = 0f;
        if (damageTarget?.characterStatsManager != null)
        {
            if (isBlock)
            {
                physAbs = damageTarget.characterStatsManager.blockingPhysicalAbsorption;
                magAbs = damageTarget.characterStatsManager.blockingMagicalAbsorption;
            }
            else
            {
                physAbs = damageTarget.characterStatsManager.basePhysicalAbsorption + damageTarget.characterStatsManager.extraPhysicalAbsorption;
                magAbs = damageTarget.characterStatsManager.baseMagicalAbsorption + damageTarget.characterStatsManager.extraMagicalAbsorption;
            }
            physAbs = Mathf.Clamp(physAbs, 0f, 100f);
            magAbs = Mathf.Clamp(magAbs, 0f, 100f);
        }

        float reducedPhys = physicalDamage * (100f - physAbs) / 100f;
        float reducedMag = magicalDamage * (100f - magAbs) / 100f;
        float finalAmount = Mathf.Max(0f, reducedPhys + reducedMag + baseExtra);

        // 크리티컬 여부
        bool isCritical = false;
        var attackerCombat = ownerCharacter?.characterCombatManager;
        if (attackerCombat != null)
        {
            isCritical = attackerCombat.canCriticalAttack || attackerCombat.currentAttackType == AttackType.CriticalAttack;
        }

        Vector3 dir = (ownerCharacter != null) ? (damageTarget.transform.position - ownerCharacter.transform.position) : Vector3.zero;
        var info = new bkTools.DamageInfo(
            finalAmount,
            dir,
            contactPoint,
            ownerCharacter ? ownerCharacter.gameObject : gameObject,
            isCritical
        );

        ModifySimpleDamage(ref info, isBlock);
        targetDamageable.ReceiveDamage(info);

        // 블록 VFX/SFX
        if (isBlock)
        {
            damageTarget?.characterEffectsManager?.PlayBlockVFX(contactPoint);
            damageTarget?.characterSoundFXManager?.PlayBlockSoundFX();
        }
    }

    // 필요 시 파생 클래스에서 간단한 보정(예: 블록 시 절반 대미지 등)
    protected virtual void ModifySimpleDamage(ref bkTools.DamageInfo info, bool isBlock)
    {
        // 블록 보정: 추가 절반 경감
        if (isBlock) info.amount *= 0.5f;
        // 크리티컬 보정
        if (info.isCritical) info.amount *= 1.5f;
    }

    protected virtual void ModifyDamageEffect(TakeDamageEffect damageEffect) { }
}