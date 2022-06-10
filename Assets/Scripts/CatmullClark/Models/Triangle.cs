using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catmull.Models{
public class Triangle
{
    public readonly Vertex V0, V1, V2;
    public readonly int VI0, VI1, VI2; // Indices des Vertex
    public readonly Edge E0, E1, E2;

    public Triangle(
        Vertex v0, Vertex v1, Vertex v2,
        Edge e0, Edge e1, Edge e2
    )
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;

        E0 = e0;
        E1 = e1;
        E2 = e2;
    }

    public Triangle(
        Vertex v0, Vertex v1, Vertex v2,
        int vi0, int vi1, int vi2,
        Edge e0, Edge e1, Edge e2
    )
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;

        VI0 = vi0;
        VI1 = vi1;
        VI2 = vi2;

        E0 = e0;
        E1 = e1;
        E2 = e2;
    }

    public Vertex GetOtherVertex(Edge e)
    {
        if (!e.Has(V0)) return V0;
        return !e.Has(V1) ? V1 : V2;
    }

    public static List<Triangle> CreateListFromMesh(Mesh mesh) {
        var trianglesList = new List<Triangle>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int index0 = mesh.triangles[i];
            int index1 = mesh.triangles[i+1];
            int index2 = mesh.triangles[i+2];

            Vertex v0 = new Vertex(mesh.vertices[index0]);
            Vertex v1 = new Vertex(mesh.vertices[mesh.triangles[i + 1]]);
            Vertex v2 = new Vertex(mesh.vertices[mesh.triangles[i + 2]]);

            Edge e0 = new Edge(v0, v1, index0, index1);
            Edge e1 = new Edge(v1, v2, index1, index2);
            Edge e2 = new Edge(v2, v0, index2, index0);

            trianglesList.Add(new Triangle(v0, v1, v2, index0, index1, index2, e0, e1, e2));
        }

        return trianglesList;
    }

    public static List<Triangle> CreateListFromArray(Vector3[] vertices, int[] triangles) {
        var trianglesList = new List<Triangle>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int index0 = triangles[i];
            int index1 = triangles[i+1];
            int index2 = triangles[i+2];

            Vertex v0 = new Vertex(vertices[index0]);
            Vertex v1 = new Vertex(vertices[triangles[i + 1]]);
            Vertex v2 = new Vertex(vertices[triangles[i + 2]]);

            Edge e0 = new Edge(v0, v1, index0, index1);
            Edge e1 = new Edge(v1, v2, index1, index2);
            Edge e2 = new Edge(v2, v0, index2, index0);

            trianglesList.Add(new Triangle(v0, v1, v2, index0, index1, index2, e0, e1, e2));
        }

        return trianglesList;
    }

    public Vector3 GetCenterPosition(Vector3[] vertices){
        return ( (vertices[VI0] + vertices[VI1] + vertices[VI2]) / 3f );
    }

    public bool Contains(Edge edge) {
        return E0.IsEqual(edge) || E1.IsEqual(edge) || E2.IsEqual(edge);
    }

    public bool Contains(int index) {
        return VI0 == index || VI1 == index || VI2 == index;
    }
}
}
