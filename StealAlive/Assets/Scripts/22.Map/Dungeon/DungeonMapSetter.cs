using System;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonMapSetter : MonoBehaviour
{
    public int dungeonID;
    private NavMeshSurface _navMeshSurface;
    private MapGeneratorFactory _mapGenerator;
    
    [SerializeField] private GameObject playerStartPrefab;
    [SerializeField] private GameObject exitPrefab;
    Vector2Int playerSpawn, exit;
    
    public static event Action OnPlayerSpawned;
    
    public static event Action OnBossStart;

    private void Awake()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _mapGenerator = GetComponent<MapGeneratorFactory>();

        GameTimer.OnTimerEnd += SpawnBoss;
    }

    private void Start()
    {
        GenerateMap();

        _navMeshSurface.BuildNavMesh();

        Vector3 offset = _mapGenerator.GetCubeSize();
        GeneratePlayerSpawn(offset);
    }

    private void GenerateMap()
    {
        _mapGenerator.GenerateMap();
    }

    private void GeneratePlayerSpawn(Vector3 offset)
    {
        playerSpawn = _mapGenerator.CurrentGenerator.GetStartPos();
        exit = _mapGenerator.CurrentGenerator.GetExitPos();

        Instantiate(playerStartPrefab, new Vector3(playerSpawn.x * offset.x, 0f, playerSpawn.y * offset.z), quaternion.identity);
        Instantiate(exitPrefab, new Vector3(exit.x * offset.x, 0f, exit.y * offset.z), quaternion.identity);
        
        ActivateGameTimerWithEvent();
    }
    
    
    private void ActivateGameTimerWithEvent()
    {
        // 플레이어 스폰 이벤트 발생
        OnPlayerSpawned?.Invoke();
        Debug.Log("Player Spawn");
    }

    private void SpawnBoss()
    {
        var spawner = AISpawnManager.Instance.GetClosestSpawnerToPlayer();
        spawner.AttemptToSpawnCharacter(true);
        Debug.Log("Time Over Boss Spawns");
        OnBossStart?.Invoke();
    }
}