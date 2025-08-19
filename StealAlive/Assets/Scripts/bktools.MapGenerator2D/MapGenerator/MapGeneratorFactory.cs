using System;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 맵 생성기 타입을 정의하는 열거형
/// </summary>
public enum MapGeneratorType
{
    BSP,            // Binary Space Partitioning
    BSPFull,        // BSP Full (분할된 영역 전체를 방으로 사용)
    Isaac,          // Isaac 스타일 (BFS 방식)
    Delaunay        // Delaunay 삼각분할 + Kruskal
}

/// <summary>
/// 맵 생성기를 생성하고 관리하는 팩토리 클래스
/// </summary>
public class MapGeneratorFactory : MonoBehaviour
{
    [Header("맵 생성기 설정")]
    [SerializeField] private MapGeneratorType currentGeneratorType = MapGeneratorType.BSP;
    [SerializeField] private bool autoGenerateOnStart = false;
    
    [Header("기본 설정")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(64, 64);
    [SerializeField] private Vector3 cubeSize = new Vector3(2, 2, 2);
    [SerializeField] private int seed = 0;
    [SerializeField] private Transform slot; // 타일을 생성할 부모 Transform
    
    [Header("Tile Data Map")]
    [SerializeField] private TileMappingDataSO tileMappingDataSO;
    
    [Header("Isaac 맵 생성기 설정")]
    [SerializeField] private int isaacMaxRooms = 15;
    [SerializeField] private int isaacSpecialRoomCount = 3;
    [SerializeField] private int isaacHorizontalSize = 11;
    [SerializeField] private int isaacVerticalSize = 11;
    [SerializeField] private PathType isaacPathType = PathType.Straight;
    [SerializeField, Range(0, 1)] private float isaacPathValue = 0.5f;
    
    [Header("Delaunay 맵 생성기 설정")]
    [SerializeField] private int delaunayMinRoomSize = 9;
    [SerializeField] private int delaunayMaxRoomSize = 12;
    [SerializeField, Range(0, 1)] private float delaunayPathValue = 0.5f;
    [SerializeField] private PathType delaunayPathType = PathType.AStar;
    
    [Header("BSP 맵 생성기 설정")]
    [SerializeField] private int bspMinRoomSize = 6;
    [SerializeField] private int bspMaxRoomSize = 20;
    [SerializeField] private int bspMaxDepth = 5;
    [SerializeField] private PathType bspPathType = PathType.LShaped;
    [SerializeField, Range(0, 1)] private float bspPathValue = 0.5f;
    
    [Header("BSP Full 맵 생성기 설정")]
    [SerializeField] private int bspFullMinSplitSize = 6;
    [SerializeField] private int bspFullMaxDepth = 5;
    [SerializeField] private PathType bspFullPathType = PathType.LShaped;
    [SerializeField, Range(0, 1)] private float bspFullPathValue = 0.5f;
    
    private BaseMapGenerator currentGenerator;
    
    public MapGeneratorType CurrentGeneratorType => currentGeneratorType;
    public BaseMapGenerator CurrentGenerator => currentGenerator;
    
    private void Start()
    {
        if (autoGenerateOnStart)
        {
            GenerateMap();
        }
    }
    
    /// <summary>
    /// 현재 선택된 맵 생성기로 맵을 생성합니다.
    /// </summary>
    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        if (currentGenerator == null)
        {
            SetupCurrentGenerator();
        }
        
        if (currentGenerator == null)
        {
            Debug.LogWarning("맵 생성기를 설정할 수 없습니다. TileMappingDataSO가 할당되었는지 확인해주세요.");
            return;
        }
        
        Debug.Log($"{currentGeneratorType} 맵 생성기를 사용하여 맵을 생성합니다.");
        currentGenerator.GenerateMap();
    }
    
    /// <summary>
    /// 맵 생성기 타입을 변경합니다.
    /// </summary>
    public void SetGeneratorType(MapGeneratorType generatorType)
    {
        if (currentGeneratorType == generatorType) return;
        
        currentGeneratorType = generatorType;
        SetupCurrentGenerator();
        
        Debug.Log($"맵 생성기가 {generatorType}로 변경되었습니다.");
    }
    
    /// <summary>
    /// 현재 맵 생성기를 설정합니다.
    /// </summary>
    private void SetupCurrentGenerator()
    {
        if (tileMappingDataSO == null)
        {
            Debug.LogError("TileMappingDataSO가 할당되지 않았습니다.");
            return;
        }
        
        // 기존 생성기 정리
        if (currentGenerator != null)
        {
            currentGenerator = null;
        }
        
        // 새 생성기 생성
        switch (currentGeneratorType)
        {
            case MapGeneratorType.BSP:
                currentGenerator = CreateBSPGenerator();
                break;
            case MapGeneratorType.BSPFull:
                currentGenerator = CreateBSPFullGenerator();
                break;
            case MapGeneratorType.Isaac:
                currentGenerator = CreateIsaacGenerator();
                break;
            case MapGeneratorType.Delaunay:
                currentGenerator = CreateDelaunayGenerator();
                break;
            default:
                Debug.LogError($"알 수 없는 맵 생성기 타입: {currentGeneratorType}");
                return;
        }
        
        if (currentGenerator != null)
        {
            Debug.Log($"{currentGeneratorType} 맵 생성기가 성공적으로 생성되었습니다.");
        }
        else
        {
            Debug.LogError($"{currentGeneratorType} 맵 생성기 생성에 실패했습니다.");
        }
    }
    
    /// <summary>
    /// BSP 맵 생성기를 생성합니다.
    /// </summary>
    private BSPDungeonMapGenerator CreateBSPGenerator()
    {
        var generator = new BSPDungeonMapGenerator(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            bspMinRoomSize, 
            bspMaxRoomSize, 
            bspMaxDepth
        );
        
        // 경로 설정 적용
        generator.pathType = bspPathType;
        generator.pathValue = bspPathValue;
        
        return generator;
    }
    
    /// <summary>
    /// BSP Full 맵 생성기를 생성합니다.
    /// </summary>
    private BSPDungeonMapGeneratorFull CreateBSPFullGenerator()
    {
        var generator = new BSPDungeonMapGeneratorFull(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            bspFullMinSplitSize, 
            bspFullMaxDepth
        );
        
        // 경로 설정 적용
        generator.pathType = bspFullPathType;
        generator.pathValue = bspFullPathValue;
        
        return generator;
    }
    
    /// <summary>
    /// Isaac 맵 생성기를 생성합니다.
    /// </summary>
    private IsaacMapGenerator CreateIsaacGenerator()
    {
        var generator = new IsaacMapGenerator(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            isaacMaxRooms, 
            isaacSpecialRoomCount, 
            isaacHorizontalSize, 
            isaacVerticalSize
        );
        
        // 경로 설정 적용
        generator.pathType = isaacPathType;
        generator.pathValue = isaacPathValue;
        
        return generator;
    }
    
    /// <summary>
    /// Delaunay 맵 생성기를 생성합니다.
    /// </summary>
    private DelaunayMapGenerator CreateDelaunayGenerator()
    {
        var generator = new DelaunayMapGenerator(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            delaunayMinRoomSize, 
            delaunayMaxRoomSize, 
            delaunayPathValue
        );
        
        // 경로 설정 적용
        generator.pathType = delaunayPathType;
        
        return generator;
    }
    
    /// <summary>
    /// 모든 맵 생성기를 정리합니다.
    /// </summary>
    public void ClearAllGenerators()
    {
        currentGenerator = null;
        Debug.Log("모든 맵 생성기가 정리되었습니다.");
    }
    
    /// <summary>
    /// 특정 맵 생성기를 활성화합니다.
    /// </summary>
    public void EnableGenerator(MapGeneratorType generatorType)
    {
        SetGeneratorType(generatorType);
    }
    
    /// <summary>
    /// 현재 맵 생성기의 상태를 확인합니다.
    /// </summary>
    public bool IsMapGenerated()
    {
        return currentGenerator != null && currentGenerator.IsMapGenerated;
    }
    
    /// <summary>
    /// 현재 맵 생성기의 맵 데이터를 가져옵니다.
    /// </summary>
    public MapData GetCurrentMapData()
    {
        return currentGenerator?.GetMapData();
    }
    
    /// <summary>
    /// 현재 생성된 맵을 제거합니다.
    /// </summary>
    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        if (currentGenerator != null)
        {
            try
            {
                currentGenerator.ClearMap();
                Debug.Log("맵이 제거되었습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"맵 제거 중 오류 발생: {e.Message}");
                
                // 오류 발생 시 강제로 slot 정리
                if (slot != null)
                {
                    ClearSlotForced();
                }
            }
        }
        else
        {
            Debug.LogWarning("제거할 맵이 없습니다.");
        }
    }
    
    /// <summary>
    /// 강제로 slot을 정리합니다.
    /// </summary>
    private void ClearSlotForced()
    {
        if (slot == null) return;
        
        try
        {
            if (Application.isPlaying)
            {
                // 런타임에서는 모든 자식을 비활성화 후 제거
                for (int i = slot.childCount - 1; i >= 0; i--)
                {
                    var child = slot.GetChild(i);
                    if (child != null)
                    {
                        child.gameObject.SetActive(false);
                        Destroy(child.gameObject);
                    }
                }
            }
            else
            {
                // 에디터에서는 즉시 제거
                for (int i = slot.childCount - 1; i >= 0; i--)
                {
                    var child = slot.GetChild(i);
                    if (child != null)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
            Debug.Log("Slot이 강제로 정리되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"강제 정리 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// 맵을 제거하고 새로운 맵을 생성합니다.
    /// </summary>
    [ContextMenu("Regenerate Map")]
    public void RegenerateMap()
    {
        ClearMap();
        GenerateMap();
    }
    
    /// <summary>
    /// 맵 생성기 설정을 검증합니다.
    /// </summary>
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetupCurrentGenerator();
        }
    }
    
    /// <summary>
    /// 정적 메서드로 맵 생성기를 생성합니다.
    /// </summary>
    public static BaseMapGenerator CreateGenerator(MapGeneratorType generatorType, Transform slot, TileMappingDataSO tileMappingData, Vector2Int gridSize, Vector3 cubeSize, int seed = 0)
    {
        switch (generatorType)
        {
            case MapGeneratorType.BSP:
                return new BSPDungeonMapGenerator(slot, tileMappingData, gridSize, cubeSize, seed, 6, 20, 5);
            case MapGeneratorType.BSPFull:
                return new BSPDungeonMapGeneratorFull(slot, tileMappingData, gridSize, cubeSize, seed, 6, 5);
            case MapGeneratorType.Isaac:
                return new IsaacMapGenerator(slot, tileMappingData, gridSize, cubeSize, seed, 15, 3, 11, 11);
            case MapGeneratorType.Delaunay:
                return new DelaunayMapGenerator(slot, tileMappingData, gridSize, cubeSize, seed, 9, 12, 0.5f);
            default:
                Debug.LogError($"알 수 없는 맵 생성기 타입: {generatorType}");
                return null;
        }
    }
    
    /// <summary>
    /// 정적 메서드로 기본 설정으로 맵 생성기를 생성합니다.
    /// </summary>
    public static BaseMapGenerator CreateGenerator(MapGeneratorType generatorType, Transform slot, TileMappingDataSO tileMappingData)
    {
        return CreateGenerator(generatorType, slot, tileMappingData, new Vector2Int(64, 64), new Vector3(2, 2, 2), 0);
    }
    
    public Vector3 GetCubeSize()
    {
        return cubeSize;
    }
}
