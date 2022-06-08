namespace Loop.Models
{
    public class Triangle
    {
        public readonly Vertex V0;
        public readonly Vertex V1;
        public readonly Vertex V2;
        public readonly Edge E0;
        public readonly Edge E1;
        public readonly Edge E2;

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

        public Vertex GetOtherVertex(Edge e)
        {
            if (!e.Has(V0)) return V0;
            return !e.Has(V1) ? V1 : V2;
        }
    }
}