using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ShopCostItem : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image itemFrame;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCntText;
    public void Init(int itemDataID, int itemCount)
    {
        ItemInfo itemInfoData = WorldDatabase_Item.Instance.GetItemByID(itemDataID);
        itemIcon.sprite = itemInfoData.itemIcon;
        itemFrame.color = WorldDatabase_Item.Instance.GetItemColorByTier(itemInfoData.itemTier);

        if(itemNameText)
            itemNameText.text = itemInfoData.itemName;
        if(itemCntText)
            itemCntText.text = " x " + itemCount;
    }
}
