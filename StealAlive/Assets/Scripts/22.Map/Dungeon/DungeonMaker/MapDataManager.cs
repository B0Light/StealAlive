using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

public class MapDataManager : MonoBehaviour
{
    public static MapDataManager Instance;
    
    private Dictionary<int, int> _killLog = new Dictionary<int, int>();
    
    [SerializeField]
    private List<WaypointData> _waypointDataList;

    public List<WaypointData> WaypointDataList
    {
        get => _waypointDataList;
        set => _waypointDataList = value;
    }

    private void Awake()
    {
        Instance = this;
    }
    
    public void AddKillLog(int itemId)
    {
        if (_killLog.TryGetValue(itemId, out int currentCount))
        {
            _killLog[itemId] = currentCount + 1;
        }
        else
        {
            _killLog[itemId] = 1;
        }
    }

    public Dictionary<int, int> GetKillLog() => _killLog;
}
