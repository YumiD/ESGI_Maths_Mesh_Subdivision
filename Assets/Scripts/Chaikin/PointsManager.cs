using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    public static List<GameObject> Points = new List<GameObject>();
    public static List<LineRenderer> lineRenderers = new List<LineRenderer>();

    public List<GameObject> Algo;
    private static List<IAlgorithm> _algorithmes = new List<IAlgorithm>();

    // public enum AlgoChoice
    // {
    //     GrahamScan = 0,
    //     Jarvis = 1,
    //     TriangulationIncrémentale = 2
    //     TriangulationDelaunay = 3
    // }
    //
    // public static AlgoChoice Choice;
    public static int Choice;

    private void Start()
    {
        // _algorithmes = FindObjectsOfType<MonoBehaviour>().OfType<IAlgorithm>().ToList();
        foreach (var algo in Algo)
        {
            _algorithmes.Add(algo.GetComponent<IAlgorithm>());
        }
    }

    public static int Orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        /*int val = (int)((q.y - p.y) * (r.x - q.x) -
                        (q.x - p.x) * (r.y - q.y));*/ //CRINGE  
        int val = (int)((q.x - p.x) * (r.y - p.y) -
                      (r.x - p.x) * (q.y - p.y));
        if (val == 0) return 0;
        return (val > 0) ? 1 : 2;
    }
    
    //Initialize lines to draw
    private static void InitLines(LineRenderer lr)
    {
        lr.name = "Segment";
        lr.startWidth = .1f;
        lr.endWidth = .1f;
        lr.sortingOrder = -1;
        lr.material = new Material(Shader.Find("Sprites/Default")) { color = Color.black };
    }

    public static void DrawLines(GameObject p1, GameObject p2)
    {
        var go = new GameObject();
        var lr = go.AddComponent<LineRenderer>();
        InitLines(lr);
        lr.SetPosition(0, p1.transform.position);
        lr.SetPosition(1, p2.transform.position);
        
        lineRenderers.Add(lr);
    }

    public static void ExecuteAlgorithm()
    {
        _algorithmes[(int)Choice].ExecuteAlgorithm();
    }

    public static void DeleteLines()
    {
        if (lineRenderers.Count <= 0) return;
        for (int i = lineRenderers.Count-1; i >= 0; i--)
        {
            Destroy(lineRenderers[i].gameObject);
            lineRenderers.RemoveAt(i);
        }
    }
    
    public static void DeletePoints()
    {
        if (Points.Count <= 0) return;
        for (int i = Points.Count-1; i >= 0; i--)
        {
            Destroy(Points[i].gameObject);
            Points.RemoveAt(i);
        }
    }

    public static void CancelPoints()
    {
        if (Points.Count <= 0) return;
        var lastPointIndex = Points.Count - 1;
        Destroy(Points[lastPointIndex].gameObject);
        Points.RemoveAt(lastPointIndex);
    }
}