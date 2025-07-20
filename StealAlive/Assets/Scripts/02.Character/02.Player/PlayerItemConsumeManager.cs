using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemConsumeManager : MonoBehaviour
{
    private PlayerManager _playerManager;
    private Dictionary<ItemEffect, Action<ItemInfoConsumable, ItemAbility>> _effectHandlers;

    private void Awake()
    {
        _playerManager = GetComponent<PlayerManager>();
        InitializeEffectHandlers();
    }

    private void InitializeEffectHandlers()
    {
        _effectHandlers = new Dictionary<ItemEffect, Action<ItemInfoConsumable, ItemAbility>>()
        {
            { ItemEffect.RestoreHealth, HandleRestoreHealthEffect },
            { ItemEffect.EatingFood, HandleEatingFoodEffect },
            { ItemEffect.BuffAttack, HandleBuffAttackEffect },
            { ItemEffect.BuffDefense, HandleBuffDefenseEffect },
            { ItemEffect.UtilitySpeed, HandleUtilitySpeedEffect },
            { ItemEffect.UtilityWeight, HandleUtilityWeightEffect }
        };
    }

    public void UseQuickSlotItem()
    {
        int useItemID = _playerManager.playerVariableManager.currentSelectQuickSlotItem.Value;
        if (useItemID == 0) return;
        
        RemoveItemFromInventory(useItemID);
        UseItem(useItemID); 
    }

    private void RemoveItemFromInventory(int itemID)
    {
        // UI에서 삭제
        WorldPlayerInventory.Instance.GetConsumableInventory().RemoveItemAtGrid(itemID);
        // 변수 매니저에서 삭제 - 데이터 삭제
        _playerManager.playerVariableManager.currentQuickSlotIDList.Remove(itemID); 
    }

    public void UseItem(int itemCode)
    {
        var itemInfo = GetConsumableItemInfo(itemCode);
        if (itemInfo == null) return;
        
        ProcessEffects(itemInfo);
    }

    private ItemInfoConsumable GetConsumableItemInfo(int itemCode)
    {
        ItemInfo itemInfo = WorldDatabase_Item.Instance.GetItemByID(itemCode);
        return itemInfo as ItemInfoConsumable;
    }
    

    private void ProcessEffects(ItemInfoConsumable itemInfo)
    {
        if (itemInfo.itemAbilities == null) return;
        
        foreach (var ability in itemInfo.itemAbilities)
        {
            ProcessEffect(ability.itemEffect, itemInfo, ability);
        }
    }

    private void ProcessEffect(ItemEffect effect, ItemInfoConsumable itemInfo, ItemAbility ability = null)
    {
        if (_effectHandlers.TryGetValue(effect, out var handler))
        {
            handler(itemInfo, ability);
        }
        else
        {
            Debug.LogWarning($"처리되지 않는 아이템 효과: {effect}");
        }
    }

    #region Effect Handlers

    private void HandleRestoreHealthEffect(ItemInfoConsumable itemInfo, ItemAbility ability)
    {
        var restoreHealthEffect = Instantiate(WorldCharacterEffectsManager.Instance.restoreHealthEffect);
        
        // ability가 있으면 해당 값을 사용, 없으면 itemInfo 값 사용
        var immediateValue = ability?.value ?? itemInfo.immediateEffectValue;
        var continuousValue = itemInfo.continuousEffectValue;
        var duration = itemInfo.continuousEffectDuration;
        
        restoreHealthEffect.SetHealAmount(immediateValue, continuousValue, duration);
        _playerManager.characterEffectsManager.ProcessInstantEffect(restoreHealthEffect);
    }

    private void HandleEatingFoodEffect(ItemInfoConsumable itemInfo, ItemAbility ability)
    {
        var eatingFoodEffect = Instantiate(WorldCharacterEffectsManager.Instance.eatingFoodEffectEffect);
        
        var immediateValue = ability?.value ?? itemInfo.immediateEffectValue;
        eatingFoodEffect.SetFoodAmount(immediateValue);
        _playerManager.characterEffectsManager.ProcessInstantEffect(eatingFoodEffect);
    }

    private void HandleBuffAttackEffect(ItemInfoConsumable itemInfo, ItemAbility ability)
    {
        // TODO: 공격력 버프 효과 구현
        Debug.Log("공격력 버프 효과 처리 예정");
    }

    private void HandleBuffDefenseEffect(ItemInfoConsumable itemInfo, ItemAbility ability)
    {
        // TODO: 방어력 버프 효과 구현
        Debug.Log("방어력 버프 효과 처리 예정");
    }

    private void HandleUtilitySpeedEffect(ItemInfoConsumable itemInfo, ItemAbility ability)
    {
        // TODO: 속도 유틸리티 효과 구현
        Debug.Log("속도 유틸리티 효과 처리 예정");
    }

    private void HandleUtilityWeightEffect(ItemInfoConsumable itemInfo, ItemAbility ability)
    {
        // TODO: 무게 유틸리티 효과 구현
        Debug.Log("무게 유틸리티 효과 처리 예정");
    }

    #endregion
}