using UnityEngine;

public class InteractableNpc : Interactable
{
    [SerializeField] protected string npcName = "";
    [SerializeField] protected string welcomeMessage = "";
    
    [Header("VCam")] 
    [SerializeField] protected GameObject vCam;
    
    private void Start()
    {
        vCam.SetActive(false);
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);

        vCam.SetActive(true);
        GUIController.Instance.OpenDialogue(npcName, ResetInteraction);
        GUIController.Instance.dialogueGUIManager.SetDialogueText(welcomeMessage);
    }
    
    public override void ResetInteraction()
    { 
        Debug.LogWarning("Reset Interaction");
        vCam.SetActive(false);
        
        PlayerInputManager.Instance.SetControlActive(true);
        
        base.ResetInteraction();
    }
}
