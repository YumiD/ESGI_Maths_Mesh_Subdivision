using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayAlgo : MonoBehaviour, IAlgorithm
{
    Triangulation _triangles;

    public void MainAlgorithm(List<GameObject> points)
    {
        throw new System.NotImplementedException();
    }

    public void ExecuteAlgorithm()
    {
        _triangles = new Triangulation();
        PointsManager.DeleteLines();
        List<GameObject> copy = new List<GameObject>(PointsManager.Points);
        MainAlgorithm(copy);
        _triangles.DrawTriangles();
    }

    public void FlippingEdges()
    {
        /*List<Edge> Ac = new List<Edge>();
        while (Ac.Count > 0)
        {
            Edge A = Ac[0];
            Ac.RemoveAt(0);
        }*/
    }
    
    
}
