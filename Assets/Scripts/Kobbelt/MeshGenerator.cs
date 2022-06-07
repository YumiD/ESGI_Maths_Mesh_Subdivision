using System.Collections.Generic;
using UnityEngine;

namespace Kobbelt
{
	public class MeshGenerator
	{
		private Dictionary<Vector3, int> _vertexDict = new Dictionary<Vector3, int>();
		private List<int> _triangles = new List<int>();

		public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			_triangles.Add(GetVertexIndex(v1));
			_triangles.Add(GetVertexIndex(v2));
			_triangles.Add(GetVertexIndex(v3));
		}
		
		private int GetVertexIndex(Vector3 vertex)
		{
			if (!_vertexDict.TryGetValue(vertex, out int index))
			{
				index = _vertexDict.Count;
				_vertexDict.Add(vertex, index);
			}

			return index;
		}

		public (Vector3[], int[]) GetResult()
		{
			Vector3[] vertices = new Vector3[_vertexDict.Count];
			foreach (KeyValuePair<Vector3,int> pair in _vertexDict)
			{
				vertices[pair.Value] = pair.Key;
			}

			return (vertices, _triangles.ToArray());
		}
	}
}