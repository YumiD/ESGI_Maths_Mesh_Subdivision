using System.Collections.Generic;
using Loop.Models;
using UnityEngine;

namespace Loop
{
    public class LoopAlgorithm : ISubdiviser
    {
        private static Vertex GetBoundariesEvenVertices(Vertex vertex)
        {
            Vertex left = vertex.Edges[0].GetOtherVertex(vertex);
            Vertex right = vertex.Edges[1].GetOtherVertex(vertex);
            const float ab = 3f / 4f;
            const float cd = 1f / 8f;

            Vertex newVertex = new Vertex(ab * vertex.Position + cd * (left.Position + right.Position), vertex.Index);
            return newVertex;
        }

        private static float GetAlpha(int n)
        {
            float center = 3f / 8f + 1f / 4f * Mathf.Cos(2f * Mathf.PI / n);
            float alpha = 1f / n * (5f / 8f - Mathf.Pow(center, 2f));

            return alpha;
        }

        private static Vector3 GetNewPositionOfOldVertex(int n, float alpha, Vertex vertex)
        {
            return (1f - n * alpha) * vertex.Position;
        }

        private static Mesh Subdivide(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> triangles, int details, bool weld)
        {
            Model model = Subdivide(vertices, triangles, details);
            Mesh mesh = model.Build(weld);
            return mesh;
        }

        private static Model Subdivide(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> triangles, int details)
        {
            Model model = new Model(vertices, triangles);

            for (int i = 0; i < details; i++)
            {
                model = Divide(model);
            }

            return model;
        }

        private static Model Divide(Model model)
        {
            Model newModel = new Model();
            for (int i = 0, n = model.Triangles.Count; i < n; i++)
            {
                Triangle face = model.Triangles[i];

                Vertex newVertex0 = GetVertexPoint(face.V0);
                Vertex newVertex1 = GetVertexPoint(face.V1);
                Vertex newVertex2 = GetVertexPoint(face.V2);
                
                Vertex newEdgePoint0 = GetEdgePoint(face.E0);
                Vertex newEdgePoint1 = GetEdgePoint(face.E1);
                Vertex newEdgePoint2 = GetEdgePoint(face.E2);

                newModel.AddTriangle(newVertex0, newEdgePoint0, newEdgePoint2);
                newModel.AddTriangle(newEdgePoint0, newVertex1, newEdgePoint1);
                newModel.AddTriangle(newEdgePoint0, newEdgePoint1, newEdgePoint2);
                newModel.AddTriangle(newEdgePoint2, newEdgePoint1, newVertex2);
            }

            return newModel;
        }

        private static Vertex GetEdgePoint(Edge edge)
        {
            if (edge.EdgePoints != null) return edge.EdgePoints;

            // Odd vertices
            if (edge.Faces.Count != 2)
            {
                // boundary case for edge
                Vector3 m = (edge.A.Position + edge.B.Position) * (1f / 2f);
                edge.EdgePoints = new Vertex(m, edge.A.Index);
            }
            else
            {
                const float i = 3f / 8f;
                const float j = 1f / 8f;
                Vertex left = edge.Faces[0].GetOtherVertex(edge);
                Vertex right = edge.Faces[1].GetOtherVertex(edge);
                edge.EdgePoints =
                    new Vertex(i * (edge.A.Position + edge.B.Position) + j * (left.Position + right.Position),
                        edge.A.Index);
            }

            return edge.EdgePoints;
        }

        private static Vertex[] GetAdjacentVertex(Vertex vertex)
        {
            Vertex[] adjacent = new Vertex[vertex.Edges.Count];
            for (int i = 0, n = vertex.Edges.Count; i < n; i++)
            {
                adjacent[i] = vertex.Edges[i].GetOtherVertex(vertex);
            }

            return adjacent;
        }

        private static Vertex GetVertexPoint(Vertex vertex)
        {
            if (vertex.Updated != null) return vertex.Updated;

            Vertex[] adjacent = GetAdjacentVertex(vertex);
            int n = adjacent.Length;

            if (n < 3)
            {
                // Boundaries for even vertices
                vertex.Updated = GetBoundariesEvenVertices(vertex);
            }
            else
            {
                float alpha = GetAlpha(n);
                Vector3 newPosition = GetNewPositionOfOldVertex(n, alpha, vertex);

                for (int i = 0; i < n; i++)
                {
                    newPosition += alpha * adjacent[i].Position;
                }

                vertex.Updated = new Vertex(newPosition, vertex.Index);
            }

            return vertex.Updated;
        }

        public (Vector3[], int[]) Compute(Vector3[] vertices, int[] triangles)
        {
            Mesh mesh = Subdivide(
                vertices,
                triangles,
                1, // subdivision count
                false // a result mesh is welded or not
            );

            return SubdiviserScript.SmoothMesh(mesh.vertices, mesh.triangles);
        }
    }
}