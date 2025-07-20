using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldSaveGameManager : Singleton<WorldSaveGameManager>
{
    [Header("SAVE/LOAD")]
    [SerializeField] bool saveGame;
    [SerializeField] bool loadGame;

    [Header("World Scene Index")]
    [SerializeField] int worldSceneIndex = 1;

    [Header("Save Data Writer")]
    private SaveFileDataWriter saveFileDataWriter;

    [Header("Current Data")]
    public CharacterSlot currentCharacterSlotBeingUsed;
    public SaveGameData currentGameData;
    private string _saveFileName;

    [Header("Character Slots")]
    public List<SaveGameData> characterSlots = new List<SaveGameData>();
    private int _maxCharacterSlots = 5; // 슬롯 수를 쉽게 조정 가능

    private void Start()
    {
        // 슬롯 데이터 초기화
        for (int i = 0; i < _maxCharacterSlots; i++)
        {
            characterSlots.Add(null);
        }
        
        LoadAllCharacterProfiles();
    }
    
    private void Update()
    {
        if (saveGame)
        {
            saveGame = false;
            SaveGame();
        }

        if(loadGame)
        {
            loadGame = false;
            LoadGame();
        }
    }

    public string DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot characterSlot)
    {
        // Enum 값을 정수로 변환하여 사용
        int slotIndex = (int)characterSlot;
        return $"characterSlot_{slotIndex + 1:00}";
    }

    public void AttemptToCreateNewGame(string playerName)
    {
        saveFileDataWriter = new SaveFileDataWriter();
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;

        foreach (CharacterSlot characterSlot in Enum.GetValues(typeof(CharacterSlot)))
        {
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // IF THIS PROFILE SLOT IS NOT TAKEN, MAKE A NEW ONE USING THIS SLOT
                currentCharacterSlotBeingUsed = characterSlot;
                currentGameData = new SaveGameData();
                NewGame(playerName);
                return;
            }
        }
        
        // IF THERE ARE NO FREE SLOTS, NOTIFY THE PLAYER
        TitleScreenManager.Instance.OpenNoFreeCharacterSlotsPopUp();
    }

    private void NewGame(string playerName)
    {
        SetDefaultGameData(playerName);
        worldSceneIndex = 1; // 1 : 튜토리얼 씬
        StartCoroutine(LoadWorldScene());
    }

    private void SetDefaultGameData(string playerName)
    {
        // player
        PlayerManager player = GameManager.Instance.SpawnPlayer();
        player.playerVariableManager.characterName.Value = playerName;
        player.playerVariableManager.health.MaxValue = 100;
        player.playerVariableManager.actionPoint.MaxValue = 4;
        player.playerVariableManager.currentArmorID.Value = 0;
        player.playerVariableManager.currentHelmetID.Value = 0;
        player.playerVariableManager.currentEquippedWeaponID.Value = 0;
        player.LoadGameDataFromCurrentCharacterDataFirst(ref currentGameData);
        // inventory
        WorldPlayerInventory.Instance.balance.Value = 10000;

        // gridworld
    }

    public void SaveGame()
    {
        _saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        saveFileDataWriter = new SaveFileDataWriter();
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveFileDataWriter.saveFileName = _saveFileName;

        GameManager.Instance.GetPlayer().SaveGameDataToCurrentCharacterData(ref currentGameData);

        saveFileDataWriter.CreateNewCharacterSaveFile(currentGameData);
    }

    public void LoadGame()
    {
        // LOAD A PREVIOUS FILE, WITH A FILE NAME DEPENDING ON WHICH SLOT WE ARE USING
        _saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        saveFileDataWriter = new SaveFileDataWriter();
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveFileDataWriter.saveFileName = _saveFileName;
        
        currentGameData = saveFileDataWriter.LoadSaveFile();
        worldSceneIndex = currentGameData.sceneIndex;
        
        Debug.LogWarning("LOAD GAME");
        PlayerManager player = GameManager.Instance.SpawnPlayer();
        player.LoadGameDataFromCurrentCharacterDataFirst(ref currentGameData);
        
        UIManager.Instance.MouseActive(false);

        StartCoroutine(LoadWorldScene());
    }

    public bool LoadLastGame()
    {
        currentCharacterSlotBeingUsed = FindMostRecentlyPlayedSlot();
        
        SaveFileDataWriter saveFileWriter = new SaveFileDataWriter();
        saveFileWriter.saveDataDirectoryPath = Application.persistentDataPath;
    
        saveFileWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
        if (saveFileWriter.CheckToSeeIfFileExists())
        {
            LoadGame();
            return true;
        }

        return false;
    }
    
    public void DeleteCurrentGame()
    {
        // CHOOSE FILE BASED ON NAME
        saveFileDataWriter = new SaveFileDataWriter();
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

        saveFileDataWriter.DeleteSaveFile();
    }

    public void DeleteGame(CharacterSlot characterSlot)
    {
        // CHOOSE FILE BASED ON NAME
        saveFileDataWriter = new SaveFileDataWriter();
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
        saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);

        saveFileDataWriter.DeleteSaveFile();
    }
    
    public void DeleteAllGame()
    {
        foreach (CharacterSlot characterSlot in Enum.GetValues(typeof(CharacterSlot)))
        {
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);

            if (saveFileDataWriter.CheckToSeeIfFileExists())
            {
                DeleteGame(characterSlot);
            }
        }
    }

    private void LoadAllCharacterProfiles()
    {
        saveFileDataWriter = new SaveFileDataWriter();
        saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;

        for (int i = 0; i < _maxCharacterSlots; i++)
        {
            CharacterSlot slot = (CharacterSlot)i;
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(slot);
            characterSlots[i] = saveFileDataWriter.LoadSaveFile();
        }
    }

    public SaveGameData GetSaveGameData(CharacterSlot slot)
    {
        int index = (int)slot;
        if (index >= 0 && index < characterSlots.Count)
        {
            return characterSlots[index];
        }
        return null;
    }

    public void SetSaveGameData(CharacterSlot slot, SaveGameData data)
    {
        int index = (int)slot;
        if (index >= 0 && index < characterSlots.Count)
        {
            characterSlots[index] = data;
        }
    }

    private IEnumerator LoadWorldScene()
    {
        WorldSceneChangeManager.Instance.LoadSceneAsync(worldSceneIndex);
        
        yield return null;
    }
    
    private CharacterSlot FindMostRecentlyPlayedSlot()
    {
        DateTime mostRecentTime = DateTime.MinValue;
        CharacterSlot mostRecentSlot = CharacterSlot.CharacterSlot_01; // 기본값 설정
        bool foundAnySave = false;

        // 모든 슬롯을 확인
        for (int i = 0; i < characterSlots.Count; i++)
        {
            SaveGameData slotData = characterSlots[i];
        
            // 슬롯에 저장된 데이터가 있는지 확인
            if (slotData != null && !string.IsNullOrEmpty(slotData.lastPlayTime))
            {
                foundAnySave = true;
            
                try
                {
                    // ISO 8601 형식의 문자열을 DateTime으로 변환
                    DateTime slotDateTime = DateTime.Parse(slotData.lastPlayTime);
                
                    // 현재까지 발견한 가장 최근 시간보다 더 최근인지 확인
                    if (slotDateTime > mostRecentTime)
                    {
                        mostRecentTime = slotDateTime;
                        mostRecentSlot = (CharacterSlot)i;
                    }
                }
                catch (FormatException e)
                {
                    Debug.LogWarning($"슬롯 {i}의 시간 형식이 올바르지 않습니다: {e.Message}");
                }
            }
        }
    
        // 저장된 게임을 찾았는지 여부를 로그로 출력
        if (foundAnySave)
        {
            Debug.Log($"가장 최근 플레이: 슬롯 {(int)mostRecentSlot + 1}, 시간: {mostRecentTime}");
            return mostRecentSlot;
        }
        else
        {
            Debug.Log("저장된 게임이 없습니다.");
            return CharacterSlot.CharacterSlot_01; // 기본 슬롯 반환
        }
    }
}