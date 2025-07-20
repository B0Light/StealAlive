public class ShopShelfItem_Building : ShopShelfItem
{
    private BuildObjData _buildObjData;

    public override void Init(ItemData itemData, IShopUIManager shopUIManager)
    {
        _buildObjData = itemData as BuildObjData;
        base.Init(itemData, shopUIManager);
    }
    
    public override int GetItemCategory() => (int)_buildObjData.GetTileCategory();
}
