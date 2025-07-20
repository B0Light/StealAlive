using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipeList", menuName = "Crafting/RecipeList")]
public class CraftingRecipeDataList : ScriptableObject
{
    public List<CraftingRecipeData> recipes;
}