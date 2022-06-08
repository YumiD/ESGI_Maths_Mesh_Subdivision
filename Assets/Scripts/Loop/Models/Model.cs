using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Loop.Models
{
    public class Model
    {
        private readonly List<Vertex> _vertices;
        private readonly List<Edge> _edges;
        public readonly List<Triangle> Triangles;

        public Model()
        {
            _vertices = new List<Vertex>();
            _edges = new List<Edge>();
            Triangles = new List<Triangle>();
        }

        public Model(Mesh mesh) : this()
        {
            Vector3[] points = mesh.vertices;
            for (int i = 0, n = points.Length; i < n; i++)
            {
                Vertex vertex = new Vertex(points[i], i);
                _vertices.Add(vertex);
            }

            int[] triangles = mesh.triangles;
            for (int i = 0, n = triangles.Length; i < n; i += 3)
            {
                int i0 = triangles[i], i1 = triangles[i + 1], i2 = triangles[i + 2];
                Vertex v0 = _vertices[i0], v1 = _vertices[i1], v2 = _vertices[i2];

                Edge e0 = GetEdge(_edges, v0, v1);
                Edge e1 = GetEdge(_edges, v1, v2);
                Edge e2 = GetEdge(_edges, v2, v0);
                Triangle face = new Triangle(v0, v1, v2, e0, e1, e2);

                Triangles.Add(face);

                e0.AddTriangle(face);
                e1.AddTriangle(face);
                e2.AddTriangle(face);
            }
        }

        private static Edge GetEdge(ICollection<Edge> edges, Vertex v0, Vertex v1)
        {
            Edge match = v0.Edges.Find(e => e.Has(v1));
            if (match != null) return match;

            Edge newEdge = new Edge(v0, v1);
            v0.AddEdge(newEdge);
            v1.AddEdge(newEdge);
            edges.Add(newEdge);
            return newEdge;
        }

        public void AddTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            if (!_vertices.Contains(v0)) _vertices.Add(v0);
            if (!_vertices.Contains(v1)) _vertices.Add(v1);
            if (!_vertices.Contains(v2)) _vertices.Add(v2);

            Edge e0 = GetEdge(v0, v1);
            Edge e1 = GetEdge(v1, v2);
            Edge e2 = GetEdge(v2, v0);
            Triangle face = new Triangle(v0, v1, v2, e0, e1, e2);

            Triangles.Add(face);

            e0.AddTriangle(face);
            e1.AddTriangle(face);
            e2.AddTriangle(face);
        }

        private Edge GetEdge(Vertex v0, Vertex v1)
        {
            Edge match = v0.Edges.Find(e => e.A == v1 || e.B == v1);
            if (match != null) return match;

            Edge ne = new Edge(v0, v1);
            _edges.Add(ne);
            v0.AddEdge(ne);
            v1.AddEdge(ne);
            return ne;
        }

        public Mesh Build(bool weld = false)
        {
            Mesh mesh = new Mesh();
            int[] triangles = new int[Triangles.Count * 3];

            if (weld)
            {
                for (int i = 0, n = Triangles.Count; i < n; i++)
                {
                    Triangle triangle = Triangles[i];
                    triangles[i * 3] = _vertices.IndexOf(triangle.V0);
                    triangles[i * 3 + 1] = _vertices.IndexOf(triangle.V1);
                    triangles[i * 3 + 2] = _vertices.IndexOf(triangle.V2);
                }

                mesh.vertices = _vertices.Select(v => v.Position).ToArray();
            }
            else
            {
                Vector3[] vertices = new Vector3[Triangles.Count * 3];
                for (int i = 0, n = Triangles.Count; i < n; i++)
                {
                    Triangle face = Triangles[i];
                    int i0 = i * 3, i1 = i * 3 + 1, i2 = i * 3 + 2;
                    vertices[i0] = face.V0.Position;
                    vertices[i1] = face.V1.Position;
                    vertices[i2] = face.V2.Position;
                    triangles[i0] = i0;
                    triangles[i1] = i1;
                    triangles[i2] = i2;
                }

                mesh.vertices = vertices;
            }

            mesh.indexFormat = mesh.vertexCount < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}