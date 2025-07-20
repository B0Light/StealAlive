using UnityEngine;

public class DamageCollider : DamageLogic
{
    protected Collider damageCollider;

    protected override void Awake()
    {
        base.Awake();
        damageCollider = GetComponent<Collider>();
    }
    
    protected virtual void Start()
    {
        damageCollider.enabled = false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();
        if(!damageTarget) return;
        contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        
        if(ownerCharacter.characterGroup == damageTarget.characterGroup) return;
        
        SetBlockingDotValues(damageTarget);
        if(CheckForParried(damageTarget)) return;
        
        DamageTarget(damageTarget, CheckForBlock(damageTarget));
    }
    
    #region AnimationEvent
    public virtual void EnableDamageCollider()
    {
        charactersDamaged.Clear();
        damageCollider.enabled = true;
    }

    public virtual void DisableDamageCollider()
    {
        damageCollider.enabled = false;
        charactersDamaged.Clear();
    }
    #endregion
}