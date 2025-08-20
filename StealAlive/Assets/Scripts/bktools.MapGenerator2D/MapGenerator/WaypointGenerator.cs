using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 웨이포인트 생성기 - 맵 데이터를 기반으로 AI 패트롤용 웨이포인트를 생성
/// </summary>
public class WaypointGenerator
{
    private MapData mapData;
    private Vector3 cubeSize;
    private WaypointSystemData waypointSystem;

    public WaypointGenerator(MapData mapData, Vector3 cubeSize)
    {
        this.mapData = mapData;
        this.cubeSize = cubeSize;
        this.waypointSystem = new WaypointSystemData();
        this.waypointSystem.gridSize = mapData.gridSize;
        this.waypointSystem.cubeSize = cubeSize;
    }

    /// <summary>
    /// 웨이포인트 시스템 생성
    /// </summary>
    public WaypointSystemData GenerateWaypointSystem()
    {
        if (mapData == null || !mapData.isGenerated)
        {
            Debug.LogError("맵 데이터가 유효하지 않습니다.");
            return null;
        }

        waypointSystem.waypoints.Clear();
        waypointSystem.patrolRoutes.Clear();

        // 1. 방 중심점 웨이포인트 생성
        GenerateRoomCenterWaypoints();

        // 2. 복도 교차점 웨이포인트 생성
        GenerateCorridorIntersectionWaypoints();

        // 3. 복도 중간점 웨이포인트 생성 (긴 복도용)
        GenerateCorridorMidpointWaypoints();

        // 4. 웨이포인트 연결 관계 설정
        ConnectWaypoints();

        // 5. 패트롤 경로 생성
        GeneratePatrolRoutes();

        Debug.Log($"웨이포인트 시스템 생성 완료: {waypointSystem.waypoints.Count}개 웨이포인트, {waypointSystem.patrolRoutes.Count}개 패트롤 경로");
        
        return waypointSystem;
    }

    /// <summary>
    /// 방 중심점에 웨이포인트 생성
    /// </summary>
    private void GenerateRoomCenterWaypoints()
    {
        for (int i = 0; i < mapData.floorList.Count; i++)
        {
            RectInt room = mapData.floorList[i];
            
            // 방 중심점 계산
            Vector2Int centerGrid = new Vector2Int(
                room.x + (room.width - 1) / 2,
                room.y + (room.height - 1) / 2
            );

            Vector3 centerWorld = GridToWorldPosition(centerGrid);

            // 웨이포인트 생성
            Waypoint roomWaypoint = new Waypoint(centerWorld, centerGrid, WaypointType.Room)
            {
                roomIndex = i,
                patrolRadius = Mathf.Min(room.width, room.height) * cubeSize.x * 0.3f
            };

            waypointSystem.AddWaypoint(roomWaypoint);
        }
    }

    /// <summary>
    /// 복도 교차점에 웨이포인트 생성
    /// </summary>
    private void GenerateCorridorIntersectionWaypoints()
    {
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();

        for (int x = 1; x < mapData.gridSize.x - 1; x++)
        {
            for (int y = 1; y < mapData.gridSize.y - 1; y++)
            {
                if (processedPositions.Contains(new Vector2Int(x, y)))
                    continue;

                if (IsPathCell(x, y) && IsIntersection(x, y))
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector3 worldPos = GridToWorldPosition(gridPos);

                    Waypoint intersectionWaypoint = new Waypoint(worldPos, gridPos, WaypointType.Intersection)
                    {
                        roomIndex = -1, // 복도
                        patrolRadius = cubeSize.x * 1.5f
                    };

                    waypointSystem.AddWaypoint(intersectionWaypoint);
                    processedPositions.Add(gridPos);
                }
            }
        }
    }

    /// <summary>
    /// 긴 복도 중간에 웨이포인트 생성
    /// </summary>
    private void GenerateCorridorMidpointWaypoints()
    {
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();
        
        // 수평 복도 처리
        for (int y = 1; y < mapData.gridSize.y - 1; y++)
        {
            List<Vector2Int> horizontalPath = new List<Vector2Int>();
            
            for (int x = 1; x < mapData.gridSize.x - 1; x++)
            {
                if (IsPathCell(x, y) && !IsIntersection(x, y))
                {
                    horizontalPath.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (horizontalPath.Count >= 6) // 긴 복도인 경우
                    {
                        CreateMidpointWaypoints(horizontalPath, processedPositions);
                    }
                    horizontalPath.Clear();
                }
            }
            
            if (horizontalPath.Count >= 6)
            {
                CreateMidpointWaypoints(horizontalPath, processedPositions);
            }
        }

        // 수직 복도 처리
        for (int x = 1; x < mapData.gridSize.x - 1; x++)
        {
            List<Vector2Int> verticalPath = new List<Vector2Int>();
            
            for (int y = 1; y < mapData.gridSize.y - 1; y++)
            {
                if (IsPathCell(x, y) && !IsIntersection(x, y) && !processedPositions.Contains(new Vector2Int(x, y)))
                {
                    verticalPath.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (verticalPath.Count >= 6)
                    {
                        CreateMidpointWaypoints(verticalPath, processedPositions);
                    }
                    verticalPath.Clear();
                }
            }
            
            if (verticalPath.Count >= 6)
            {
                CreateMidpointWaypoints(verticalPath, processedPositions);
            }
        }
    }

    /// <summary>
    /// 긴 복도의 중간점들에 웨이포인트 생성
    /// </summary>
    private void CreateMidpointWaypoints(List<Vector2Int> pathPoints, HashSet<Vector2Int> processedPositions)
    {
        int interval = 4; // 4칸마다 웨이포인트 생성
        
        for (int i = interval; i < pathPoints.Count; i += interval)
        {
            Vector2Int gridPos = pathPoints[i];
            if (!processedPositions.Contains(gridPos))
            {
                Vector3 worldPos = GridToWorldPosition(gridPos);
                
                Waypoint corridorWaypoint = new Waypoint(worldPos, gridPos, WaypointType.Corridor)
                {
                    roomIndex = -1,
                    patrolRadius = cubeSize.x
                };

                waypointSystem.AddWaypoint(corridorWaypoint);
                processedPositions.Add(gridPos);
            }
        }
    }

    /// <summary>
    /// 웨이포인트들 간의 연결 관계 설정
    /// </summary>
    private void ConnectWaypoints()
    {
        float maxConnectionDistance = cubeSize.x * 8f; // 최대 연결 거리

        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            for (int j = i + 1; j < waypointSystem.waypoints.Count; j++)
            {
                Waypoint waypoint1 = waypointSystem.waypoints[i];
                Waypoint waypoint2 = waypointSystem.waypoints[j];

                float distance = Vector3.Distance(waypoint1.position, waypoint2.position);

                if (distance <= maxConnectionDistance)
                {
                    // 경로가 존재하는지 확인
                    if (HasClearPath(waypoint1.gridPosition, waypoint2.gridPosition))
                    {
                        waypointSystem.ConnectWaypoints(i, j);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 패트롤 경로 생성
    /// </summary>
    private void GeneratePatrolRoutes()
    {
        // 1. 각 방별 패트롤 경로 생성
        GenerateRoomPatrolRoutes();

        // 2. 복도 패트롤 경로 생성
        GenerateCorridorPatrolRoutes();

        // 3. 전체 맵 순회 경로 생성
        GenerateGlobalPatrolRoute();
    }

    /// <summary>
    /// 각 방별 패트롤 경로 생성
    /// </summary>
    private void GenerateRoomPatrolRoutes()
    {
        for (int roomIndex = 0; roomIndex < mapData.floorList.Count; roomIndex++)
        {
            List<int> roomWaypointIndices = new List<int>();
            
            for (int i = 0; i < waypointSystem.waypoints.Count; i++)
            {
                if (waypointSystem.waypoints[i].roomIndex == roomIndex)
                {
                    roomWaypointIndices.Add(i);
                }
            }

            if (roomWaypointIndices.Count > 0)
            {
                PatrolRoute roomRoute = new PatrolRoute($"Room_{roomIndex}_Patrol", true)
                {
                    waitTime = 2f
                };
                roomRoute.waypointIndices = roomWaypointIndices;
                waypointSystem.AddPatrolRoute(roomRoute);
            }
        }
    }

    /// <summary>
    /// 복도 패트롤 경로 생성
    /// </summary>
    private void GenerateCorridorPatrolRoutes()
    {
        List<int> corridorWaypointIndices = new List<int>();
        
        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            Waypoint waypoint = waypointSystem.waypoints[i];
            if (waypoint.type == WaypointType.Corridor || waypoint.type == WaypointType.Intersection)
            {
                corridorWaypointIndices.Add(i);
            }
        }

        if (corridorWaypointIndices.Count > 1)
        {
            PatrolRoute corridorRoute = new PatrolRoute("Corridor_Patrol", false)
            {
                waitTime = 1f
            };
            corridorRoute.waypointIndices = corridorWaypointIndices;
            waypointSystem.AddPatrolRoute(corridorRoute);
        }
    }

    /// <summary>
    /// 전체 맵 순회 경로 생성
    /// </summary>
    private void GenerateGlobalPatrolRoute()
    {
        if (waypointSystem.waypoints.Count < 2) return;

        // 모든 방 웨이포인트를 연결하는 경로 생성
        List<int> globalRoute = new List<int>();
        
        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            if (waypointSystem.waypoints[i].type == WaypointType.Room)
            {
                globalRoute.Add(i);
            }
        }

        if (globalRoute.Count > 1)
        {
            PatrolRoute globalPatrolRoute = new PatrolRoute("Global_Patrol", true)
            {
                waitTime = 3f
            };
            globalPatrolRoute.waypointIndices = globalRoute;
            waypointSystem.AddPatrolRoute(globalPatrolRoute);
        }
    }

    /// <summary>
    /// 두 지점 간에 명확한 경로가 있는지 확인
    /// </summary>
    private bool HasClearPath(Vector2Int start, Vector2Int end)
    {
        // 간단한 직선 경로 체크 (실제로는 A* 같은 알고리즘을 사용할 수 있음)
        Vector2Int direction = new Vector2Int(
            end.x > start.x ? 1 : end.x < start.x ? -1 : 0,
            end.y > start.y ? 1 : end.y < start.y ? -1 : 0
        );

        Vector2Int current = start;
        int maxSteps = Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y);
        int steps = 0;

        while (current != end && steps < maxSteps)
        {
            if (!IsWalkableCell(current.x, current.y))
                return false;

            // 다음 위치로 이동
            if (current.x != end.x)
                current.x += direction.x;
            else if (current.y != end.y)
                current.y += direction.y;

            steps++;
        }

        return current == end;
    }

    /// <summary>
    /// 교차점인지 확인
    /// </summary>
    private bool IsIntersection(int x, int y)
    {
        if (!IsPathCell(x, y)) return false;

        int pathNeighbors = 0;
        
        // 상하좌우 확인
        if (IsWalkableCell(x - 1, y)) pathNeighbors++;
        if (IsWalkableCell(x + 1, y)) pathNeighbors++;
        if (IsWalkableCell(x, y - 1)) pathNeighbors++;
        if (IsWalkableCell(x, y + 1)) pathNeighbors++;

        return pathNeighbors >= 3; // 3방향 이상 연결되면 교차점
    }

    /// <summary>
    /// 경로 셀인지 확인
    /// </summary>
    private bool IsPathCell(int x, int y)
    {
        if (x < 0 || x >= mapData.gridSize.x || y < 0 || y >= mapData.gridSize.y)
            return false;

        CellType cellType = mapData.GetCellType(x, y);
        return cellType == CellType.Path || cellType == CellType.ExpandedPath;
    }

    /// <summary>
    /// 이동 가능한 셀인지 확인
    /// </summary>
    private bool IsWalkableCell(int x, int y)
    {
        if (x < 0 || x >= mapData.gridSize.x || y < 0 || y >= mapData.gridSize.y)
            return false;

        CellType cellType = mapData.GetCellType(x, y);
        return cellType == CellType.Floor || 
               cellType == CellType.FloorCenter || 
               cellType == CellType.Path || 
               cellType == CellType.ExpandedPath;
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환
    /// </summary>
    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cubeSize.x, 0, gridPos.y * cubeSize.z);
    }

    /// <summary>
    /// 웨이포인트 시스템 데이터 반환
    /// </summary>
    public WaypointSystemData GetWaypointSystemData()
    {
        return waypointSystem;
    }
}
