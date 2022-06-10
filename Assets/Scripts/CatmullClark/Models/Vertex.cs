using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catmull.Models{
public class Vertex
{
    public Vector3 Position;
    public readonly List<Edge> Edges;
    public Vertex Updated;

    // reference index to original vertex
    public readonly int Index;

    public Vertex(Vector3 position, int index = -1)
    {
        Position = position;
        Index = index;
        Edges = new List<Edge>();
    }

    public void AddEdge(Edge e)
    {
        Edges.Add(e);
    }
}
}