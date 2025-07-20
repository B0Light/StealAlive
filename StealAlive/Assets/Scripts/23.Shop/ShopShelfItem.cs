using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopShelfItem : MonoBehaviour, IShopShelfItem
{
    private ItemData _itemData;
    
    [Header("Item Info")] 
    [SerializeField] private int itemCode;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private Image itemTierBackground;
    
    [Header("Item Cost")]
    [SerializeField] private TextMeshProUGUI itemCost;

    [Header("BUY")] 
    [SerializeField] private Button itemButton;
    
    private IShopUIManager _playerUIShopManager;
    
    public virtual void Init(ItemData itemData, IShopUIManager shopUIManager)
    {
        this._itemData = itemData;
        itemCode = itemData.itemCode;
        ChangeSprite(itemIcon, itemData.itemIcon);
        itemName.text = itemData.itemName;
        itemName.color = WorldDatabase_Item.Instance.GetItemColorByTier(itemData.itemTier);
        itemTierBackground.color = WorldDatabase_Item.Instance.GetItemBackgroundColorByTier(itemData.itemTier);

        itemCost.text = itemData.purchaseCost.ToString();
        
        itemButton.onClick.AddListener(SelectThisItem);
        _playerUIShopManager = shopUIManager;
    }

    private void ChangeSprite(Image uiImage, Sprite newSprite)
    {
        if (uiImage == null || newSprite == null)
        {
            if(uiImage == null)
                Debug.LogError("NO UI IMAGE");
            else
                Debug.LogError("NO SPRITE");
            return;
        }

        // 기존 RectTransform 참조
        RectTransform rectTransform = uiImage.GetComponent<RectTransform>();

        // 새로운 스프라이트의 크기 가져오기
        Vector2 newSpriteSize = new Vector2(newSprite.rect.width, newSprite.rect.height);

        // 현재 RectTransform의 너비에 기반해 비율 유지
        float aspectRatio = newSpriteSize.y / newSpriteSize.x;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x * aspectRatio);

        // 스프라이트 교체
        uiImage.sprite = newSprite;
    }
    
    private void SelectThisItem() => _playerUIShopManager.SelectItemToBuy(_itemData);
    
    public int GetItemCode() => itemCode;

    public ItemData GetItem() => _itemData;

    public virtual int GetItemCategory() => 0;
}
