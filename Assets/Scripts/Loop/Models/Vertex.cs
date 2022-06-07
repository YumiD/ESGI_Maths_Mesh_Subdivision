using System.Collections.Generic;
using UnityEngine;

namespace Loop.Models
{
    public class Vertex
    {
        public Vector3 p;
        public readonly List<Edge> Edges;
        private readonly List<Triangle> _triangles;
        public Vertex Updated;

        // reference index to original vertex
        public readonly int Index;

        public Vertex(Vector3 p, int index = -1)
        {
            this.p = p;
            Index = index;
            Edges = new List<Edge>();
            _triangles = new List<Triangle>();
        }

        public void AddEdge(Edge e)
        {
            Edges.Add(e);
        }

        public void AddTriangle(Triangle f)
        {
            _triangles.Add(f);
        }
    }
}
