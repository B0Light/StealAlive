using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopShelfItem_Item : ShopShelfItem
{
    private ItemInfo _itemInfo;
    
    [Header("Item Cost")]
    [SerializeField] private GameObject costItemSlot;
    [SerializeField] private GameObject costItemPrefab;
    [SerializeField] private GameObject costCashSlot;

    public override void Init(ItemData itemData, IShopUIManager shopUIManager)
    {
        _itemInfo = itemData as ItemInfo;
        
        costItemSlot.SetActive(_itemInfo.purChaseWithItem);
        costCashSlot.SetActive(!_itemInfo.purChaseWithItem);

        // 해당 아이템이 아이템 교환으로 거래된다면 해당 아이템의 아이콘을 띄운다 
        if (_itemInfo.purChaseWithItem)
        {
            foreach (int costItemID in itemData.costItemList)
            {
                GameObject spawnedCostItem = Instantiate(costItemPrefab, costItemSlot.transform);
                spawnedCostItem.GetComponent<ShopCostItem>()?.Init(costItemID);
            }
        }
        
        base.Init(itemData, shopUIManager);
    }
    
    public override int GetItemCategory() => (int)_itemInfo.itemType;
    
}
