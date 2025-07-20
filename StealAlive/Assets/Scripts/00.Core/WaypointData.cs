using UnityEngine;

[System.Serializable]
public struct WaypointData
{
    public Vector3[] waypoints;
    
    public WaypointData(Vector3[] waypoints)
    {
        this.waypoints = waypoints;
    }
}