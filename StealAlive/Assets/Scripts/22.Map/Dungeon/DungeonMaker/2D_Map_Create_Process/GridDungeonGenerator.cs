using System.Collections.Generic;
using UnityEngine;

public class GridDungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize = new Vector2Int(64, 64); // 전체 맵 크기
    [SerializeField] private int minRoomSize = 4; // 최소 방 크기
    [SerializeField] private int maxRoomSize = 12; // 최대 방 크기
    [SerializeField] private int roomCount = 10; // 방의 개수
    [SerializeField] private GameObject floorPrefab; // 방 타일 프리팹
    [SerializeField] private GameObject wallPrefab; // 벽 타일 프리팹

    private CellType[,] grid;

    private enum CellType
    {
        Empty,
        Floor,
        Wall
    }

    void Start()
    {
        GenerateGrid();
        PlaceRooms();
        BuildWalls();
        RenderGrid();
    }

    void GenerateGrid()
    {
        grid = new CellType[gridSize.x, gridSize.y];

        // 그리드 초기화 (모든 셀을 Empty로 설정)
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                grid[x, y] = CellType.Empty;
            }
        }
    }

    void PlaceRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            // 방 크기와 위치 랜덤 생성
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize);

            int roomX = Random.Range(1, gridSize.x - roomWidth - 1); // 벽을 위한 여유 공간 확보
            int roomY = Random.Range(1, gridSize.y - roomHeight - 1);

            // 방을 그리드에 배치 (방이 겹치는지 확인하지 않고 단순히 배치)
            for (int x = roomX; x < roomX + roomWidth; x++)
            {
                for (int y = roomY; y < roomY + roomHeight; y++)
                {
                    grid[x, y] = CellType.Floor;
                }
            }
        }
    }

    void BuildWalls()
    {
        // 벽 배치: 바닥 셀 주변이 빈 셀이라면 그곳에 벽을 배치
        for (int x = 1; x < gridSize.x - 1; x++)
        {
            for (int y = 1; y < gridSize.y - 1; y++)
            {
                if (grid[x, y] == CellType.Floor)
                {
                    // 상하좌우 체크
                    if (grid[x - 1, y] == CellType.Empty) grid[x - 1, y] = CellType.Wall;
                    if (grid[x + 1, y] == CellType.Empty) grid[x + 1, y] = CellType.Wall;
                    if (grid[x, y - 1] == CellType.Empty) grid[x, y - 1] = CellType.Wall;
                    if (grid[x, y + 1] == CellType.Empty) grid[x, y + 1] = CellType.Wall;
                }
            }
        }
    }

    void RenderGrid()
    {
        // 그리드를 시각적으로 표현
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 position = new Vector3(x, 0, y);
                if (grid[x, y] == CellType.Floor)
                {
                    Instantiate(floorPrefab, position, Quaternion.identity);
                }
                else if (grid[x, y] == CellType.Wall)
                {
                    Instantiate(wallPrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
