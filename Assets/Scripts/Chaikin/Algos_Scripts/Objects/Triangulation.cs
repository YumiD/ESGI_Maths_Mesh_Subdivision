using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulation
{
    private List<(GameObject, GameObject, GameObject)> _triangles;
    private double _circleRayon = 0;
    private Vector3 _circleCenterPoint = new Vector3();

    public Triangulation()
    {
        this._triangles = new List<(GameObject, GameObject, GameObject)>();
        calculateCircumcircle();
    }

    public void AddTriangle(GameObject point1, GameObject point2, GameObject point3)
    {
        _triangles.Add((point1, point2, point3));
    }

    public void DrawTriangles()
    {
        if (_triangles != null)
        {
            foreach ((GameObject, GameObject, GameObject) triangle in _triangles)
            {
                PointsManager.DrawLines(triangle.Item1, triangle.Item2);
                PointsManager.DrawLines(triangle.Item2, triangle.Item3);
                PointsManager.DrawLines(triangle.Item3, triangle.Item1);
            }
        }
    }

    public void calculateCircumcircle()
    {
        /*On va calculer double circleRayon et Vector3 circleCenterPoint
         */

    }

    public bool checkDelaunayCriteria(Vector3 point)
    {
        if (SqrDistance(_circleCenterPoint, point) < _circleRayon)
            return false;
        else
            return true;
    }

    public float SqrDistance(Vector3 p1, Vector3 p2)
    {
        float x = p2.x - p1.x;
        float y = p2.y - p1.y;
        float z = p2.z - p1.z;
        return x * x + y * y + z * z;
    }
}
