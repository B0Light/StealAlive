using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FieldMapGenerator : DungeonMapGenerator
{
    // Start is called before the first frame update

    protected override void CreatePath()
    {
        var edges = _delaunay.Edges.Select(edge => new Kruskal.Edge(edge.U, edge.V)).ToList();
        _selectedEdges = new HashSet<Kruskal.Edge>(edges);
    }

    public Vector3 GetCubeSize()
    {
        return cubeSize;
    }
}
