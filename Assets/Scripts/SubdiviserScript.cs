using Kobbelt;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class SubdiviserScript : MonoBehaviour
{
	[SerializeField]
	private Mesh _originalMesh;

	private Mesh _smoothedOriginalMesh;
	private MeshFilter _meshFilter;

	private KobbeltSubdiviser _kobbeltSubdiviser = new KobbeltSubdiviser();
	
	private void Start()
	{
		_meshFilter = GetComponent<MeshFilter>();

		MeshGenerator generator = new MeshGenerator();

		int[] triangles = _originalMesh.triangles;
		Vector3[] vertices = _originalMesh.vertices;
		for (int i = 0; i < triangles.Length / 3; i++)
		{
			generator.AddTriangle(
				vertices[triangles[i * 3 + 0]],
				vertices[triangles[i * 3 + 1]],
				vertices[triangles[i * 3 + 2]]
			);
		}

		(vertices, triangles) = generator.GetResult();
		_smoothedOriginalMesh = new Mesh();
		SetTriangles(_smoothedOriginalMesh, vertices, triangles);
		
		ResetMesh();
	}

	private static void SetTriangles(Mesh mesh, Vector3[] vertices, int[] triangles)
	{
		mesh.indexFormat = IndexFormat.UInt32;
		mesh.vertices = vertices;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
	}

	public void ResetMesh()
	{
		_meshFilter.mesh = _smoothedOriginalMesh;
	}

	public void ApplyKobbelt()
	{
		ApplySubdiviser(_kobbeltSubdiviser);
	}

	private void ApplySubdiviser(ISubdiviser subdiviser)
	{
		Mesh mesh = _meshFilter.mesh;
		
		(Vector3[] vertices, int[] triangles) = subdiviser.Compute(mesh.vertices, mesh.triangles);

		Mesh newMesh = new Mesh();
		SetTriangles(newMesh, vertices, triangles);
		_meshFilter.mesh = newMesh;
	}
}