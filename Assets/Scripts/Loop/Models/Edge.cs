using System.Collections.Generic;

namespace Loop.Models
{
    public class Edge
    {
        public readonly Vertex A;
        public readonly Vertex B;
        public readonly List<Triangle> Faces;
        public Vertex EdgePoints;

        public Edge(Vertex a, Vertex b)
        {
            A = a;
            B = b;
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
    }
}