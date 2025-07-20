using UnityEngine;

public class InteractableDoor : Interactable
{
    private Animator _animator;

    [SerializeField] private int keyItemCode = 0;
    private readonly int _masterKeyCode = 1299;
    private bool _isOpen;
    private bool _isLock;
    
    
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
    }
    
    protected virtual void Start()
    {
        _isOpen = false;
        
        if (keyItemCode != 0)
        {
            _isLock = true;
            interactableText = "Unlock the door : Using " + WorldDatabase_Item.Instance.GetItemByID(keyItemCode).itemName;
        }
        else
        {
            _isLock = false;
            interactableText = "Open the door";
        }
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        if (_isLock)
        {
            if (WorldPlayerInventory.Instance.GetInventory().RemoveItem(keyItemCode) || WorldPlayerInventory.Instance.GetInventory().RemoveItem(_masterKeyCode))
            {
                _isLock = false;
                ToggleDoor();
            }
            else
            {
                ResetInteraction();
            }
        }
        else
        {
            ToggleDoor();
        }
    }

    protected virtual void ToggleDoor()
    {
        // 잠겨있으면 return
        if(_isLock) return;
        // 문 상태 반전
        _isOpen = !_isOpen;
        _animator?.CrossFade(_isOpen ? "Open" : "Close",0.2f);
        interactableText = _isOpen ? "Close the door" : "Open the door";
        ResetInteraction();
    }
}
