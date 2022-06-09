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
            float alpha;
            if (n > 3)
            {
                float center = 3f / 8f + 1f / 4f * Mathf.Cos(2f * Mathf.PI / n);
                alpha = 1f / n * (5f / 8f - Mathf.Pow(center, 2f));
            }
            else
            {
                alpha = 3f / 16f;
            }

            return alpha;
        }

        private static Vector3 GetNewPositionOfOldVertex(int n, float alpha, Vertex vertex)
        {
            return (1f - n * alpha) * vertex.Position;
        }

        public static Mesh Subdivide(Vector3[] vertices, int[] triangles, int details, bool weld)
        {
            Model model = Subdivide(vertices, triangles, details);
            Mesh mesh = model.Build(weld);
            return mesh;
        }

        private static Model Subdivide(Vector3[] vertices, int[] triangles, int details)
        {
            Model model = new Model(vertices, triangles);

            for (int i = 0; i < details; i++)
            {
                model = Divide(model);
            }

            return model;
        }

        public static Mesh RemoveDuplicateVertices(Vector3[] oldVertices, int[] oldTriangle, float threshold, float bucketStep)
        {
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            int[] old2New = new int[oldVertices.Length];
            int newSize = 0;

            // Find AABB
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < oldVertices.Length; i++)
            {
                if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
                if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
                if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
                if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
                if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
                if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
            }

            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
            int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
            int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
            List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

            // Make new vertices
            for (int i = 0; i < oldVertices.Length; i++)
            {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
                int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
                int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

                // Check to see if it's already been added
                buckets[x, y, z] ??= new List<int>();

                for (int j = 0; j < buckets[x, y, z].Count; j++)
                {
                    Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                    if (Vector3.SqrMagnitude(to) < threshold)
                    {
                        old2New[i] = buckets[x, y, z][j];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }

                // Add new vertex
                newVertices[newSize] = oldVertices[i];
                buckets[x, y, z].Add(newSize);
                old2New[i] = newSize;
                newSize++;

                skip: ;
            }

            // Make new triangles
            int[] newTriangle = new int[oldTriangle.Length];
            for (int i = 0; i < oldTriangle.Length; i++)
            {
                newTriangle[i] = old2New[oldTriangle[i]];
            }

            Vector3[] finalVertices = new Vector3[newSize];
            for (int i = 0; i < newSize; i++)
            {
                finalVertices[i] = newVertices[i];
            }

            Mesh newMesh = new Mesh
            {
                vertices = finalVertices,
                triangles = newTriangle
            };

            return newMesh;
        }

        private static Model Divide(Model model)
        {
            Model newModel = new Model();
            for (int i = 0, n = model.Triangles.Count; i < n; i++)
            {
                Triangle face = model.Triangles[i];

                Vertex newEdge0 = GetEdgePoint(face.E0);
                Vertex newEdge1 = GetEdgePoint(face.E1);
                Vertex newEdge2 = GetEdgePoint(face.E2);

                Vertex newVertex0 = GetVertexPoint(face.V0);
                Vertex newVertex1 = GetVertexPoint(face.V1);
                Vertex newVertex2 = GetVertexPoint(face.V2);

                newModel.AddTriangle(newVertex0, newEdge0, newEdge2);
                newModel.AddTriangle(newEdge0, newVertex1, newEdge1);
                newModel.AddTriangle(newEdge0, newEdge1, newEdge2);
                newModel.AddTriangle(newEdge2, newEdge1, newVertex2);
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

        private static Vertex[] GetAdjacent(Vertex v)
        {
            Vertex[] adjacent = new Vertex[v.Edges.Count];
            for (int i = 0, n = v.Edges.Count; i < n; i++)
            {
                adjacent[i] = v.Edges[i].GetOtherVertex(v);
            }

            return adjacent;
        }

        private static Vertex GetVertexPoint(Vertex vertex)
        {
            if (vertex.Updated != null) return vertex.Updated;

            Vertex[] adjacent = GetAdjacent(vertex);
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