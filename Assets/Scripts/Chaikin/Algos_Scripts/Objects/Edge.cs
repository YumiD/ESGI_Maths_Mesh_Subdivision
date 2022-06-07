using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    private (Vector3, Vector3) _edge;
    public Vector3 _p1 => _edge.Item1;
    public Vector3 _p2 => _edge.Item2;

    public Edge(Vector3 point1, Vector3 point2)
    {
        _edge = (point1, point2);
    }

    public float GetLength()
    {
        float x = _p2.x - _p1.x;
        float y = _p2.y - _p1.y;
        float z = _p2.z - _p1.z;
        return x * x + y * y + z * z;
    }
}
