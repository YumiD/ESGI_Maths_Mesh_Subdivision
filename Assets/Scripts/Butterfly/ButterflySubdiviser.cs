using System.Collections.Generic;
using UnityEngine;

namespace Butterfly
{
	public class ButterflySubdiviser : ISubdiviser
	{
		private static void RegisterTriangle(Dictionary<Edge, List<int>> trianglePairs, int v1, int v2, int index)
		{
			Edge edge = new Edge
			{
				V1 = v1,
				V2 = v2
			};

			if (!trianglePairs.TryGetValue(edge, out List<int> indices))
			{
				indices = new List<int>(2);
				trianglePairs.Add(edge, indices);
			}

			indices.Add(index);
		}

		private static int GetVertexOppositeToEdge(int edgeV1, int edgeV2, int v1, int v2, int v3)
		{
			if (v1 != edgeV1 && v1 != edgeV2)
			{
				return v1;
			}
			
			if (v2 != edgeV1 && v2 != edgeV2)
			{
				return v2;
			}
			
			if (v3 != edgeV1 && v3 != edgeV2)
			{
				return v3;
			}

			return -1;
		}

		private static Vector3 Accumulate2(Dictionary<Edge, List<int>> trianglePairs, Edge edge, int triangle, Vector3[] vertices, int[] triangles)
		{
			Vector3 accumulator = Vector3.zero;
			
			List<int> pair = trianglePairs[edge];
			int outerTriangle = pair[0] == triangle ? pair[1] : pair[0];
			
			int outer1 = GetVertexOppositeToEdge(
				edge.V1,
				edge.V2,
				triangles[outerTriangle*3+0],
				triangles[outerTriangle*3+1],
				triangles[outerTriangle*3+2]
			);
			accumulator -= vertices[outer1] * (1.0f / 16.0f);

			return accumulator;
		}

		private static Vector3 Accumulate1(Dictionary<Edge, List<int>> trianglePairs, Edge edge, int triangle, Vector3[] vertices, int[] triangles)
		{
			Vector3 accumulator = Vector3.zero;
			
			int opposite = GetVertexOppositeToEdge(
				edge.V1,
				edge.V2,
				triangles[triangle*3+0],
				triangles[triangle*3+1],
				triangles[triangle*3+2]
			);
			accumulator += vertices[opposite] * (1.0f / 8.0f);
			
			Edge edge1 = new Edge
			{
				V1 = edge.V1,
				V2 = opposite
			};
			accumulator += Accumulate2(trianglePairs, edge1, triangle, vertices, triangles);
			
			Edge edge2 = new Edge
			{
				V1 = edge.V2,
				V2 = opposite
			};
			accumulator += Accumulate2(trianglePairs, edge2, triangle, vertices, triangles);

			return accumulator;
		}
		
		public (Vector3[], int[]) Compute(Vector3[] vertices, int[] triangles)
		{
			Dictionary<Edge, List<int>> trianglePairs = new Dictionary<Edge, List<int>>();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				RegisterTriangle(
					trianglePairs,
					triangles[i*3+0],
					triangles[i*3+1],
					i);
				RegisterTriangle(
					trianglePairs,
					triangles[i*3+1],
					triangles[i*3+2],
					i);
				RegisterTriangle(
					trianglePairs,
					triangles[i*3+2],
					triangles[i*3+0],
					i);
			}

			Dictionary<Edge, Vector3> edgePoints = new Dictionary<Edge, Vector3>();
			foreach (KeyValuePair<Edge, List<int>> pair in trianglePairs)
			{
				Vector3 accumulator = Vector3.zero;

				Edge edge = pair.Key;

				accumulator += vertices[edge.V1] * (1.0f / 2.0f);
				accumulator += vertices[edge.V2] * (1.0f / 2.0f);

				accumulator += Accumulate1(trianglePairs, edge, pair.Value[0], vertices, triangles);
				accumulator += Accumulate1(trianglePairs, edge, pair.Value[1], vertices, triangles);

				edgePoints.Add(edge, accumulator);
			}

			MeshGenerator generator = new MeshGenerator();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				int v1Index = triangles[i*3+0];
				int v2Index = triangles[i*3+1];
				int v3Index = triangles[i*3+2];
				
				Vector3 v1 = vertices[v1Index];
				Vector3 v2 = vertices[v2Index];
				Vector3 v3 = vertices[v3Index];

				Edge edge1 = new Edge
				{
					V1 = v1Index,
					V2 = v2Index
				};
				Vector3 v1ToV2 = edgePoints[edge1];

				Edge edge2 = new Edge
				{
					V1 = v2Index,
					V2 = v3Index
				};
				Vector3 v2ToV3 = edgePoints[edge2];

				Edge edge3 = new Edge
				{
					V1 = v3Index,
					V2 = v1Index
				};
				Vector3 v3ToV1 = edgePoints[edge3];
				
				generator.AddTriangle(v1, v1ToV2, v3ToV1);
				generator.AddTriangle(v2, v2ToV3, v1ToV2);
				generator.AddTriangle(v3, v3ToV1, v2ToV3);
				generator.AddTriangle(v1ToV2, v2ToV3, v3ToV1);
			}

			return generator.GetResult();
		}
	}
}
