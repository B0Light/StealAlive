using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    public CraftingRecipe recipe;
}

[System.Serializable]
public class CraftingRecipe
{
    public List<RecipeIngredient> ingredients;
    public ItemData resultItem;
    public int resultQuantity;
    public bool requiresExactPosition; // 위치가 중요한지 여부
}

[System.Serializable]
public class RecipeIngredient
{
    public ItemData itemData;
    public int quantity;
    public Vector2Int position; // 그리드 내 위치 (위치가 중요한 경우)
}