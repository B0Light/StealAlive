# 맵 생성기 시스템 (Map Generator System)

## 개요

이 시스템은 Unity에서 다양한 알고리즘을 사용하여 던전 맵을 생성하는 확장 가능한 프레임워크입니다.

## 구조

### 핵심 클래스들

#### 1. IMapGenerator (인터페이스)
- 모든 맵 생성기가 구현해야 하는 기본 인터페이스
- `GenerateMap()`, `IsMapGenerated`, `GetMapData()` 메서드 정의

#### 2. BaseMapGenerator (추상 클래스)
- 모든 맵 생성기의 공통 기능을 구현
- 그리드 초기화, 벽 생성, 렌더링 등 공통 로직 제공
- `GenerateMap()` 메서드는 추상 메서드로 정의

#### 3. 구체적인 맵 생성기들
- **BSPDungeonMapGenerator**: Binary Space Partitioning 알고리즘
- **BSPDungeonMapGeneratorFull**: BSP Full (분할된 영역 전체를 방으로 사용)
- **IsaacMapGenerator**: Isaac 스타일 (BFS 방식)
- **DelaunayMapGenerator**: Delaunay 삼각분할 + Kruskal 알고리즘

#### 4. MapGeneratorFactory
- 다양한 맵 생성기를 쉽게 선택하고 관리할 수 있는 팩토리 클래스
- 런타임에 맵 생성기 타입을 변경 가능

## 사용법

### 1. 기본 사용법

```csharp
// 특정 맵 생성기 사용
public class DungeonManager : MonoBehaviour
{
    [SerializeField] private BaseMapGenerator mapGenerator;
    
    void Start()
    {
        mapGenerator.GenerateMap();
    }
    
    void Update()
    {
        if (mapGenerator.IsMapGenerated)
        {
            var mapData = mapGenerator.GetMapData();
            // 맵 데이터 사용
        }
    }
}
```

### 2. 팩토리를 사용한 동적 맵 생성기 변경

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private MapGeneratorFactory factory;
    
    void Start()
    {
        // BSP 맵 생성기로 시작
        factory.SetGeneratorType(MapGeneratorType.BSP);
        factory.GenerateMap();
    }
    
    public void SwitchToIsaacGenerator()
    {
        // Isaac 맵 생성기로 변경
        factory.SetGeneratorType(MapGeneratorType.Isaac);
        factory.GenerateMap();
    }
}
```

### 3. 새로운 맵 생성기 추가

```csharp
public class CustomMapGenerator : BaseMapGenerator
{
    [Header("커스텀 설정")]
    [SerializeField] private int customParameter = 10;
    
    public override void GenerateMap()
    {
        InitializeGrid();
        
        // 커스텀 맵 생성 로직 구현
        GenerateCustomRooms();
        BuildWalls();
        RenderGrid();
        
        OnMapGenerationComplete();
    }
    
    private void GenerateCustomRooms()
    {
        // 구현...
    }
}
```

## 설정

### BaseMapGenerator 공통 설정
- `seed`: 랜덤 시드 (0이면 무작위)
- `gridSize`: 그리드 크기
- `cubeSize`: 큐브 크기
- `tileMappingDataSO`: 타일 매핑 데이터

### 생성기별 특수 설정

#### BSPDungeonMapGenerator
- `minRoomSize`: 최소 방 크기
- `maxRoomSize`: 최대 방 크기
- `maxDepth`: 최대 분할 깊이

#### IsaacMapGenerator
- `maxRooms`: 최대 방 개수
- `specialRoomCount`: 특수 방 개수
- `horizontalSize`: 방 가로 크기
- `verticalSize`: 방 세로 크기

#### DelaunayMapGenerator
- `minRoomSize`: 최소 방 크기
- `maxRoomSize`: 최대 방 크기
- `pathValue`: 경로 생성 확률
