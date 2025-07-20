using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class AICharacterPatrolManager : MonoBehaviour
{
    private AICharacterManager aiCharacter;

    [SerializeField]
    private List<WaypointData> _waypointDataList;

    private int currentWaypointDataIndex = 0;
    private int currentWaypointIndex = 0;

    private bool _isMapDataLoaded;
    
    [Header("Ambush Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float ambushProbability = 0.3f; // 30% 확률로 매복 모드
    
    [SerializeField]
    private float ambushDuration = 10f; // 매복 지속 시간 (초)
    
    private bool isInAmbushMode = false;
    private Vector3 ambushPosition;
    private float ambushStartTime;
    
    private void Start()
    {
        aiCharacter = GetComponent<AICharacterManager>();
        StartCoroutine(WaitForMapDataManager());
        SelectWaypointData();
    }
    
    private IEnumerator WaitForMapDataManager()
    {
        float waitTime = 0f;
        const float maxWaitTime = 5f;
    
        while (MapDataManager.Instance == null && waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
    
        if (MapDataManager.Instance != null)
        {
            _waypointDataList = new List<WaypointData>(MapDataManager.Instance.WaypointDataList);
        }
        else
        {
            Debug.LogWarning("MapDataManager.Instance를 5초 동안 기다렸지만 null입니다.");
            // 필요에 따라 기본값 설정
            _waypointDataList = new List<WaypointData>();
        }

        _isMapDataLoaded = true;
    }

    public void SelectWaypointData()
    {
        currentWaypointDataIndex = Random.Range(0, _waypointDataList.Count);
        currentWaypointIndex = 0;
        
        // 새로운 웨이포인트 데이터를 선택할 때 매복 모드 여부 결정
        CheckForAmbushMode();
    }
    
    private void CheckForAmbushMode()
    {
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue <= ambushProbability)
        {
            EnterAmbushMode();
        }
        else
        {
            ExitAmbushMode();
        }
    }
    
    private void EnterAmbushMode()
    {
        isInAmbushMode = true;
        ambushPosition = transform.position;
        ambushStartTime = Time.time;
        
        //Debug.Log($"AI가 매복 모드에 진입했습니다. 위치: {ambushPosition}");
    }
    
    private void ExitAmbushMode()
    {
        /*
        if (isInAmbushMode)
        {
            //Debug.Log("AI가 매복 모드를 종료하고 순찰을 재개합니다.");
        }
        */
        isInAmbushMode = false;
    }
    
    private void Update()
    {
        // 매복 모드 시간 체크
        if (isInAmbushMode && Time.time - ambushStartTime >= ambushDuration)
        {
            ExitAmbushMode();
            SelectWaypointData(); // 매복 종료 후 새로운 웨이포인트 선택
        }
    }

    private bool IsWaypointCompleted()
    {
        if (_waypointDataList.Count == 0 || _waypointDataList[currentWaypointDataIndex].waypoints.Length == 0)
        {
            return false;
        }

        Vector3 targetPosition = isInAmbushMode ? ambushPosition : _waypointDataList[currentWaypointDataIndex].waypoints[currentWaypointIndex];
        
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance < 5.0f; 
    }

    public Vector3? GetNextWaypoint()
    {
        // 데이터가 아직 로딩되지 않았으면 null 반환
        if (!_isMapDataLoaded)
        {
            Debug.LogWarning("MapData가 아직 로딩되지 않았습니다.");
            return null;
        }
        
        // 매복 모드일 때는 매복 위치 반환
        if (isInAmbushMode)
        {
            return ambushPosition;
        }
    
        if (_waypointDataList.Count == 0 || _waypointDataList[currentWaypointDataIndex].waypoints.Length == 0)
        {
            Debug.LogWarning("NO Patrol Data");
            return null;
        }

        if (IsWaypointCompleted())
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= _waypointDataList[currentWaypointDataIndex].waypoints.Length)
            {
                SelectWaypointData();
            }
        }
        return _waypointDataList[currentWaypointDataIndex].waypoints[currentWaypointIndex];
    }
    
    // 현재 AI가 매복 모드인지 확인하는 public 메서드
    public bool IsInAmbushMode()
    {
        return isInAmbushMode;
    }
    
    // 강제로 매복 모드를 종료하는 메서드 (필요시 사용)
    public void ForceExitAmbushMode()
    {
        ExitAmbushMode();
    }
    
    // 매복 확률을 동적으로 변경하는 메서드
    public void SetAmbushProbability(float probability)
    {
        ambushProbability = Mathf.Clamp01(probability);
    }
}