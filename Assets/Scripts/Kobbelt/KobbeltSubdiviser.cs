using System.Collections.Generic;
using UnityEngine;

namespace Kobbelt
{
	public class KobbeltSubdiviser : ISubdiviser
	{
		public (Vector3[], int[]) Compute(Vector3[] vertices, int[] triangles)
		{
			Vector3[] centers = new Vector3[triangles.Length];
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				centers[i] = (vertices[triangles[i * 3 + 0]] + vertices[triangles[i * 3 + 1]] + vertices[triangles[i * 3 + 2]]) / 3;
			}

			static void AddVertex(Dictionary<int, List<int>> adjacentIndexDict, int vertexIndex, int other1Index, int other2Index)
			{
				if (!adjacentIndexDict.TryGetValue(vertexIndex, out List<int> adjacentIndices))
				{
					adjacentIndices = new List<int>();
					adjacentIndexDict.Add(vertexIndex, adjacentIndices);
				}

				adjacentIndices.Add(other1Index);
				adjacentIndices.Add(other2Index);
			}

			Dictionary<int, List<int>> adjacentIndexDict = new Dictionary<int, List<int>>();
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
				List<int> adjacentIndices = adjacentIndexDict[index];
				int adjacentIndicesCount = adjacentIndices.Count;
				float alpha = (1.0f / 9.0f) * (4 - 2 * Mathf.Cos((2 * Mathf.PI) / adjacentIndicesCount));

				Vector3 sum = Vector3.zero;
				for (int j = 0; j < adjacentIndicesCount; j++)
				{
					sum += vertices[adjacentIndices[j]];
				}

				perturbedVertices[index] = (1 - alpha) * vertices[index] + (alpha / adjacentIndicesCount) * sum;
			}

			vertices = perturbedVertices;

			static void AddTriangle(Dictionary<Edge, List<int>> trianglePairs, MeshGenerator generator, Vector3 v1, Vector3 v2, Vector3 v3, int index)
			{
				generator.AddTriangle(v1, v2, v3);

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

				AddTriangle(trianglePairs,
					generator,
					vertices[triangles[i * 3 + 0]],
					vertices[triangles[i * 3 + 1]],
					center,
					i * 3 + 0);

				AddTriangle(trianglePairs,
					generator,
					vertices[triangles[i * 3 + 1]],
					vertices[triangles[i * 3 + 2]],
					center,
					i * 3 + 1);

				AddTriangle(trianglePairs,
					generator,
					vertices[triangles[i * 3 + 2]],
					vertices[triangles[i * 3 + 0]],
					center,
					i * 3 + 2);
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
