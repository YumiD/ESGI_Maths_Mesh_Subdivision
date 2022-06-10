using System.Collections.Generic;
using UnityEngine;

namespace Kobbelt
{
	public class KobbeltSubdiviser : ISubdiviser
	{
		public (Vector3[], int[]) Compute(Vector3[] vertices, int[] triangles)
		{
			Vector3[] centers = new Vector3[triangles.Length / 3];
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				centers[i] = (vertices[triangles[i * 3 + 0]] + vertices[triangles[i * 3 + 1]] + vertices[triangles[i * 3 + 2]]) / 3;
			}

			static void AddVertex(Dictionary<int, HashSet<int>> adjacentIndexDict, int vertexIndex, int other1Index, int other2Index)
			{
				if (!adjacentIndexDict.TryGetValue(vertexIndex, out HashSet<int> adjacentIndices))
				{
					adjacentIndices = new HashSet<int>();
					adjacentIndexDict.Add(vertexIndex, adjacentIndices);
				}

				adjacentIndices.Add(other1Index);
				adjacentIndices.Add(other2Index);
			}

			Dictionary<int, HashSet<int>> adjacentIndexDict = new Dictionary<int, HashSet<int>>();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				int v1 = triangles[i * 3 + 0];
				int v2 = triangles[i * 3 + 1];
				int v3 = triangles[i * 3 + 2];

				AddVertex(adjacentIndexDict, v1, v2, v3);
				AddVertex(adjacentIndexDict, v2, v3, v1);
				AddVertex(adjacentIndexDict, v3, v1, v2);
			}

			Vector3[] perturbedVertices = new Vector3[vertices.Length];
			for (int i = 0; i < triangles.Length; i++)
			{
				int index = triangles[i];
				HashSet<int> adjacentIndices = adjacentIndexDict[index];
				int adjacentIndicesCount = adjacentIndices.Count;
				float alpha = (1.0f / 9.0f) * (4 - 2 * Mathf.Cos((2 * Mathf.PI) / adjacentIndicesCount));

				Vector3 sum = Vector3.zero;
				foreach (int adjacentIndex in adjacentIndices)
				{
					sum += vertices[adjacentIndex];
				}

				perturbedVertices[index] = (1 - alpha) * vertices[index] + (alpha / adjacentIndicesCount) * sum;
			}

			vertices = perturbedVertices;

			static void AddTriangle(Dictionary<Edge, List<int>> trianglePairs, int v1, int v2, int index)
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

			Dictionary<Edge, List<int>> trianglePairs = new Dictionary<Edge, List<int>>();
			MeshGenerator generator = new MeshGenerator();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				Vector3 center = centers[i];

				int v1Index = triangles[i * 3 + 0];
				int v2Index = triangles[i * 3 + 1];
				int v3Index = triangles[i * 3 + 2];

				Vector3 v1 = vertices[v1Index];
				Vector3 v2 = vertices[v2Index];
				Vector3 v3 = vertices[v3Index];

				AddTriangle(trianglePairs,
					v1Index,
					v2Index,
					i * 3 + 0);

				AddTriangle(trianglePairs,
					v2Index,
					v3Index,
					i * 3 + 1);

				AddTriangle(trianglePairs,
					v3Index,
					v1Index,
					i * 3 + 2);
				
				generator.AddTriangle(v1, v2, center);
				generator.AddTriangle(v2, v3, center);
				generator.AddTriangle(v3, v1, center);
			}

			(vertices, triangles) = generator.GetResult();

			int[] flippedTriangles = new int[triangles.Length];
			foreach (List<int> adjacentTriangles in trianglePairs.Values)
			{
				Debug.Assert(adjacentTriangles.Count == 2);

				flippedTriangles[adjacentTriangles[0] * 3 + 0] = triangles[adjacentTriangles[0] * 3 + 0];
				flippedTriangles[adjacentTriangles[0] * 3 + 1] = triangles[adjacentTriangles[1] * 3 + 2];
				flippedTriangles[adjacentTriangles[0] * 3 + 2] = triangles[adjacentTriangles[0] * 3 + 2];

				flippedTriangles[adjacentTriangles[1] * 3 + 0] = triangles[adjacentTriangles[1] * 3 + 0];
				flippedTriangles[adjacentTriangles[1] * 3 + 1] = triangles[adjacentTriangles[0] * 3 + 2];
				flippedTriangles[adjacentTriangles[1] * 3 + 2] = triangles[adjacentTriangles[1] * 3 + 2];
			}

			return (vertices, flippedTriangles);
		}
	}
}
