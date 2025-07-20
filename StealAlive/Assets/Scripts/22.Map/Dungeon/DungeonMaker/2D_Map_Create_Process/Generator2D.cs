using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Graphs;
using Random = UnityEngine.Random;

public class Generator2D : MonoBehaviour 
{
    // 셀의 타입을 정의하는 열거형
    private enum CellType 
    {
        None,
        Field,
        Path,
    }

    // 방(Field) 클래스 정의
    [Serializable]
    class Field
    {
        [SerializeField] private GameObject fieldGameObject;
        public RectInt bounds;

        public Field(Vector2Int location, Vector2Int size) 
        {
            bounds = new RectInt(location, size);
        }

        // 두 방이 겹치는지 확인
        public static bool Intersect(Field a, Field b) 
        {
            return a.bounds.Overlaps(b.bounds);
        }

        // 방에 연결된 게임 오브젝트 설정
        public void SetObject(GameObject obj)
        {
            fieldGameObject = obj;
        }
    }

    // 필드 변수들
    [SerializeField] private Vector2Int size;
    [SerializeField] private int roomCount;
    [SerializeField] private Vector2Int roomMaxSize;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private float unitSize = 1f;  // 기본 유닛 크기 변수
    private Grid2D<CellType> _grid;
    [SerializeField] private List<Field> _rooms;
    private Delaunay2D _delaunay;
    private HashSet<Kruskal.Edge> _selectedEdges;

    // 초기화 메서드
    void Start() 
    {
        Generate();
    }

    // 전체 맵 생성 과정
    void Generate() 
    {
        _grid = new Grid2D<CellType>(size, Vector2Int.zero);
        _rooms = new List<Field>();

        PlaceFields();
        Triangulate();
        CreateHallways();
        Pathfind();
        FindFurthestRooms();
    }

    // 방들을 배치하는 메서드
    private void PlaceFields() 
    {
        for (int i = 0; i < roomCount; i++) 
        {
            Vector2Int location = new Vector2Int(
                Random.Range(0, size.x),
                Random.Range(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(
                Random.Range(1, roomMaxSize.x + 1),
                Random.Range(1, roomMaxSize.y + 1)
            );

            Field newField = new Field(location, roomSize);
            Field buffer = new Field(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            // 방이 다른 방과 겹치거나 맵의 경계를 넘는 경우 배치 생략
            if (_rooms.Any(room => Field.Intersect(room, buffer)) || 
                newField.bounds.xMin < 0 || newField.bounds.xMax >= size.x || 
                newField.bounds.yMin < 0 || newField.bounds.yMax >= size.y) 
            {
                continue;
            }
            
            _rooms.Add(newField);
            PlaceRoom(newField.bounds.position, newField.bounds.size, newField);

            // 그리드에 방 위치를 저장
            foreach (var pos in newField.bounds.allPositionsWithin) 
            {
                _grid[pos] = CellType.Field;
            }
        }
    }

    // 델로네 삼각분할을 사용해 방들 간의 연결 계산
    private void Triangulate() 
    {
        List<Vertex> vertices = new List<Vertex>();
        
        vertices.AddRange(_rooms.Select(room => 
            new Vertex<Field>(room.bounds.position + ((Vector2)room.bounds.size) / 2, room)));

        _delaunay = Delaunay2D.Triangulate(vertices);
    }

    // 방들을 연결하는 복도 생성
    private void CreateHallways() 
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

    // A* 경로 탐색을 사용하여 복도를 배치
    private void Pathfind() 
    {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

        foreach (var edge in _selectedEdges) 
        {
            var startRoom = (edge.U as Vertex<Field>)?.Item;
            var endRoom = (edge.V as Vertex<Field>)?.Item;

            if (startRoom == null || endRoom == null) continue;

            Vector2Int startPos = new Vector2Int((int)startRoom.bounds.center.x, (int)startRoom.bounds.center.y);
            Vector2Int endPos = new Vector2Int((int)endRoom.bounds.center.x, (int)endRoom.bounds.center.y);

            var path = aStar.FindPath(
                startPos, 
                endPos, 
                (a, b) => 
                { 
                    // 휴리스틱 비용 계산
                    var cost = Vector2Int.Distance(b.Position, endPos);

                    // 셀 타입에 따른 이동 비용 결정
                    var traversalCost = _grid[b.Position] switch
                    {
                        CellType.Field => 10f,
                        CellType.None => 5f,
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
                if (_grid[pos] == CellType.None) 
                {
                    _grid[pos] = CellType.Path;
                    PlaceHallway(pos);
                }
            }
        }
    }
    
    // 가장 멀리 떨어진 두 방을 찾는 메서드
    private void FindFurthestRooms() 
    {
        int roomCount = _rooms.Count;
        float[,] distances = new float[roomCount, roomCount];

        // 방들 간의 유클리드 거리 계산
        for (int i = 0; i < roomCount; i++) 
        {
            for (int j = i + 1; j < roomCount; j++) 
            {
                float distance = Vector2.Distance(_rooms[i].bounds.center, _rooms[j].bounds.center);
                distances[i, j] = distance;
                distances[j, i] = distance;
            }
        }

        // 플로이드-워셜 알고리즘을 사용해 모든 방 간의 최단 거리 계산
        for (int k = 0; k < roomCount; k++) 
        {
            for (int i = 0; i < roomCount; i++) 
            {
                for (int j = 0; j < roomCount; j++) 
                {
                    distances[i, j] = Mathf.Min(distances[i, j], distances[i, k] + distances[k, j]);
                }
            }
        }

        // 가장 멀리 떨어진 두 방 찾기
        float maxDistance = 0f;
        int roomA = 0, roomB = 0;

        for (int i = 0; i < roomCount; i++) 
        {
            for (int j = i + 1; j < roomCount; j++) 
            {
                if (distances[i, j] > maxDistance) 
                {
                    maxDistance = distances[i, j];
                    roomA = i;
                    roomB = j;
                }
            }
        }

        Debug.Log($"가장 멀리 떨어진 방: Room {roomA}와 Room {roomB}, 거리: {maxDistance}");
    }

    // 큐브를 배치하는 메서드
    private void PlaceCube(Vector2Int location, Vector2Int setSize, Material material, Field curField = null) 
    {
        Vector3 worldPosition = new Vector3(location.x * unitSize, 0, location.y * unitSize);  // 위치에 유닛 크기 반영
        Vector3 worldScale = new Vector3(setSize.x * unitSize, 1, setSize.y * unitSize);  // 크기에 유닛 크기 반영

        GameObject go = Instantiate(cubePrefab, worldPosition, Quaternion.identity);
        go.GetComponent<Transform>().localScale = worldScale;
        go.GetComponentInChildren<MeshRenderer>().material = material;
        curField?.SetObject(go);
    }

    // 방을 배치하는 메서드
    private void PlaceRoom(Vector2Int location, Vector2Int setSize, Field curField) 
    {
        PlaceCube(location, setSize, redMaterial, curField);
    }

    // 복도를 배치하는 메서드
    private void PlaceHallway(Vector2Int location) 
    {
        PlaceCube(location, new Vector2Int(1, 1), blueMaterial);
    }
}
