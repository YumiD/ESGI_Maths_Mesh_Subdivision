using System.Collections.Generic;
using UnityEngine;

namespace Butterfly
{
	public class ButterflySubdiviser : ISubdiviser
	{
		private static void RegisterTriangle(Dictionary<Edge, List<int>> trianglePairs, Vector3 v1, Vector3 v2, int index)
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

		private static Vector3 GetVertexOppositeToEdge(Vector3 edgeV1, Vector3 edgeV2, Vector3 v1, Vector3 v2, Vector3 v3)
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

			return Vector3.zero;
		}

		private static Vector3 Accumulate2(Dictionary<Edge, List<int>> trianglePairs, Edge edge, int triangle, Vector3[] vertices, int[] triangles)
		{
			Vector3 accumulator = Vector3.zero;
			
			List<int> pair = trianglePairs[edge];
			int outerTriangle = pair[0] == triangle ? pair[1] : pair[0];
			
			Vector3 outer1 = GetVertexOppositeToEdge(
				edge.V1,
				edge.V2,
				vertices[triangles[outerTriangle*3+0]],
				vertices[triangles[outerTriangle*3+1]],
				vertices[triangles[outerTriangle*3+2]]
			);
			accumulator -= outer1 * (1.0f / 16.0f);

			return accumulator;
		}

		private static Vector3 Accumulate1(Dictionary<Edge, List<int>> trianglePairs, Edge edge, int triangle, Vector3[] vertices, int[] triangles)
		{
			Vector3 accumulator = Vector3.zero;
			
			Vector3 opposite = GetVertexOppositeToEdge(
				edge.V1,
				edge.V2,
				vertices[triangles[triangle*3+0]],
				vertices[triangles[triangle*3+1]],
				vertices[triangles[triangle*3+2]]
			);
			accumulator += opposite * (1.0f / 8.0f);
			
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
					vertices[triangles[i*3+0]],
					vertices[triangles[i*3+1]],
					i);
				RegisterTriangle(
					trianglePairs,
					vertices[triangles[i*3+1]],
					vertices[triangles[i*3+2]],
					i);
				RegisterTriangle(
					trianglePairs,
					vertices[triangles[i*3+2]],
					vertices[triangles[i*3+0]],
					i);
			}

			Dictionary<Edge, Vector3> edgePoints = new Dictionary<Edge, Vector3>();
			foreach (KeyValuePair<Edge, List<int>> pair in trianglePairs)
			{
				Vector3 accumulator = Vector3.zero;

				Edge edge = pair.Key;

				accumulator += edge.V1 * (1.0f / 2.0f);
				accumulator += edge.V2 * (1.0f / 2.0f);

				accumulator += Accumulate1(trianglePairs, edge, pair.Value[0], vertices, triangles);
				accumulator += Accumulate1(trianglePairs, edge, pair.Value[1], vertices, triangles);

				edgePoints.Add(edge, accumulator);
			}

			MeshGenerator generator = new MeshGenerator();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				Vector3 v1 = vertices[triangles[i*3+0]];
				Vector3 v2 = vertices[triangles[i*3+1]];
				Vector3 v3 = vertices[triangles[i*3+2]];

				Edge edge1 = new Edge
				{
					V1 = v1,
					V2 = v2
				};
				Vector3 v1ToV2 = edgePoints[edge1];

				Edge edge2 = new Edge
				{
					V1 = v2,
					V2 = v3
				};
				Vector3 v2ToV3 = edgePoints[edge2];

				Edge edge3 = new Edge
				{
					V1 = v3,
					V2 = v1
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
