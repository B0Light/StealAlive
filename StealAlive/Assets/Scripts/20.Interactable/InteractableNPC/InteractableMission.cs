using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

[System.Serializable]
public class MissionDialogueStep
{
    [Header("대화 내용")]
    public string dialogueText;
    
    [Header("버튼 설정")]
    public string buttonText = "다음";
    public Sprite buttonIcon;
    
    [Header("이벤트")]
    public UnityEvent onStepCompleted;
    
    [Header("조건")]
    public bool requiresItem = false;
    public int requiredItemID = 0;
    public bool consumeItem = false;
}

public class InteractableMission : InteractableNpc
{
    [Header("미션 설정")]
    [SerializeField] private List<MissionDialogueStep> dialogueSteps = new List<MissionDialogueStep>();
    [SerializeField] private bool resetAfterCompletion = false;
    [SerializeField] private string completionMessage = "수고했어!";
    
    [Header("현재 상태")]
    [SerializeField] private int currentStepIndex = 0;
    [SerializeField] private bool missionCompleted = false;
    
    private void Start()
    {
        vCam.SetActive(false);
        LoadMissionProgress();
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);

        vCam.SetActive(true);
        GUIController.Instance.OpenDialogue(npcName, ResetInteraction);
        
        HandleMissionInteraction();
    }
    
    private void HandleMissionInteraction()
    {
        if (missionCompleted)
        {
            HandleCompletedMission();
            return;
        }
        
        if (IsValidStepIndex(currentStepIndex))
        {
            var currentStep = dialogueSteps[currentStepIndex];
            DisplayCurrentStep(currentStep);
        }
        else
        {
            CompleteMission();
        }
    }
    
    private void DisplayCurrentStep(MissionDialogueStep step)
    {
        // 대화 텍스트 설정
        GUIController.Instance.dialogueGUIManager.SetDialogueText(step.dialogueText);
        GUIController.Instance.dialogueGUIManager.ClearButtons();
        
        // 다음 단계 버튼 생성
        CreateNextStepButton(step);
    }
    
    private void CreateNextStepButton(MissionDialogueStep step)
    {
        var nextButton = GUIController.Instance.dialogueGUIManager.CreateDialogueButton(
            step.buttonText, 
            step.buttonIcon
        );
        
        bool canProceed = !step.requiresItem || CheckItemRequirement(step);

        nextButton.button.onClick.RemoveAllListeners();
        nextButton.button.interactable = canProceed;

        if (canProceed)
            nextButton.button.onClick.AddListener(() => OnNextStepClick(step));
    }
    
    private bool CheckItemRequirement(MissionDialogueStep step)
    {
        if (!step.requiresItem) return true;
        
        return WorldPlayerInventory.Instance.CheckItemInInventory(step.requiredItemID);
    }
    
    private void OnNextStepClick(MissionDialogueStep step)
    {
        // 아이템 소모
        if (step.requiresItem && step.consumeItem)
        {
            WorldPlayerInventory.Instance.RemoveItemInInventory(step.requiredItemID);
        }
        
        // 이벤트 실행 (오브젝트 제어)
        step.onStepCompleted?.Invoke();
        
        // 다음 단계로 진행
        currentStepIndex++;
        SaveMissionProgress();
        
        // UI 새로고침
        Invoke(nameof(RefreshMissionUI), 0.5f);
    }
    
    private void RefreshMissionUI()
    {
        HandleMissionInteraction();
    }
    
    private void CompleteMission()
    {
        missionCompleted = true;
        SaveMissionProgress();
        
        GUIController.Instance.dialogueGUIManager.SetDialogueText(completionMessage);
        GUIController.Instance.dialogueGUIManager.ClearButtons();
        
        if (resetAfterCompletion)
        {
            var resetButton = GUIController.Instance.dialogueGUIManager.CreateDialogueButton("처음부터 다시", null);
            resetButton.button.onClick.AddListener(ResetMission);
        }
    }
    
    private void HandleCompletedMission()
    {
        GUIController.Instance.dialogueGUIManager.SetDialogueText(completionMessage);
        GUIController.Instance.dialogueGUIManager.ClearButtons();
        
        if (resetAfterCompletion)
        {
            var resetButton = GUIController.Instance.dialogueGUIManager.CreateDialogueButton("처음부터 다시", null);
            resetButton.button.onClick.AddListener(ResetMission);
        }
    }
    
    private void ResetMission()
    {
        currentStepIndex = 0;
        missionCompleted = false;
        SaveMissionProgress();
        RefreshMissionUI();
    }
    
    private bool IsValidStepIndex(int index)
    {
        return index >= 0 && index < dialogueSteps.Count;
    }
    
    private void SaveMissionProgress()
    {
        // 게임 저장 시스템에 미션 진행도 저장
        string saveKey = $"Mission_{gameObject.name}_{GetInstanceID()}";
        PlayerPrefs.SetInt($"{saveKey}_CurrentStep", currentStepIndex);
        PlayerPrefs.SetInt($"{saveKey}_Completed", missionCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadMissionProgress()
    {
        // 저장된 미션 진행도 불러오기
        string saveKey = $"Mission_{gameObject.name}_{GetInstanceID()}";
        currentStepIndex = PlayerPrefs.GetInt($"{saveKey}_CurrentStep", 0);
        missionCompleted = PlayerPrefs.GetInt($"{saveKey}_Completed", 0) == 1;
    }
    
    public override void ResetInteraction()
    { 
        Debug.LogWarning("Reset Interaction");
        vCam.SetActive(false);
        
        PlayerInputManager.Instance.SetControlActive(true);
        
        base.ResetInteraction();
    }
    
    // 외부에서 미션 상태를 제어할 수 있는 메서드들
    public void SetMissionStep(int stepIndex)
    {
        if (IsValidStepIndex(stepIndex))
        {
            currentStepIndex = stepIndex;
            SaveMissionProgress();
        }
    }
    
    public void CompleteMissionExternal()
    {
        CompleteMission();
    }
    
    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
    
    public bool IsMissionCompleted()
    {
        return missionCompleted;
    }
}
