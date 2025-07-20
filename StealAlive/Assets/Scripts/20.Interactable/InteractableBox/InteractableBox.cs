using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class InteractableBox : Interactable
{
    [Header("Box Info")]
    [SerializeField] [Range(1,6)] private int boxWidth;
    [SerializeField] [Range(1,10)] private int boxHeight;
    
    [SerializeField] protected BoxType boxType;
    [SerializeField] [Range(0,5)] protected int boxTier = 0;
    [SerializeField] protected List<int> itemIdList = new List<int>();
    
    private Animator _animator;
    
    protected bool isOpen;
    private bool _isDoorOpened;
    
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        isOpen = false;
        
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        if (isOpen == false)
        {
            OpenDoor(); 
            PlayerInputManager.Instance.SetControlActive(false);
            GUIController.Instance.WaitToInteraction(PerformInteraction);
            isOpen = true;
        }
        else
        {
            PerformInteraction();
        }
        
    }
    
    protected virtual void PerformInteraction()
    {
        GUIController.Instance.OpenInteractableBox(boxWidth, boxHeight, itemIdList, this);
    }

    protected void OpenDoor()
    {
        if(_isDoorOpened) return; // 이미 열여있는 상태면 열지 않음
        
        _isDoorOpened = true;
        if(_animator)
            _animator.CrossFade("Open",0.2f);
    }
    
    private void CloseDoor()
    {
        if (!_isDoorOpened) return; // 이미 닫혀있는 상태면 닫지 않음
        
        _isDoorOpened = false;
        if(_animator)
            _animator.CrossFade("Close",0.2f);
    }

    public override void ResetInteraction()
    {
        base.ResetInteraction();
        CloseDoor();
    }
}
