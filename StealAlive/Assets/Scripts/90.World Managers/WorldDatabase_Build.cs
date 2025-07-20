using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldDatabase_Build : Singleton<WorldDatabase_Build>
{
    public bool IsDataLoaded { get; private set; }
    
    private readonly List<BuildObjData> _allBuildObjDataList = new List<BuildObjData>();
    private readonly List<int> _buildingCodeList = new List<int>();
    
    private readonly List<Sprite> _defaultCategoryIcon = new List<Sprite>();
    // 만약 다른 카테고리의 건축물이 추가 될 경우 고려 
    protected override void Awake()
    {
        base.Awake();
        IsDataLoaded = false;
        LoadData();
    }

    private void LoadData()
    {
        BuildObjData[] buildingObjects = Resources.LoadAll<BuildObjData>("Building");
        foreach (var unit in buildingObjects)
        {
            if(unit.isForbidden) continue;
            _allBuildObjDataList.Add(unit);
            _buildingCodeList.Add(unit.itemCode);
        }
        
        Sprite[] defaultIcons = Resources.LoadAll<Sprite>("Building/99_DefaultIcon");
        foreach (var icon in defaultIcons)
        {
            _defaultCategoryIcon.Add(icon);
        }
        IsDataLoaded = true;
    }
    
    public BuildObjData GetBuildingByID(int id) => 
        _allBuildObjDataList.FirstOrDefault(buildObjData => buildObjData.itemCode == id);

    public List<int> GetAllBuildObjData() => _buildingCodeList;
    public Sprite GetCategoryIcon(TileCategory id) => _defaultCategoryIcon[(int)id];

}
