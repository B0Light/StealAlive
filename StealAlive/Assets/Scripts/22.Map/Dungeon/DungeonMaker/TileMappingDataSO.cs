using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileMappingData", menuName = "Dungeon/Tile Mapping Data")]
public class TileMappingDataSO : ScriptableObject
{
    public List<TileTypeMapping> tileMappings;
}