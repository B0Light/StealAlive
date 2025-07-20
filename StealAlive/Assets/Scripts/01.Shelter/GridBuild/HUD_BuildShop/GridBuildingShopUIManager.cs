using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class GridBuildingShopUIManager : ShopUIManager
{
    /*
     * costItemSlot; // to build
     * costCashSlot; // Licence
     */
    [SerializeField] private Button itemBuyButton;
    
    [SerializeField] private GridBuildingUI gridBuildingUI;
    public override void OpenShop(List<int> itemIds, Interactable interactable = null)
    {
        SetUpShelf(itemIds);
        ResetItemInfo();
    }
    
    private void SetUpShelf(List<int> itemIds)
    {
        ResetShelf();
        notEnoughItemsComment.SetActive(false);
        notEnoughSlot.SetActive(false);
        foreach (int itemId in itemIds)
        {
            BuildObjData itemData = WorldDatabase_Build.Instance.GetBuildingByID(itemId);
            GameObject saleItem = Instantiate(itemProductPrefab, productContainer);
            IShopShelfItem shelfItemProduct = saleItem.GetComponent<IShopShelfItem>();
            if(shelfItemProduct != null)
                shelfItemProduct.Init(itemData, this);
            onSaleItems.Add(shelfItemProduct);
        }
    }
    
    protected override void ResetItemInfo()
    {
        base.ResetItemInfo();
        itemBuyButton.gameObject.SetActive(false);
        notEnoughItemsComment.SetActive(false);
        notEnoughSlot.SetActive(false);
    }
    
    public override void SelectItemToBuy(ItemData item)
    {
        BuildObjData buildObjData = item as BuildObjData;
        if(!buildObjData) return;
        SetItemInfo(buildObjData);
        costItemSlot.SetActive(true);
        costCashSlot.SetActive(true);
        itemBuyButton.gameObject.SetActive(true);
        notEnoughItemsComment.SetActive(false);
        notEnoughSlot.SetActive(false);
        DeleteAllChildren(costItemSpawnSlot);
        foreach (int costItemID in item.costItemList)
        {
            GameObject spawnedCostItem = Instantiate(costItemPrefab, costItemSpawnSlot);
                
            spawnedCostItem.GetComponent<ShopCostItem>()?.Init(costItemID);
        }
        SetBuildInfoText(buildObjData);
        SetGridInfo(buildObjData);
        itemCostText.text = buildObjData.purchaseCost.ToString();
        itemBuyButton.onClick.RemoveAllListeners();
        itemBuyButton.onClick.AddListener(()=>RegisterBuilding(buildObjData));
    }

    private void RegisterBuilding(BuildObjData buildObjData)
    {
        if (WorldPlayerInventory.Instance.TrySpend(buildObjData.purchaseCost))
        {
            BuildingManager.Instance.RegisterBuilding(buildObjData);
        
            Debug.Log("RegisterBuilding" + selectProduct.GetItem().itemName);
            BuyItemProcess();
        }
        else
        {
            notEnoughItemsComment.SetActive(true);
        }
    }
    
    private void BuyItemProcess()
    {
        if(selectProduct == null) return;
        //진열대에서 상품을 제거
        onSaleItems.Remove(selectProduct);
        Destroy(((MonoBehaviour)selectProduct)?.gameObject);
        ResetItemInfo();
    }

    private void SetBuildInfoText(BuildObjData data)
    {
        string infoText = $"[상세 정보]\n\n기본 요금 : {data.baseFee}\n\n최대 레벨 : {data.maxLevel}";
        
        itemInfo_Description.text = infoText;
    }

    private void SetGridInfo(BuildObjData data)
    {
        PlacedObject placedObject = data.prefab.gameObject.GetComponent<PlacedObject>();
        
        if(placedObject == null) return;
        
        gridBuildingUI.SetGridLayer(data.width, data.height, placedObject.entrancePos, placedObject.exitDir, placedObject.exitPos);
    }
}
