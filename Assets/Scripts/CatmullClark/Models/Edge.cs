using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catmull.Models{
public class Edge
{
    public readonly Vertex A, B;
    public readonly int AI, BI; // Indices des Vertex
    public readonly List<Triangle> Faces;
    public Vertex EdgePoints;

    public Edge(Vertex a, Vertex b)
    {
        A = a;
        B = b;
        Faces = new List<Triangle>();
    }

    public Edge(Vertex a, Vertex b, int ai, int bi)
    {
        A = a;
        B = b;
        AI = ai;
        BI = bi;
        Faces = new List<Triangle>();
    }

    public void AddTriangle(Triangle f)
    {
        Faces.Add(f);
    }

    public bool Has(Vertex v)
    {
        return v == A || v == B;
    }

    public Vertex GetOtherVertex(Vertex v)
    {
        return A != v ? A : B;
    }

    public static List<Edge> CreateListFromMesh(Mesh mesh) {
        var edgesList = new List<Edge>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int index0 = mesh.triangles[i];
            int index1 = mesh.triangles[i+1];
            int index2 = mesh.triangles[i+2];

            Vertex v0 = new Vertex(mesh.vertices[index0]);
            Vertex v1 = new Vertex(mesh.vertices[index1]);
            Vertex v2 = new Vertex(mesh.vertices[index2]);

            edgesList.Add(new Edge(v0, v1, index0, index1));
            edgesList.Add(new Edge(v1, v2, index1, index2));
            edgesList.Add(new Edge(v2, v0, index2, index0));
        }

        return edgesList;
    }
    public static List<Edge> CreateListFromArray(Vector3[] vertices, int[] triangles) {
        var edgesList = new List<Edge>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int index0 = triangles[i];
            int index1 = triangles[i+1];
            int index2 = triangles[i+2];

            Vertex v0 = new Vertex(vertices[index0]);
            Vertex v1 = new Vertex(vertices[index1]);
            Vertex v2 = new Vertex(vertices[index2]);

            edgesList.Add(new Edge(v0, v1, index0, index1));
            edgesList.Add(new Edge(v1, v2, index1, index2));
            edgesList.Add(new Edge(v2, v0, index2, index0));
        }

        return edgesList;
    }
    public bool IsEqual(Edge e2) {
        if (e2 == null) return false;
        if (AI == e2.AI && BI == e2.BI) return true ;
        if (AI == e2.BI && BI == e2.AI) return true ;
        return false;
    }

    public Vector3 GetCenterPosition(Vector3[] vertices){
        return ( (vertices[AI] + vertices[BI]) / 2f );
    }

    public Vertex GetCommonVertex(Edge edge){
        if(A == edge.A || A == edge.B ) return A;
        if(B == edge.A || B == edge.B ) return B;
        return null;
    }

    public int GetCommonVertexIndex(Edge edge){
        if(AI == edge.AI || AI == edge.BI ) return AI;
        if(BI == edge.AI || BI == edge.BI ) return BI;
        return -1;
    }
}
}
