using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;

public class CharacterCombatManager : MonoBehaviour
{
    protected CharacterManager character;

    [Header("Last Attack Anim Performed")]
    [ReadOnly] public string lastAttackAnimationPerformed;

    [Header("Attack Target")]
    [ReadOnly] public CharacterManager currentTarget;

    [Header("Attack Type")]
    [ReadOnly] public AttackType currentAttackType;

    [Header("Lock On Transform")]
    [ReadOnly] public Transform lockOnTransform;

    [ReadOnly] public CharacterManager criticalDamagedCharacter;
    [ReadOnly] public bool canCriticalAttack = false;
        
    [Header("Attack Flag")]
    [ReadOnly] public bool canPerformRollingAttack = false;
    [ReadOnly] public bool canPerformBackStepAttack = false;
    [ReadOnly] public bool canPerformJumpingAttack = false;
    
    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public virtual void SetTarget(CharacterManager newTarget)
    {
        if(currentTarget == newTarget) return;
        
        if(newTarget != null)
        {
            currentTarget = newTarget;
            character.characterVariableManager.CLVM.isSprinting = true;
            Debug.Log("SET TARGET : " + newTarget.name);
        }
        else
        {
            currentTarget = null;
            character.characterVariableManager.CLVM.isSprinting = false;
            Debug.Log("RESET TARGET");
        }
    }
    
    public void ReactToSound(CharacterManager source)
    {
        currentTarget = source;
    }
    public void EnableIsInvulnerable()
    {
        character.characterVariableManager.isInvulnerable.Value = true;
    }

    public void DisableIsInvulnerable()
    {
        character.characterVariableManager.isInvulnerable.Value = false;
    }
    
    public virtual void EnableCanDoCombo()
    {
       
    }

    public virtual void DisableCanDoCombo()
    {

    }
    
    public void EnableCanDoRollingAttack()
    {
        canPerformRollingAttack = true;
    }
    
    public void DisableCanDoRollingAttack()
    {
        canPerformRollingAttack = false;
    }
    
    public void EnableCanDoBeckStepAttack()
    {
        canPerformBackStepAttack = true;
    }
    
    public void DisableCanDoBeckStepAttack()
    {
        canPerformBackStepAttack = false;
    }
    
    public void EnableCanDoJumpingAttack()
    {
        Debug.Log("Can Jump Attack");
        canPerformJumpingAttack = true;
    }
    
    public void DisableCanDoJumpingAttack()
    {
        canPerformJumpingAttack = false;
    }
}
