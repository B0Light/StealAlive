using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForgeGUIManager : GUIComponent
{
    private int _girdWidth = 6;
    private int _gridHeight = 5;
    
    private List<CraftingRecipeData> recipes;
    [SerializeField] private ItemGrid craftingGrid;
    [SerializeField] private ItemGrid previewGrid;
    
    private System.Action<int> onValueChangedHandler;
    
    public void InitForge()
    {
        craftingGrid.SetGrid(_girdWidth, _gridHeight, null);
        previewGrid.SetGrid(6, 3, null);
        LoadRecipe();
        onValueChangedHandler = i => UpdateCraftingGridUI();
        craftingGrid.totalItemValue.OnValueChanged += onValueChangedHandler;
    }
    
    private void LoadRecipe()
    {
        recipes = new List<CraftingRecipeData>();
        CraftingRecipeData[] recipeData = Resources.LoadAll<CraftingRecipeData>("Crafting");

        foreach (var recipe in recipeData)
        {
            recipes.Add(recipe);
        }
    }
    
    public override void CloseGUI()
    {
        if (onValueChangedHandler != null)
        {
            craftingGrid.totalItemValue.OnValueChanged -= onValueChangedHandler;
            onValueChangedHandler = null;
        }
        base.CloseGUI();
    }

    public void TryCraft()
    {
        var currentItemDict = craftingGrid.GetCurItemDictById();
        var matchedRecipe = FindMatchingRecipe(currentItemDict);
        
        if (matchedRecipe != null)
        {
            ExecuteCrafting(matchedRecipe);
        }
    }
    
    private CraftingRecipe FindMatchingRecipe(Dictionary<int,int> items)
    {
        var sortedRecipes = recipes.OrderByDescending(r => r.recipe.ingredients.Count);

        foreach (var recipeData in sortedRecipes)
        {
            if (CheckIngredientCount(items, recipeData.recipe))
            {
                return recipeData.recipe;
            }
        }
        return null;
    }
    
    // 레시피 매칭 
    private bool CheckIngredientCount(Dictionary<int,int> gridItems, CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!gridItems.ContainsKey(ingredient.itemData.itemCode) || 
                gridItems[ingredient.itemData.itemCode] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }
    
    // 조합 로직 
    private void ExecuteCrafting(CraftingRecipe recipe)
    {
        UpdateCraftingGridUI();
        // 1. 재료 아이템들 제거
        RemoveIngredients(recipe);
    
        // 2. 결과 아이템 생성
        CreateResultItem(recipe);
    
        // 3. UI 업데이트
        UpdateCraftingGridUI();
    }

    private void RemoveIngredients(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            craftingGrid.RemoveItemById(ingredient.itemData.itemCode, ingredient.quantity);
        }
    }

    private void CreateResultItem(CraftingRecipe recipe)
    {
        craftingGrid.AddItemById(recipe.resultItem.itemCode,recipe.resultQuantity, false);
    }

    private void UpdateCraftingGridUI()
    {
        var currentItemDict = craftingGrid.GetCurItemDictById();
        var matchedRecipe = FindMatchingRecipe(currentItemDict);
        
        previewGrid.ResetItemGrid();
        if (matchedRecipe != null)
        {
            previewGrid.AddItemById(matchedRecipe.resultItem.itemCode, matchedRecipe.resultQuantity);
        }
    }
    
    public ItemGrid GetItemGrid => craftingGrid;
}
