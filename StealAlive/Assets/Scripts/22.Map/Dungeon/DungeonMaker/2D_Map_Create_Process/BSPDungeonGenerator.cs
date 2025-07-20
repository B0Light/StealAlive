using System.Collections.Generic;
using UnityEngine;

public class BSPDungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int dungeonSize = new Vector2Int(64, 64); // 전체 맵 크기
    [SerializeField] private int minRoomSize = 6; // 최소 방 크기
    [SerializeField] private int maxRoomSize = 20; // 최대 방 크기
    [SerializeField] private int maxDepth = 5; // 최대 분할 깊이
    [SerializeField] private GameObject cubePrefab; // 방을 표현할 큐브 프리팹
    [SerializeField] private Vector3 unitSize = new Vector3(1, 1, 1); // 유닛 크기 설정

    private List<RoomNode> _rooms;

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        _rooms = new List<RoomNode>();

        // 전체 던전 크기로 시작하는 루트 노드 생성
        RoomNode rootNode = new RoomNode(new RectInt(0, 0, dungeonSize.x, dungeonSize.y));
        SplitNode(rootNode, 0); // 재귀적으로 분할

        // 각 노드에 방 배치
        PlaceRooms(rootNode);

        // 방의 위치에 따른 실제 던전 생성
        foreach (var room in _rooms)
        {
            Debug.Log($"Room at {room.RoomRect.position} with size {room.RoomRect.size}");
            PlaceRoom(room.RoomRect.position, room.RoomRect.size);
        }
    }

    // 노드 분할 함수
    bool SplitNode(RoomNode node, int depth)
    {
        if (depth >= maxDepth) return false; // 최대 깊이에 도달하면 더 이상 분할하지 않음

        // 현재 노드의 너비와 높이
        bool splitHorizontally = Random.Range(0, 2) == 0;

        // 방 크기가 최소 크기보다 작다면 분할하지 않음
        if (node.RoomRect.width < minRoomSize * 2 || node.RoomRect.height < minRoomSize * 2)
        {
            return false;
        }

        // 수평 분할과 수직 분할의 기준 설정
        int splitPos = splitHorizontally
            ? Random.Range(minRoomSize, node.RoomRect.height - minRoomSize)
            : Random.Range(minRoomSize, node.RoomRect.width - minRoomSize);

        // 새로운 서브노드들 생성
        if (splitHorizontally)
        {
            node.Left = new RoomNode(new RectInt(node.RoomRect.xMin, node.RoomRect.yMin, node.RoomRect.width, splitPos));
            node.Right = new RoomNode(new RectInt(node.RoomRect.xMin, node.RoomRect.yMin + splitPos, node.RoomRect.width, node.RoomRect.height - splitPos));
        }
        else
        {
            node.Left = new RoomNode(new RectInt(node.RoomRect.xMin, node.RoomRect.yMin, splitPos, node.RoomRect.height));
            node.Right = new RoomNode(new RectInt(node.RoomRect.xMin + splitPos, node.RoomRect.yMin, node.RoomRect.width - splitPos, node.RoomRect.height));
        }

        // 재귀적으로 서브노드 분할
        if (SplitNode(node.Left, depth + 1))
        {
            SplitNode(node.Right, depth + 1);
        }

        return true;
    }

    // 분할된 노드에 방 배치
    void PlaceRooms(RoomNode node)
    {
        if (node.Left != null || node.Right != null)
        {
            // 자식 노드가 있으면 자식 노드에 방을 배치
            if (node.Left != null) PlaceRooms(node.Left);
            if (node.Right != null) PlaceRooms(node.Right);
        }
        else
        {
            // 리프 노드인 경우 방 생성
            int roomWidth = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, node.RoomRect.width));
            int roomHeight = Random.Range(minRoomSize, Mathf.Min(maxRoomSize, node.RoomRect.height));

            int roomX = Random.Range(node.RoomRect.xMin, node.RoomRect.xMax - roomWidth);
            int roomY = Random.Range(node.RoomRect.yMin, node.RoomRect.yMax - roomHeight);

            node.RoomRect = new RectInt(roomX, roomY, roomWidth, roomHeight);
            _rooms.Add(node);
        }
    }
    
    // 큐브를 배치하는 메서드, 유닛 크기 반영
    private void PlaceCube(Vector2Int location, Vector2Int setSize) 
    {
        Vector3 worldPosition = new Vector3(location.x * unitSize.x, 0, location.y * unitSize.z);  // 위치에 유닛 크기 반영
        Vector3 worldScale = new Vector3(setSize.x * unitSize.x, unitSize.y, setSize.y * unitSize.z);  // 크기에 유닛 크기 반영

        GameObject go = Instantiate(cubePrefab, worldPosition, Quaternion.identity);
        go.GetComponent<Transform>().localScale = worldScale;
    }

    // 방을 배치하는 메서드
    private void PlaceRoom(Vector2Int location, Vector2Int setSize) 
    {
        PlaceCube(location, setSize);
    }
}

// RoomNode 클래스
public class RoomNode
{
    public RectInt RoomRect;
    public RoomNode Left;
    public RoomNode Right;

    public RoomNode(RectInt roomRect)
    {
        RoomRect = roomRect;
    }
}
