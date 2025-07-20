using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graphs;
using Random = UnityEngine.Random;

[Serializable]
public class TileTypeMapping
{
    public CellType cellType;
    public TileDataSO tileData;
}

public class DungeonMapGenerator : MonoBehaviour
{
    [SerializeField] protected Vector2Int gridSize = new Vector2Int(196, 196); // 전체 맵 크기
    [SerializeField] protected int minRoomSize = 9; // 최소 방 크기
    [SerializeField] protected int maxRoomSize = 12; // 최대 방 크기
    protected int roomCount; // 방의 개수

    private int _startRoomIndex;
    private int _exitRoomIndex;
    
    [SerializeField] protected Vector3 cubeSize = new Vector3(2, 2, 2); // 큐브의 크기 추가
    
    [Header("Tile Data Map")]
    [SerializeField] private TileMappingDataSO tileMappingDataSO;
    private Dictionary<CellType, TileDataSO> tileDataDict;

    private CellType[,] _grid;

    private List<WaypointData> _waypointDataList;

    [SerializeField] private List<RectInt> _floorList;
    public List<RectInt> FloorList
    {
        get => _floorList;
        set => _floorList = value;
    }
    
    protected Delaunay2D _delaunay;
    protected HashSet<Kruskal.Edge> _selectedEdges;
    
    private void Awake()
    {
        BuildTileDataDictionary();

        int avgRoomSize = (minRoomSize + maxRoomSize) / 2;
        int spacing = 3; // 벽+복도
        int effectiveSize = avgRoomSize + spacing;
    
        int roomsX = gridSize.x / effectiveSize;
        int roomsY = gridSize.y / effectiveSize;
    
        roomCount = Mathf.Max(8, Mathf.RoundToInt(roomsX * roomsY * 0.5f));
    
        Debug.Log($"방 개수: {roomCount}개");
    }
    
    private void BuildTileDataDictionary()
    {
        tileDataDict = new Dictionary<CellType, TileDataSO>();
        if (tileMappingDataSO == null || tileMappingDataSO.tileMappings == null)
        {
            Debug.LogWarning("TileMappingDataSO is not assigned.");
            return;
        }

        foreach (var mapping in tileMappingDataSO.tileMappings)
        {
            if (!tileDataDict.ContainsKey(mapping.cellType) && mapping.tileData != null)
            {
                tileDataDict.Add(mapping.cellType, mapping.tileData);
            }
        }
    }
    

    [ContextMenu("Create Map")]
    public void GenerateMap()
    {
        _waypointDataList = new List<WaypointData>();
        
        GenerateGrid();
        PlaceRooms();
        Triangulate();
        CreatePath();
        FindPath();
        ExpandPath();
        BuildWalls();
        
        CacheEscapePoints();
        
        RenderGrid();
        
        SetMapData();
    }

    private void GenerateGrid()
    {
        _grid = new CellType[gridSize.x, gridSize.y];
        _floorList = new List<RectInt>();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                _grid[x, y] = CellType.Empty;
            }
        }
    }
    
    private void PlaceRooms()
    {
        int maxAttempts = roomCount * 5; // 방 하나당 최대 5번 시도
        int roomsPlaced = 0;

        int currentMinSize = minRoomSize;
        int currentMaxSize = maxRoomSize;

        while (roomsPlaced < roomCount && maxAttempts > 0)
        {
            // Generate random room size
            int roomWidth = Random.Range(currentMinSize, currentMaxSize + 1);
            int roomHeight = Random.Range(currentMinSize, currentMaxSize + 1);

            // Ensure the room fits within the grid boundaries
            if (roomWidth >= gridSize.x - 2 || roomHeight >= gridSize.y - 2)
            {
                maxAttempts--;
                continue;
            }

            // Generate position
            int roomX = Random.Range(1, gridSize.x - roomWidth - 1);
            int roomY = Random.Range(1, gridSize.y - roomHeight - 1);

            RectInt newRoom = new RectInt(roomX, roomY, roomWidth, roomHeight);
            // 버림 계산으로 변경
            Vector2Int roomCenter = new Vector2Int(
                roomX + roomWidth / 2,
                roomY + roomHeight / 2
            );
            
            bool overlapping = false;
            foreach (var existingRoom in _floorList)
            {
                RectInt expandedExistingRoom = new RectInt(
                    existingRoom.x - 1,
                    existingRoom.y - 1,
                    existingRoom.width + 2,
                    existingRoom.height + 2
                );

                if (newRoom.Overlaps(expandedExistingRoom))
                {
                    overlapping = true;
                    break;
                }
            }

            if (!overlapping)
            {
                _floorList.Add(newRoom);

                // Mark the room on the grid
                List<Vector2Int> roomCoordinates = new List<Vector2Int>();
                
                for (int x = roomX; x < roomX + roomWidth; x++)
                {
                    for (int y = roomY; y < roomY + roomHeight; y++)
                    {
                        Vector2Int coord = new Vector2Int(x, y);
                        roomCoordinates.Add(coord);
                        if (x == roomCenter.x && y == roomCenter.y)
                            _grid[x, y] = CellType.FloorCenter;
                        else
                            _grid[x, y] = CellType.Floor;
                    }
                }

                // Add waypoints
                Vector2Int[] randomPoints = roomCoordinates.OrderBy(_ => Random.value).Take(4).ToArray();
                Vector3[] convertedPoints = randomPoints.Select(pos => ConvertGridPos(pos)).ToArray();
                _waypointDataList.Add(new WaypointData(convertedPoints));

                roomsPlaced++;
            }

            maxAttempts--;

            // 점진적 완화 로직: 실패가 많아지면 roomSize를 줄여서 배치 확률을 높임
            if (maxAttempts == roomCount * 3)
            {
                currentMaxSize = Mathf.Max(minRoomSize + 1, currentMaxSize - 1);
            }
            else if (maxAttempts == roomCount)
            {
                currentMinSize = Mathf.Max(2, currentMinSize - 1);
            }
        }

        if (roomsPlaced < roomCount)
        {
            Debug.LogWarning($"⚠️ {roomCount}개 중 {roomsPlaced}개 방만 배치되었습니다.\n" +
                             $"- gridSize: {gridSize.x}x{gridSize.y}, " +
                             $"- minSize: {minRoomSize}, maxSize: {maxRoomSize}\n" +
                             $"- 남은 공간 부족일 수 있습니다.");
        }
    }


    
    private void Triangulate() 
    {
        List<Vertex> vertices = new List<Vertex>();
        
        vertices.AddRange(_floorList.Select(floor => 
            new Vertex<RectInt>(floor.position + ((Vector2)floor.size) / 2, floor)));

        _delaunay = Delaunay2D.Triangulate(vertices);
    }
    
    protected virtual void CreatePath() 
    {
        var edges = _delaunay.Edges.Select(edge => new Kruskal.Edge(edge.U, edge.V)).ToList();
        var vertices = _delaunay.Vertices;
        _selectedEdges = new HashSet<Kruskal.Edge>(Kruskal.MinimumSpanningTree(edges, vertices));

        // 일부 랜덤한 엣지를 추가하여 더 많은 복도 생성
        foreach (var edge in edges.Where(e => !_selectedEdges.Contains(e))) 
        {
            if (Random.value < 0.125) 
            {
                _selectedEdges.Add(edge);
            }
        }
    }
    
    private void FindPath() 
    {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(gridSize);

        foreach (var edge in _selectedEdges) 
        {
            var startRoom = (edge.U as Vertex<RectInt>)?.Item;
            var endRoom = (edge.V as Vertex<RectInt>)?.Item;

            if (startRoom == null || endRoom == null) continue;

            // 버림 계산으로 변경
            var startPos = new Vector2Int(
                (int)startRoom?.center.x, 
                (int)startRoom?.center.y
            );
            var endPos = new Vector2Int(
                (int)endRoom?.center.x, 
                (int)endRoom?.center.y
            );

            var path = aStar.FindPath(
                startPos, 
                endPos, 
                (a, b) => 
                { 
                    // 휴리스틱 비용 계산
                    var cost = Vector2Int.Distance(b.Position, endPos);

                    // 셀 타입에 따른 이동 비용 결정
                    var traversalCost = _grid[b.Position.x,b.Position.y] switch
                    {
                        CellType.Floor => 10f,
                        CellType.Empty => 5f,
                        CellType.Path => 1f,
                        _ => 1f
                    };

                    return new DungeonPathfinder2D.PathCost
                    {
                        traversable = true,
                        cost = cost + traversalCost
                    };
                });

            if (path == null) continue;

            // 경로에 복도 배치
            foreach (var pos in path) 
            {
                if (_grid[pos.x,pos.y] == CellType.Empty) 
                {
                    _grid[pos.x,pos.y] = CellType.Path;
                }
            }
        }
    }
    
    private void ExpandPath()
    {
        // 벽 배치: 바닥 셀 주변이 빈 셀이라면 그곳에 벽을 배치
        for (var x = 1; x < gridSize.x - 1; x++)
        {
            for (var y = 1; y < gridSize.y - 1; y++)
            {
                if (_grid[x, y] == CellType.Path)
                {
                    // 상하좌우 체크
                    if (_grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.ExpandedPath;
                    if (_grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.ExpandedPath;
                    if (_grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.ExpandedPath;
                    if (_grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.ExpandedPath;
                }
            }
        }
    }
    
    private void BuildWalls()
    {
        // 벽 배치: 바닥 셀 주변이 빈 셀이라면 그곳에 벽을 배치
        for (int x = 1; x < gridSize.x - 1; x++)
        {
            for (int y = 1; y < gridSize.y - 1; y++)
            {
                if (_grid[x, y] == CellType.Floor || _grid[x, y] == CellType.ExpandedPath)
                {
                    // 상하좌우 체크
                    if (_grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.Wall;
                    if (_grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.Wall;
                    if (_grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.Wall;
                    if (_grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.Wall;
                }
            }
        }
    }
    
    private void CacheEscapePoints()
    {
        FindFurthestRooms();
    }
    
    private void RenderGrid()
    {
        // 그리드를 시각적으로 표현
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                RenderTileAt(x, y);
            }
        }
    }
    
    private void RenderTileAt(int x, int y)
    {
        if (!TryGetTileData(x, y, out TileDataSO tileData))
            return;

        Vector3 spawnPos = new Vector3(x * cubeSize.x, 0, y * cubeSize.z);

        if (_grid[x, y] == CellType.FloorCenter)
        {
            HandleCenterTileRendering(x, y, tileData, spawnPos);
        }
        else
        {
            tileData.SpawnTile(spawnPos, cubeSize, transform);
        }
    }

    private bool TryGetTileData(int x, int y, out TileDataSO tileData)
    {
        return tileDataDict.TryGetValue(_grid[x, y], out tileData) && tileData != null;
    }

    private void HandleCenterTileRendering(int x, int y, TileDataSO tileData, Vector3 spawnPos)
    {
        RectInt? room = GetRoomByCenterPosition(x, y);
        if (!room.HasValue)
        {
            Debug.LogWarning( "POS :" + x + ", " + y+ " : NO ROOM");
            tileData.SpawnTile(spawnPos, cubeSize, transform);
            return;
        }

        if (_floorList[_startRoomIndex] == room || _floorList[_exitRoomIndex] == room)
        {
            Debug.LogWarning("Room SpawnPoint");
            tileData.SpawnTile(spawnPos, cubeSize, transform, true);
        }
        else
        {
            RoomInfo roomInfo = GetRoomInfo(room.Value);
            tileData.SpawnTileWithSizeAwareProps(spawnPos, cubeSize, transform, roomInfo);
        }
    }


    private void SetMapData()
    {
        if (MapDataManager.Instance != null)
        {
            MapDataManager.Instance.WaypointDataList = _waypointDataList;
        }
    }

    private Vector3 ConvertGridPos(Vector2Int pos)
    {
        Vector3 position = new Vector3(pos.x * cubeSize.x, 0, pos.y * cubeSize.z); // 큐브 크기를 고려한 위치 조정
        return position;
    }
    
    // 가장 멀리 떨어진 두 방을 찾는 메서드
    private void FindFurthestRooms() 
    {
        int floorListCount = _floorList.Count;
        float[,] distances = new float[floorListCount, floorListCount];
        // 방들 간의 유클리드 거리 계산
        for (int i = 0; i < floorListCount; i++) 
        {
            for (int j = i + 1; j < floorListCount; j++) 
            {
                float distance = Vector2.Distance(_floorList[i].center, _floorList[j].center);
                distances[i, j] = distance;
                distances[j, i] = distance;
            }
        }

        // 플로이드-워셜 알고리즘을 사용해 모든 방 간의 최단 거리 계산
        for (int k = 0; k < floorListCount; k++) 
        {
            for (int i = 0; i < floorListCount; i++) 
            {
                for (int j = 0; j < floorListCount; j++) 
                {
                    distances[i, j] = Mathf.Min(distances[i, j], distances[i, k] + distances[k, j]);
                }
            }
        }

        // 가장 멀리 떨어진 두 방 찾기
        float maxDistance = 0f;
        int roomA = 0, roomB = 0;

        for (int i = 0; i < floorListCount; i++) 
        {
            for (int j = i + 1; j < floorListCount; j++) 
            {
                if (distances[i, j] > maxDistance) 
                {
                    maxDistance = distances[i, j];
                    roomA = i;
                    roomB = j;
                }
            }
        }

        Debug.Log($"{floorListCount}중 가장 멀리 떨어진 방: Room {roomA}와 Room {roomB}, 거리: {maxDistance}");
        _startRoomIndex = roomA;
        _exitRoomIndex = roomB;
    }

    #region Find Room

    // 좌표로 방 찾기 메서드 추가
    public RectInt? GetRoomAtPosition(int x, int y)
    {
        // 해당 셀이 방(Floor 또는 FloorCenter)이 아니면 null 반환
        if (_grid[x, y] != CellType.Floor && _grid[x, y] != CellType.FloorCenter)
            return null;

        // 모든 방을 순회하며 해당 좌표가 포함된 방 찾기
        foreach (var room in _floorList)
        {
            if (room.Contains(new Vector2Int(x, y)))
            {
                return room;
            }
        }

        return null;
    }

    // 방 중심점 찾기 메서드 추가
    private RectInt? GetRoomByCenterPosition(int x, int y)
    {
        if (_grid[x, y] != CellType.FloorCenter)
            return null;

        foreach (var room in _floorList)
        {
            // 버림 계산으로 변경
            Vector2Int center = new Vector2Int(
                room.x + room.width / 2,
                room.y + room.height / 2
            );
            
            if (center.x == x && center.y == y)
            {
                Debug.LogWarning("FIND ROOM!");
                return room;
            }
        }

        return null;
    }

    // 추가 기능: 방 정보 얻기
    public RoomInfo GetRoomInfo(RectInt room)
    {
        // 버림 계산으로 변경
        Vector2Int center = new Vector2Int(
            room.x + room.width / 2,
            room.y + room.height / 2
        );
        
        return new RoomInfo
        {
            position = room.position,
            size = room.size,
            center = center,
            worldPosition = ConvertGridPos(room.position),
            worldCenter = ConvertGridPos(center)
        };
    }

    #endregion

    public Vector2Int GetStartPos()
    {
        // 버림 계산으로 변경
        RectInt startRoom = _floorList[_startRoomIndex];
        return new Vector2Int(
            startRoom.x + startRoom.width / 2,
            startRoom.y + startRoom.height / 2
        );
    }
    
    public Vector2Int GetExitPos()
    {
        // 버림 계산으로 변경
        RectInt exitRoom = _floorList[_exitRoomIndex];
        return new Vector2Int(
            exitRoom.x + exitRoom.width / 2,
            exitRoom.y + exitRoom.height / 2
        );
    }
}


[Serializable]
public class RoomInfo
{
    public Vector2Int position;  // 그리드 상의 좌측 하단 위치
    public Vector2Int size;      // 방 크기
    public Vector2Int center;    // 그리드 상의 중심점
    public Vector3 worldPosition; // 월드 좌표계에서의 좌측 하단 위치
    public Vector3 worldCenter;   // 월드 좌표계에서의 중심점

    public override string ToString()
    {
        return $"Room at {position}, size: {size}, center: {center}";
    }
}