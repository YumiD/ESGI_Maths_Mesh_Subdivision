using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


public class Triangulation2DIncremental : MonoBehaviour, IAlgorithm
{
    Triangulation _triangles;


    public void MainAlgorithm(List<GameObject> points)
    {
        if (points.Count < 3)
            return;

        // Step 1 : Sort by absciss ascending order 
        var copytmp = points.OrderBy(c => c.transform.position.y).ToList();
        var pointsSorted = copytmp.OrderBy(c => c.transform.position.x).ToList();

        // Step 2  : Initialize processus
        //2.a ) k-1 colinear edges from pointsSorted[0]
        List<GameObject> pointsChecked = new List<GameObject>();
        pointsChecked.Add(pointsSorted[0]);
        for (int i = 1; i < pointsSorted.Count; i++)
        {
            if (Math.Abs(pointsSorted[0].transform.position.x - pointsSorted[i].transform.position.x) < 0)
                pointsChecked.Add(pointsSorted[i]); //Colinear
            else
                break;
        }
        //2.b) Triangulation Initialize 
        int k = pointsChecked.Count;
        if (k >= 2)
        {
            GameObject kplus1;
            if (k < pointsSorted.Count)
            {
                kplus1 = pointsSorted[k]; //Equivalent du point K+1 vu que normalement les points commencent par P1 et non points[0]
                pointsChecked.Add(kplus1);
            }
            else //Exception qui ne devrait jamais arriver en temps normal
            {
                kplus1 = pointsSorted[k - 1];
                k--; //Pour pas que la boucle d'après plante
            }

            for (int i = 0; i < k; i++)
            {
                _triangles.AddTriangle(pointsSorted[i], pointsSorted[i + 1], kplus1);
            }

        }
        else //Si aucun point colinéaire n'est trouvé
        {
            _triangles.AddTriangle(pointsSorted[0], pointsSorted[1], pointsSorted[2]);
            pointsChecked = new List<GameObject>() { pointsSorted[0], pointsSorted[1], pointsSorted[2] };
        }

        //3) Triangulation Follow Up
        GameObject pqplus1;
        for (int i = pointsChecked.Count; i < pointsSorted.Count; i++)
        {
            pqplus1 = pointsSorted[i];
            List<GameObject> polygonHull = new List<GameObject>();
            JarvisAlgoStatic jarvis = new JarvisAlgoStatic();
            polygonHull = jarvis.JarvisAlgorithm(pointsChecked);

            for (int p = 0; p < polygonHull.Count; p++)
            {
                int indexNext = p + 1;
                if (p + 1 == polygonHull.Count)
                    indexNext = 0;
                Vector3 segmentHull = polygonHull[indexNext].transform.position - polygonHull[p].transform.position;
                Vector3 segmentHullNormalized = Vector3.Cross(segmentHull.normalized, -Vector3.forward);
                Vector3 segmentToPQplus1 = pqplus1.transform.position - polygonHull[p].transform.position;
                if (Vector3.Dot(segmentHullNormalized, segmentToPQplus1.normalized) > 0)
                {
                    pointsChecked.Add(pqplus1);
                    _triangles.AddTriangle(polygonHull[p], polygonHull[indexNext], pqplus1);
                }
            }
        }
    }


    public void ExecuteAlgorithm()
    {
        _triangles = new Triangulation();
        PointsManager.DeleteLines();
        List<GameObject> copy = new List<GameObject>(PointsManager.Points);
        MainAlgorithm(copy);
        _triangles.DrawTriangles();
    }
}