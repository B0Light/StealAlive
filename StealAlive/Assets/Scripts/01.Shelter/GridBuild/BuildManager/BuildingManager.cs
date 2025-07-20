using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public HUDGridBuildCategorySelector gridBuildCategorySelector;
    public HUDGridBuildingSelector gridBuildingSelector;

    public ShelterManager shelterManager;

    public SerializableDictionary<int, int> unlockedBuilding;
    public SerializableDictionary<TileCategory, List<int[]>> unlockedBuildingByCategory;

    private List<int> _purchasedBuilding;
    private List<int> _buildingsForSale;

    [SerializeField] private CanvasGroup constructionCanvasGroup;
    [SerializeField] private CanvasGroup buildingSelectionCanvasGroup;
    [SerializeField] private CanvasGroup hudBuildingShop;
    [SerializeField] private CanvasGroup buildingPopupCanvasGroup;
    
    
    [Space(10)]
    [SerializeField] private GridBuildingShopUIManager gridBuildingShopUIManager;
    [SerializeField] private HUD_BuildInfo buildInfoHUD;
    
    [Header("BuildCam")] 
    [SerializeField] private GridBuildCamera _gridBuildCamera;
    
    private Interactable _interactableObject;
    private RevenueFacilityTile _currentSelectTile = null;
    
    // 시간에 따라 상호작용 가능 여부 확인 

    private void Awake()
    {
        Instance = this;
        LoadUnlockedBuildings();
        InitBuildingCategory();
    }
    
    private void LoadUnlockedBuildings()
    {
        unlockedBuilding = WorldSaveGameManager.Instance.currentGameData.unlockedBuilding;
        _purchasedBuilding = new List<int>(unlockedBuilding.Keys);
    }
    
    private void InitBuildingCategory()
    {
        foreach (TileCategory tileCategory in Enum.GetValues(typeof(TileCategory)))
        {
            if(tileCategory == TileCategory.None) return;
            if(unlockedBuildingByCategory.ContainsKey(tileCategory) == false)
                unlockedBuildingByCategory.Add(tileCategory, new List<int[]>());
        }
    }

    private void Start()
    {
        ToggleMainBuildHUD(false);
        ToggleShopHUD(false);
        ToggleBuildPopUpHUD(false);
        ToggleBuildSelectionHUD(false);
        StartCoroutine(InitShop());
    }
    
    private IEnumerator InitShop()
    {
        yield return StartCoroutine(WaitForDataLoad());
        LoadUnlockedBuildings();
        _buildingsForSale = new List<int>(WorldDatabase_Build.Instance.GetAllBuildObjData());
        RemoveCommonElements(_buildingsForSale, _purchasedBuilding);
        
        foreach (var code in unlockedBuilding.Keys)
        {
            UpdateCategory(WorldDatabase_Build.Instance.GetBuildingByID(code));
        }
        gridBuildCategorySelector.RefreshBuildingCategory();
    }
    
    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Build.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
    }

    // Interactable Build Controller 와 상호작용해서 HUD를 열때 사용 
    public void ToggleMainBuildHUD(bool isActive, Interactable interactable = null)
    {
        if (isActive && interactable != null)
        {
            _interactableObject = interactable;
        }
        else if (!isActive && _interactableObject != null)
        {
            _interactableObject.ResetInteraction();
            _interactableObject = null;
        }
        
        ToggleConstructionHUD(isActive);
        GridBuildingSystem.Instance.SetActive(isActive);
        if (isActive)
        {
            TurnOnGridBuildCamera();
        }
        else
        {
            TurnOffGridBuildCamera();
        }
    }

    public void OpenShop()
    {
        LoadUnlockedBuildings();
        _buildingsForSale = new List<int>(WorldDatabase_Build.Instance.GetAllBuildObjData());
        RemoveCommonElements(_buildingsForSale, _purchasedBuilding);
        
        gridBuildingShopUIManager.OpenShop(_buildingsForSale);
        ToggleShopHUD(true);
        ToggleConstructionHUD(false);
    }

    public void ToggleBuildSelector()
    {
        bool isOpen = buildingSelectionCanvasGroup.interactable;

        gridBuildCategorySelector.RefreshBuildingCategory();
        ToggleBuildSelectionHUD(!isOpen);
    }

    public void ToggleShopHUD(bool isActive)
    {
        hudBuildingShop.alpha = isActive ? 1f : 0f;
        hudBuildingShop.blocksRaycasts = isActive;
        hudBuildingShop.interactable = isActive;
    }

    public void ToggleConstructionHUD(bool isActive)
    {
        constructionCanvasGroup.alpha = isActive ? 1f : 0f;
        constructionCanvasGroup.blocksRaycasts = isActive;
        constructionCanvasGroup.interactable = isActive;
    }
    
    public void ToggleBuildSelectionHUD(bool isActive)
    {
        buildingSelectionCanvasGroup.alpha = isActive ? 1f : 0f;
        buildingSelectionCanvasGroup.blocksRaycasts = isActive;
        buildingSelectionCanvasGroup.interactable = isActive;
    }

    private void ToggleBuildPopUpHUD(bool isActive)
    {
        buildingPopupCanvasGroup.alpha = isActive ? 1f : 0f;
        buildingPopupCanvasGroup.blocksRaycasts = isActive;
        buildingPopupCanvasGroup.interactable = isActive;
    }

    public void OpenBuildPopUpHUD(RevenueFacilityTile revenueFacilityTile)
    {
        _currentSelectTile = revenueFacilityTile;
        _currentSelectTile.SelectObject(true);
        
        buildInfoHUD.Init(revenueFacilityTile);
        
        ToggleShopHUD(false);
        ToggleConstructionHUD(false);
        ToggleBuildPopUpHUD(true);
    }
    
    public void CloseBuildInfoHUD()
    {
        if (!_currentSelectTile) return;
        _currentSelectTile.SelectObject(false);
        _currentSelectTile = null;
        
        ToggleShopHUD(false);
        ToggleConstructionHUD(true);
        ToggleBuildPopUpHUD(false);
    }

    public void SelectCategory(TileCategory id)
    {
        GridBuildingSystem.Instance.SelectToBuild(null);
        StartCoroutine(gridBuildingSelector.InitBtnSlot(id));
    }

    public void RefreshCategory()
    {
        GridBuildingSystem.Instance.SelectToBuild(null);
        gridBuildingSelector.RefreshSlot();
    }
    
    private void RemoveCommonElements<T>(List<T> listA, List<T> listB)
    {
        HashSet<T> commonElements = new HashSet<T>(listA.Intersect(listB));

        listA.RemoveAll(item => commonElements.Contains(item));
    }

    private void TurnOnGridBuildCamera()
    {
        _gridBuildCamera.gameObject.SetActive(true);
        PlayerCameraController.Instance.TurnOffCamera();
    }
    
    private void TurnOffGridBuildCamera()
    {
        _gridBuildCamera.gameObject.SetActive(false);
        PlayerCameraController.Instance.TurnOnCamera();
    }

    public void RegisterBuilding(BuildObjData buildObjData)
    {
        if(!buildObjData) return;
        unlockedBuilding.TryAdd(buildObjData.itemCode, 0);
        UpdateCategory(buildObjData);
        gridBuildCategorySelector.RefreshBuildingCategory();
        SaveGridData();
        WorldSaveGameManager.Instance.SaveGame();
    }

    private void UpdateCategory(BuildObjData buildObjData)
    {
        TileCategory tileCategory = buildObjData.GetTileCategory(); 
        
        if(unlockedBuildingByCategory.ContainsKey(tileCategory) == false)
            unlockedBuildingByCategory.Add(tileCategory, new List<int[]>());
        
        unlockedBuildingByCategory[tileCategory].Add(new int[]{buildObjData.itemCode, 0});
    }

    public void ExitBuildHUD()
    {
        SaveGridData(); 
        
        GridBuildingSystem.Instance.SelectToBuild(null);
        GUIController.Instance.ToggleMainGUI(true);
        PlayerInputManager.Instance.SetControlActive(true);
        PlayerCameraController.Instance.TurnOnCamera();

        ToggleMainBuildHUD(false);
    }

    private void SaveGridData()
    {
        WorldSaveGameManager.Instance.currentGameData.buildings.Clear();
        foreach (var building in GridBuildingSystem.Instance.SaveBuildingDataList)
        {
            if (building != null)
            {
                WorldSaveGameManager.Instance.currentGameData.buildings.Add(building);
            }
        }
        
        foreach (var building in GridBuildingSystem.Instance.SalvagedBuildingList)
        {
            WorldSaveGameManager.Instance.currentGameData.unlockedBuilding.TryAdd(building.Key, 0);
            if (building.Value > 0)
            {
                WorldSaveGameManager.Instance.currentGameData.unlockedBuilding[building.Key] = building.Value;
            }
        }
        WorldSaveGameManager.Instance.SaveGame();
    }

}
