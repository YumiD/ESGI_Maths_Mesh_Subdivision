using System.Collections.Generic;
using Loop.Models;
using UnityEngine;

namespace Loop
{
    public class LoopAlgorithm
    {
        public static Vertex GetNewEdgePoint(Edge e)
        {
            const float ab = 3f / 8f;
            const float cd = 1f / 8f;
            Vertex left = e.Faces[0].GetOtherVertex(e);
            Vertex right = e.Faces[1].GetOtherVertex(e);
        
            Vertex newVertex = new Vertex(ab * e.A.p + e.B.p + cd * left.p + right.p, e.A.Index);
            return newVertex;
        }
        
        private static float GetAlpha(int n)
        {
            float alpha;
            if (n != 3)
            {
                float center = 3f / 8f + 1f / 4f * Mathf.Cos(2f*Mathf.PI / n);
                alpha = 1f / n * (5f / 8f - Mathf.Pow(center, 2f));
            }
            else
            {
                alpha = 3f / 16f;
            }
            
            return alpha;
        }
        
        private static Vector3 GetNewPositionOfOldVertex(int n, float alpha, Vertex v)
        {
            return (1f - n * alpha) * v.p;
        }
        
        public float GetBeta(int n, float alpha)
        {
            return (1 - n) * (1 - alpha);
        }
        
        public static Mesh Subdivide(Mesh source, int details = 1, bool weld = false)
        {
            Model model = Subdivide(source, details);
            Mesh mesh = model.Build(weld);
            return mesh;
        }
        
        private static Model Subdivide(Mesh source, int details = 1)
        {
            Model model = new Model(source);
        
            for (int i = 0; i < details; i++)
            {
                model = Divide(model);
            }
        
            return model;
        }
        
        public static Mesh Weld(Mesh mesh, float threshold, float bucketStep)
        {
            Vector3[] oldVertices = mesh.vertices;
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
            int[] oldTriangle = mesh.triangles;
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
        
            mesh.Clear();
            mesh.vertices = finalVertices;
            mesh.triangles = newTriangle;
            mesh.RecalculateNormals();
        
            return mesh;
        }
        
        public Edge GetEdge(List<Edge> edges, Vertex v0, Vertex v1)
        {
            Edge match = v0.Edges.Find(e => e.Has(v1));
            if (match != null) return match;
        
            Edge ne = new Edge(v0, v1);
            v0.AddEdge(ne);
            v1.AddEdge(ne);
            edges.Add(ne);
            return ne;
        }
        
        private static Model Divide(Model model)
        {
            Model nModel = new Model();
            for (int i = 0, n = model.Triangles.Count; i < n; i++)
            {
                Triangle f = model.Triangles[i];
        
                Vertex newEdge0 = GetEdgePoint(f.E0);
                Vertex newEdge1 = GetEdgePoint(f.E1);
                Vertex newEdge2 = GetEdgePoint(f.E2);
        
                Vertex newVertex0 = GetVertexPoint(f.V0);
                Vertex newVertex1 = GetVertexPoint(f.V1);
                Vertex newVertex2 = GetVertexPoint(f.V2);
        
                nModel.AddTriangle(newVertex0, newEdge0, newEdge2);
                nModel.AddTriangle(newEdge0, newVertex1, newEdge1);
                nModel.AddTriangle(newEdge0, newEdge1, newEdge2);
                nModel.AddTriangle(newEdge2, newEdge1, newVertex2);
            }
        
            return nModel;
        }
        
        private static Vertex GetEdgePoint(Edge edge)
        {
            if (edge.EdgePoints != null) return edge.EdgePoints;
        
            if (edge.Faces.Count != 2)
            {
                // boundary case for edge
                Vector3 m = (edge.A.p + edge.B.p) * 0.5f;
                edge.EdgePoints = new Vertex(m, edge.A.Index);
            }
            else
            {
                const float alpha = 3f / 8f;
                const float beta = 1f / 8f;
                Vertex left = edge.Faces[0].GetOtherVertex(edge);
                Vertex right = edge.Faces[1].GetOtherVertex(edge);
                edge.EdgePoints = new Vertex((edge.A.p + edge.B.p) * alpha + (left.p + right.p) * beta, edge.A.Index);
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
                Vertex left = vertex.Edges[0].GetOtherVertex(vertex);
                Vertex right = vertex.Edges[1].GetOtherVertex(vertex);
                const float ab = 3f / 4f;
                const float cd = 1f / 8f;   
                vertex.Updated = new Vertex(ab * vertex.p + cd * (left.p + right.p), vertex.Index);
                // v.Updated = GetNewEdgePoint(adjacent);
            }
            else
            {
                float alpha = GetAlpha(n);
                Vector3 newPosition = GetNewPositionOfOldVertex(n, alpha, vertex);
        
                for (int i = 0; i < n; i++)
                {
                    newPosition += alpha * adjacent[i].p;
                }
        
                vertex.Updated = new Vertex(newPosition, vertex.Index);
            }
        
            return vertex.Updated;
        }
        
        //  public static Mesh Subdivide(Mesh source, int details = 1, bool weld = false)
        // {
        //     var model = Subdivide(source, details);
        //     var mesh = model.Build(weld);
        //     return mesh;
        // }
        //
        // public static Model Subdivide(Mesh source, int details = 1)
        // {
        //     var model = new Model(source);
        //     var divider = new LoopAlgorithm();
        //
        //     for (int i = 0; i < details; i++) {
        //         model = divider.Divide(model);
        //     }
        //
        //     return model;
        // }
        //
        // public static Mesh Weld(Mesh mesh, float threshold, float bucketStep)
        // {
        //     Vector3[] oldVertices = mesh.vertices;
        //     Vector3[] newVertices = new Vector3[oldVertices.Length];
        //     int[] old2new = new int[oldVertices.Length];
        //     int newSize = 0;
        //
        //     // Find AABB
        //     Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        //     Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        //     for (int i = 0; i < oldVertices.Length; i++)
        //     {
        //         if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
        //         if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
        //         if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
        //         if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
        //         if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
        //         if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        //     }
        //
        //     // Make cubic buckets, each with dimensions "bucketStep"
        //     int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        //     int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        //     int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        //     List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];
        //
        //     // Make new vertices
        //     for (int i = 0; i < oldVertices.Length; i++)
        //     {
        //         // Determine which bucket it belongs to
        //         int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
        //         int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
        //         int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);
        //
        //         // Check to see if it's already been added
        //         if (buckets[x, y, z] == null) buckets[x, y, z] = new List<int>(); // Make buckets lazily
        //
        //         for (int j = 0; j < buckets[x, y, z].Count; j++)
        //         {
        //             Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
        //             if (Vector3.SqrMagnitude(to) < threshold)
        //             {
        //                 old2new[i] = buckets[x, y, z][j];
        //                 goto skip; // Skip to next old vertex if this one is already there
        //             }
        //         }
        //
        //         // Add new vertex
        //         newVertices[newSize] = oldVertices[i];
        //         buckets[x, y, z].Add(newSize);
        //         old2new[i] = newSize;
        //         newSize++;
        //
        //         skip:;
        //     }
        //
        //     // Make new triangles
        //     int[] oldTris = mesh.triangles;
        //     int[] newTris = new int[oldTris.Length];
        //     for (int i = 0; i < oldTris.Length; i++)
        //     {
        //         newTris[i] = old2new[oldTris[i]];
        //     }
        //
        //     Vector3[] finalVertices = new Vector3[newSize];
        //     for (int i = 0; i < newSize; i++)
        //     {
        //         finalVertices[i] = newVertices[i];
        //     }
        //
        //     mesh.Clear();
        //     mesh.vertices = finalVertices;
        //     mesh.triangles = newTris;
        //     mesh.RecalculateNormals();
        //
        //     return mesh;
        // }
        //
        // public Edge GetEdge(List<Edge> edges, Vertex v0, Vertex v1)
        // {
        //     var match = v0.Edges.Find(e => {
        //         return e.Has(v1);
        //     });
        //     if (match != null) return match;
        //
        //     var ne = new Edge(v0, v1);
        //     v0.AddEdge(ne);
        //     v1.AddEdge(ne);
        //     edges.Add(ne);
        //     return ne;
        // }
        //
        // Model Divide(Model model)
        // {
        //     var nmodel = new Model();
        //     for (int i = 0, n = model.Triangles.Count; i < n; i++)
        //     {
        //         var f = model.Triangles[i];
        //
        //         var ne0 = GetEdgePoint(f.E0);
        //         var ne1 = GetEdgePoint(f.E1);
        //         var ne2 = GetEdgePoint(f.E2);
        //
        //         var nv0 = GetVertexPoint(f.V0);
        //         var nv1 = GetVertexPoint(f.V1);
        //         var nv2 = GetVertexPoint(f.V2);
        //
        //         nmodel.AddTriangle(nv0, ne0, ne2);
        //         nmodel.AddTriangle(ne0, nv1, ne1);
        //         nmodel.AddTriangle(ne0, ne1, ne2);
        //         nmodel.AddTriangle(ne2, ne1, nv2);
        //     }
        //     return nmodel;
        // }
        //
        // public Vertex GetEdgePoint(Edge e)
        // {
        //     if (e.EdgePoints != null) return e.EdgePoints;
        //
        //     if(e.Faces.Count != 2) {
        //         // boundary case for edge
        //         var m = (e.A.p + e.B.p) * 0.5f;
        //         e.EdgePoints = new Vertex(m, e.A.Index);
        //     } else
        //     {
        //         const float alpha = 3f / 8f;
        //         const float beta = 1f / 8f;
        //         var left = e.Faces[0].GetOtherVertex(e);
        //         var right = e.Faces[1].GetOtherVertex(e);
        //         e.EdgePoints = new Vertex((e.A.p + e.B.p) * alpha + (left.p + right.p) * beta, e.A.Index);
        //     }
        //
        //     return e.EdgePoints;
        // }
        //
        // public Vertex[] GetAdjancies(Vertex v)
        // {
        //     var adjancies = new Vertex[v.Edges.Count];
        //     for(int i = 0, n = v.Edges.Count; i < n; i++)
        //     {
        //         adjancies[i] = v.Edges[i].GetOtherVertex(v);
        //     }
        //     return adjancies;
        // }
        //
        // public Vertex GetVertexPoint(Vertex v)
        // {
        //     if (v.Updated != null) return v.Updated;
        //
        //     var adjancies = GetAdjancies(v);
        //     var n = adjancies.Length;
        //     if(n < 3)
        //     {
        //         // boundary case for vertex
        //         var e0 = v.Edges[0].GetOtherVertex(v);
        //         var e1 = v.Edges[1].GetOtherVertex(v);
        //         const float k0 = (3f / 4f);
        //         const float k1 = (1f / 8f);
        //         v.Updated = new Vertex(k0 * v.p + k1 * (e0.p + e1.p), v.Index);
        //     } else
        //     {
        //         const float pi2 = Mathf.PI * 2f;
        //         const float k0 = (5f / 8f);
        //         const float k1 = (3f / 8f);
        //         const float k2 = (1f / 4f);
        //         var alpha = (n == 3) ? (3f / 16f) : ((1f / n) * (k0 - Mathf.Pow(k1 + k2 * Mathf.Cos(pi2 / n), 2f)));
        //
        //         var np = (1f - n * alpha) * v.p;
        //
        //         for(int i = 0; i < n; i++)
        //         {
        //             var adj = adjancies[i];
        //             np += alpha * adj.p;
        //         }
        //
        //         v.Updated = new Vertex(np, v.Index);
        //     }
        //
        //     return v.Updated;
        // }

    }
}