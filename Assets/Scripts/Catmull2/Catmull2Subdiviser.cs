using System.Collections.Generic;
using UnityEngine;

namespace Catmull2
{
	public class Catmull2Subdiviser : ISubdiviser
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
		
		private static void AddAdjacentVertices(Dictionary<int, HashSet<int>> adjacentIndicesDict, int vertexIndex, int other1Index, int other2Index)
		{
			if (!adjacentIndicesDict.TryGetValue(vertexIndex, out HashSet<int> adjacentIndices))
			{
				adjacentIndices = new HashSet<int>();
				adjacentIndicesDict.Add(vertexIndex, adjacentIndices);
			}

			adjacentIndices.Add(other1Index);
			adjacentIndices.Add(other2Index);
		}
		
		private static void AddAdjacentTriangles(Dictionary<int, List<int>> adjacentTrianglesDict, int vertexIndex, int triangleIndex)
		{
			if (!adjacentTrianglesDict.TryGetValue(vertexIndex, out List<int> adjacentTriangles))
			{
				adjacentTriangles = new List<int>();
				adjacentTrianglesDict.Add(vertexIndex, adjacentTriangles);
			}

			adjacentTriangles.Add(triangleIndex);
		}

		public (Vector3[], int[]) Compute(Vector3[] vertices, int[] triangles)
		{
			Dictionary<Edge, List<int>> trianglePairs = new Dictionary<Edge, List<int>>();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				RegisterTriangle(
					trianglePairs,
					triangles[i * 3 + 0],
					triangles[i * 3 + 1],
					i);
				RegisterTriangle(
					trianglePairs,
					triangles[i * 3 + 1],
					triangles[i * 3 + 2],
					i);
				RegisterTriangle(
					trianglePairs,
					triangles[i * 3 + 2],
					triangles[i * 3 + 0],
					i);
			}
			
			Dictionary<int, HashSet<int>> adjacentIndicesDict = new Dictionary<int, HashSet<int>>();
			Dictionary<int, List<int>> adjacentTrianglesDict = new Dictionary<int, List<int>>();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				int v1 = triangles[i * 3 + 0];
				int v2 = triangles[i * 3 + 1];
				int v3 = triangles[i * 3 + 2];

				AddAdjacentVertices(adjacentIndicesDict, v1, v2, v3);
				AddAdjacentVertices(adjacentIndicesDict, v2, v3, v1);
				AddAdjacentVertices(adjacentIndicesDict, v3, v1, v2);
				
				AddAdjacentTriangles(adjacentTrianglesDict, v1, i);
				AddAdjacentTriangles(adjacentTrianglesDict, v2, i);
				AddAdjacentTriangles(adjacentTrianglesDict, v3, i);
			}
			
			Vector3[] facePoints = new Vector3[triangles.Length / 3];
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				facePoints[i] = (vertices[triangles[i * 3 + 0]] + vertices[triangles[i * 3 + 1]] + vertices[triangles[i * 3 + 2]]) / 3;
			}

			Dictionary<Edge, Vector3> edgePoints = new Dictionary<Edge, Vector3>();
			foreach (KeyValuePair<Edge, List<int>> pair in trianglePairs)
			{
				Vector3 edgePoint = Vector3.zero;
				
				edgePoint += vertices[pair.Key.V1];
				edgePoint += vertices[pair.Key.V2];

				edgePoint += facePoints[pair.Value[0]];
				edgePoint += facePoints[pair.Value[1]];

				edgePoint /= 4;
				
				edgePoints.Add(pair.Key, edgePoint);
			}

			Vector3[] transformedVertices = new Vector3[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 Q = Vector3.zero;
				List<int> adjacentTriangles = adjacentTrianglesDict[i];
				for (int j = 0; j < adjacentTriangles.Count; j++)
				{
					Q += facePoints[adjacentTriangles[j]];
				}
				Q /= adjacentTriangles.Count;
				
				Vector3 R = Vector3.zero;
				HashSet<int> adjacentIndices = adjacentIndicesDict[i];
				foreach (int adjacentIndex in adjacentIndices)
				{
					Edge edge = new Edge
					{
						V1 = i,
						V2 = adjacentIndex
					};
					R += edgePoints[edge];
				}
				R /= adjacentIndices.Count;

				Vector3 v = vertices[i];
				
				Debug.Assert(adjacentTriangles.Count == adjacentIndices.Count);

				float n = adjacentTriangles.Count;
				
				transformedVertices[i] = (1/n)*Q + (2/n)*R + ((n-3)/n)*v;
			}

			MeshGenerator generator = new MeshGenerator();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				Vector3 center = facePoints[i];
				
				Vector3 v1 = transformedVertices[triangles[i*3+0]];
				Vector3 v2 = transformedVertices[triangles[i*3+1]];
				Vector3 v3 = transformedVertices[triangles[i*3+2]];

				Edge v1ToV2 = new Edge
				{
					V1 = triangles[i*3+0],
					V2 = triangles[i*3+1]
				};
				Edge v2ToV3 = new Edge
				{
					V1 = triangles[i*3+1],
					V2 = triangles[i*3+2]
				};
				Edge v3ToV1 = new Edge
				{
					V1 = triangles[i*3+2],
					V2 = triangles[i*3+0]
				};

				Vector3 v1ToV2Point = edgePoints[v1ToV2];
				Vector3 v2ToV3Point = edgePoints[v2ToV3];
				Vector3 v3ToV1Point = edgePoints[v3ToV1];
				
				generator.AddTriangle(v1, v1ToV2Point, center);
				generator.AddTriangle(v1ToV2Point, v2, center);
				generator.AddTriangle(v2, v2ToV3Point, center);
				generator.AddTriangle(v2ToV3Point, v3, center);
				generator.AddTriangle(v3, v3ToV1Point, center);
				generator.AddTriangle(v3ToV1Point, v1, center);
			}

			return generator.GetResult();
		}
	}
}