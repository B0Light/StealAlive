using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ShopCostItem : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image itemFrame;
    
    public void Init(int itemDataID)
    {
        ItemInfo itemInfoData = WorldDatabase_Item.Instance.GetItemByID(itemDataID);
        itemIcon.sprite = itemInfoData.itemIcon;
        itemFrame.color = WorldDatabase_Item.Instance.GetItemColorByTier(itemInfoData.itemTier);
    }
}
