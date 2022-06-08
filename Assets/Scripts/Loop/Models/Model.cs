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
                Vertex v = new Vertex(points[i], i);
                _vertices.Add(v);
            }

            int[] triangles = mesh.triangles;
            for (int i = 0, n = triangles.Length; i < n; i += 3)
            {
                int i0 = triangles[i], i1 = triangles[i + 1], i2 = triangles[i + 2];
                Vertex v0 = _vertices[i0], v1 = _vertices[i1], v2 = _vertices[i2];

                Edge e0 = GetEdge(_edges, v0, v1);
                Edge e1 = GetEdge(_edges, v1, v2);
                Edge e2 = GetEdge(_edges, v2, v0);
                Triangle f = new Triangle(v0, v1, v2, e0, e1, e2);

                Triangles.Add(f);

                v0.AddTriangle(f);
                v1.AddTriangle(f);
                v2.AddTriangle(f);

                e0.AddTriangle(f);
                e1.AddTriangle(f);
                e2.AddTriangle(f);
            }
        }

        private static Edge GetEdge(ICollection<Edge> edges, Vertex v0, Vertex v1)
        {
            Edge match = v0.Edges.Find(e => e.Has(v1));
            if (match != null) return match;

            Edge ne = new Edge(v0, v1);
            v0.AddEdge(ne);
            v1.AddEdge(ne);
            edges.Add(ne);
            return ne;
        }

        public void AddTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            if (!_vertices.Contains(v0)) _vertices.Add(v0);
            if (!_vertices.Contains(v1)) _vertices.Add(v1);
            if (!_vertices.Contains(v2)) _vertices.Add(v2);

            Edge e0 = GetEdge(v0, v1);
            Edge e1 = GetEdge(v1, v2);
            Edge e2 = GetEdge(v2, v0);
            Triangle f = new Triangle(v0, v1, v2, e0, e1, e2);

            Triangles.Add(f);

            v0.AddTriangle(f);
            v1.AddTriangle(f);
            v2.AddTriangle(f);

            e0.AddTriangle(f);
            e1.AddTriangle(f);
            e2.AddTriangle(f);
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
                    Triangle f = Triangles[i];
                    triangles[i * 3] = _vertices.IndexOf(f.V0);
                    triangles[i * 3 + 1] = _vertices.IndexOf(f.V1);
                    triangles[i * 3 + 2] = _vertices.IndexOf(f.V2);
                }

                mesh.vertices = _vertices.Select(v => v.p).ToArray();
            }
            else
            {
                Vector3[] vertices = new Vector3[Triangles.Count * 3];
                for (int i = 0, n = Triangles.Count; i < n; i++)
                {
                    Triangle f = Triangles[i];
                    int i0 = i * 3, i1 = i * 3 + 1, i2 = i * 3 + 2;
                    vertices[i0] = f.V0.p;
                    vertices[i1] = f.V1.p;
                    vertices[i2] = f.V2.p;
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