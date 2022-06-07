using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JarvisAlgoStatic
{
    public JarvisAlgoStatic()
    {
    }

    public List<GameObject> JarvisAlgorithm(List<GameObject> points)
    {
        int n = points.Count;
        if (n < 3) return points;
        List<GameObject> hull = new List<GameObject>();

        int lPoint = 0;
        for (int i = 1; i < n; i++)
        {
            if (points[i].transform.position.x < points[lPoint].transform.position.x)
            {
                lPoint = i;
            }
        }

        int p = lPoint;
        int cpt = 0;
        do
        {
            cpt++;
            hull.Add(points[p]);
            var q = (p + 1) % n;
            for (int i = 0; i < n; i++)
            {
                if (Orientation(points[p].transform.position, points[i].transform.position,
                        points[q].transform.position) == 2)
                {
                    q = i;
                }
            }

            p = q;
        } while (p != lPoint);

        return hull;
    }

    public static int Orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        int val = (int)((q.x - p.x) * (r.y - p.y) -
                      (r.x - p.x) * (q.y - p.y));
        if (val == 0) return 0;
        return (val > 0) ? 1 : 2;
    }
}
