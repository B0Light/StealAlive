using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShredderHUDManager : MonoBehaviour
{
    [SerializeField] private ItemGrid itemGrid;
    [SerializeField] [Range(0f,1f)] private float disassemblyRate;
    private int _disassembledValue;
    private bool _isDisassemble = false;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI totalItemCostText;
    [SerializeField] private Button disassembleButton;

    public void Init(int width, int height, List<int> itemIdList)
    {
        disassembleButton.onClick.AddListener(DisassembleItems);
        WorldSceneChangeManager.OnSceneChanged += ResetDisassemble;
        
        itemGrid.SetGrid(width, height, itemIdList);
    }

    private void LateUpdate()
    {
        if(_isDisassemble) return;
        totalItemCostText.text = $"Value : {CalculateDisassembledValue()}";
    }

    private int CalculateDisassembledValue()
    {
        _disassembledValue = (int)(itemGrid.totalItemValue.Value * disassemblyRate);
        return _disassembledValue;
    }

    private void DisassembleItems()
    {
        _isDisassemble = true;
        disassembleButton.interactable = false;

        totalItemCostText.text = "Disassembly complete. Crusher is now inactive.";
        WorldPlayerInventory.Instance.balance.Value += _disassembledValue;
        _disassembledValue = 0;
        itemGrid.ResetItemGrid();
    }

    public void ResetDisassemble()
    {
        _isDisassemble = false;
        disassembleButton.interactable = true;
    }

    public ItemGrid GetItemGrid => itemGrid;
}
