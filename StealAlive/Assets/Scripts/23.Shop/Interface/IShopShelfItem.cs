using UnityEngine;

public interface IShopShelfItem
{
    public void Init(ItemData itemData, IShopUIManager shopUIManager);

    int GetItemCategory();

    ItemData GetItem();

    int GetItemCode();
}
