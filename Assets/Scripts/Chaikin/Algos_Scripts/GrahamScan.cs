using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationComparer : IComparer<GameObject>
{
    private GameObject p0;

    public OrientationComparer(GameObject p0)
    {
        this.p0 = p0;
    }

    private float DistSq(Vector3 p1, Vector3 p2)
    {
        return (p1.x - p2.x) * (p1.x - p2.x) +
               (p1.y - p2.y) * (p1.y - p2.y);
    }

    public int Compare(GameObject p1, GameObject p2)
    {
        int orientation =
            PointsManager.Orientation(p0.transform.position, p1.transform.position, p2.transform.position);
        if (orientation == 0)
        {
            return (DistSq(p0.transform.position, p2.transform.position) >=
                    DistSq(p0.transform.position, p1.transform.position)
                ? -1
                : 1);
        }

        return (orientation == 2) ? -1 : 1;
    }
}

public class GrahamScan : MonoBehaviour, IAlgorithm
{
    private GameObject p0;

    private GameObject NextToTop(Stack<GameObject> stack)
    {
        GameObject p = stack.Peek();
        stack.Pop();
        GameObject res = stack.Peek();
        stack.Push(p);
        return res;
    }   

    private void GrahamScanAlgo(List<GameObject> points)
    {

        // Point le plus haut en y
        float yMin = points[0].transform.position.y;
        int min = 0;
        int n = points.Count;
        for (int i = 1; i < n; i++)
        {
            float y = points[i].transform.position.y;

            if (y < min || Math.Abs(yMin - y) < 0.01 && points[i].transform.position.x < points[min].transform.position.x)
            {
                yMin = points[i].transform.position.y;
                min = i;
            }
        }

        (points[0], points[min]) = (points[min], points[0]);

        p0 = points[0];

        OrientationComparer oc = new OrientationComparer(p0);

        points.Sort(1, points.Count-1, oc);

        int m = 1;
        for (int i = 1; i < n; i++)
        {
            while (i < n - 1 && PointsManager.Orientation(p0.transform.position, points[i].transform.position,
                       points[i + 1].transform.position) == 0)
            {
                i++;
            }
        
            points[m] = points[i];
            m++;
        }
        
        if (m < 3)
        {
            return;
        }
        
        Stack<GameObject> stack = new Stack<GameObject>();
        stack.Push(points[0]);
        stack.Push(points[1]);
        stack.Push(points[2]); 
        
        for (int i = 3; i < m; i++)
        {
            while (PointsManager.Orientation(NextToTop(stack).transform.position, stack.Peek().transform.position,
                       points[i].transform.position) != 2)
            {
                stack.Pop();
            }
            stack.Push(points[i]);
        }
        
        GameObject firstPoint = stack.Peek();
        while (stack.Count > 0)
        {
            GameObject p1 = stack.Pop();
            if (stack.Count <= 0)
            {
                PointsManager.DrawLines(p1, firstPoint);
            }
            else
            {
                GameObject p2 = stack.Peek();
                PointsManager.DrawLines(p1, p2);
            }
        }
    }
    
    public void MainAlgorithm(List<GameObject> points)
    {
        GrahamScanAlgo(points);
    }

    public void ExecuteAlgorithm()
    {
        PointsManager.DeleteLines();
        List<GameObject> copy = new List<GameObject>(PointsManager.Points);
        MainAlgorithm(copy);
    }
}