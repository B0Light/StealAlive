using System;
using UnityEngine;

public class AICharacterCombatManager_Range : AICharacterCombatManager
{
    [SerializeField] protected ProjectileType projectileType;
    [SerializeField] protected Transform firePoint;
    
    private void Start()
    {
        if (firePoint == null) firePoint = transform;
    }

    /* Animation Event */
    public virtual void FireProjectile()
    {
        // projectilePoolManager.FireAtTarget(projectileType, firePoint.position, currentTarget.transform, firePoint);
        UnifiedProjectilePoolManager.Instance.FireInDirection(aiCharacter, projectileType, firePoint.position, firePoint.forward, firePoint);
    }
}
