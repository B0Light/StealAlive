using UnityEngine;

[CreateAssetMenu(menuName = "Item/Helmet")]

public class EquipmentItemInfoHelmet : EquipmentItemInfo
{
    [Header("Helmet")]
    [Range(0,80)] public float extraPhysicalAbsorption = 0;
    [Range(0,80)] public float extraMagicalAbsorption = 0;
    [Range(0,5)] public int extraActionPoint = 1;
}
