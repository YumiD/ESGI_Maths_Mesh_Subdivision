using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    public static List<GameObject> Points = new List<GameObject>();
    public static List<LineRenderer> lineRenderers = new List<LineRenderer>();


    public static int Orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        int val = (int)((q.x - p.x) * (r.y - p.y) -
                      (r.x - p.x) * (q.y - p.y));
        if (val == 0) return 0;
        return (val > 0) ? 1 : 2;
    }

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

    public static void DrawAllLines()
    {
        if (Points.Count <= 0) return;
        for (int i = Points.Count-2; i >= 0; i--)
        {
            DrawLines(Points[i], Points[i+1]);
        }
    }


    public static void ExecuteChaikinAlgorithm(GameObject point)
    {
        List<GameObject> newPoints = new List<GameObject>();
        for (int i = Points.Count-2; i >= 0; i--)
        {
            GameObject p1 = Points[i];
            GameObject p2 = Points[i + 1];

            Vector2 pos1 = new Vector2(p1.transform.position.x * 0.75f + p2.transform.position.x * 0.25f, p1.transform.position.y * 0.75f + p2.transform.position.y * 0.25f);
            var newp1 = Instantiate(point, pos1, Quaternion.identity);
            Vector2 pos2 = new Vector2(p1.transform.position.x * 0.25f + p2.transform.position.x * 0.75f, p1.transform.position.y * 0.25f + p2.transform.position.y * 0.75f);
            var newp2 = Instantiate(point, pos2, Quaternion.identity);

            newPoints.Add(newp2);
            newPoints.Add(newp1);
        }
        DeletePoints();
        Points = newPoints;
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

    public static void SetActivePoints(bool visibility)
    {
        for (int i = Points.Count-1; i >= 0; i--)
        {
            Points[i].SetActive(visibility);
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