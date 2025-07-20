using UnityEngine;
using System.Collections.Generic;

public class ItemData : ScriptableObject
{
    [Header("Default Info")]
    public int itemCode;
    public string itemName;
    public Sprite itemIcon;
    public ItemTier itemTier;
    [TextArea] public string itemDescription;
    
    public int purchaseCost = 0;
    public List<int> costItemList;
    
    public int width = 1;
    public int height = 1;
    
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
            
        ItemData other = (ItemData)obj;
        return itemCode == other.itemCode;
    }

    public override int GetHashCode()
    {
        return itemCode.GetHashCode();
    }
}
