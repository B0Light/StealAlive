using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take BlockDamage")]
public class TakeBlockDamageEffect : TakeDamageEffect
{
    protected override void CalculateDamage(CharacterManager hitTarget)
    {
        physicalDamage *= (100 - hitTarget.characterStatsManager.blockingPhysicalAbsorption) / 100;
        magicalDamage *= (100 - hitTarget.characterStatsManager.blockingMagicalAbsorption) / 100;
        poiseDamage *= (100 - hitTarget.characterStatsManager.blockingStability) / 100;

        finalDamageDealt = Mathf.RoundToInt(physicalDamage + magicalDamage + extraDamage);
        finalPoiseDamage = poiseDamage * ((100 - hitTarget.characterStatsManager.passivePoise.Value) / 100);
        

        if (finalDamageDealt <= 0)
        {
            finalDamageDealt = 1;
        }
    }

    // Override the ProcessEffect method to apply damage via IDamageable interface
    public override void ProcessEffect(CharacterManager effectTarget)
    {
        if (effectTarget.characterVariableManager.isInvulnerable.Value) return;
        if (effectTarget.isDead.Value) return;

        // Apply damage using IDamageable
        if (effectTarget is IDamageable damageableTarget)
        {
            DamageData damageData = new DamageData
            {
                attacker = attacker,
                physicalDamage = finalDamageDealt,
                magicalDamage = magicalDamage,
                extraDamage = extraDamage,
                poiseDamage = finalPoiseDamage,
                contactPoint = contactPoint,
                angleHitFrom = angleHitFrom
            };

            damageableTarget.TakeDamage(damageData);
        }

        HandlePostHitEffects(effectTarget);
    }

    // Overridden method to play VFX when blocked damage occurs
    protected override void PlayDamageVfx(CharacterManager character)
    {
        character.characterEffectsManager.PlayBlockVFX(contactPoint);
    }

    // Overridden method to play SFX when blocked damage occurs
    protected override void PlayDamageSfx(CharacterManager character)
    {
        AudioClip blockSfx = WorldSoundFXManager.Instance.ChooseBlockSfx();

        if (blockSfx != null)
            character.characterSoundFXManager.PlaySoundFX(blockSfx);

        character.characterSoundFXManager.PlayDamageGruntSoundFX();
    }

    // Overridden method to handle directional animation for block damage
    protected override void PlayDirectionalBasedDamagedAnimation(CharacterManager character)
    {
        if (character.isDead.Value || character.isGroggy.Value) return;

        string damageAnimation;

        if ((145 <= angleHitFrom && angleHitFrom <= 180) || (-145 >= angleHitFrom && angleHitFrom >= -180))
        {
            // front
            damageAnimation = character.characterAnimatorManager.blockForward;
        }
        else if (-144 <= angleHitFrom && angleHitFrom <= -45)
        {
            // left
            damageAnimation = character.characterAnimatorManager.blockLeft;
        }
        else if (45 <= angleHitFrom && angleHitFrom <= 144)
        {
            // right
            damageAnimation = character.characterAnimatorManager.blockRight;
        }
        else
        {
            return;
        }

        character.characterAnimatorManager.lastDamageAnimationPlayed = damageAnimation;
        character.characterAnimatorManager.PlayTargetActionAnimation(damageAnimation, true);
    }
}
