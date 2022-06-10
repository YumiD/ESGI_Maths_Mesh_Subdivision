using Butterfly;
using Kobbelt;
using Loop;
using CatmullClark;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class SubdiviserScript : MonoBehaviour
{
	[SerializeField]
	private Mesh _originalMesh;

	private Mesh _smoothedOriginalMesh;
	private MeshFilter _meshFilter;

	private readonly CatmullClarkSubdiviser _catmullClarkSubdiviser = new CatmullClarkSubdiviser();
	private readonly KobbeltSubdiviser _kobbeltSubdiviser = new KobbeltSubdiviser();
	private readonly LoopAlgorithm _loopSubdiviser = new LoopAlgorithm();
	private readonly ButterflySubdiviser _butterflySubdiviser = new ButterflySubdiviser();

	private void Start()
	{
		_meshFilter = GetComponent<MeshFilter>();

		(Vector3[] vertices, int[] triangles) = SmoothMesh(_originalMesh.vertices, _originalMesh.triangles);

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


	public void ApplyCatmullClark()
	{
		ApplySubdiviser(_catmullClarkSubdiviser);
	}
	public void ApplyKobbelt()
	{
		ApplySubdiviser(_kobbeltSubdiviser);
	}
	public void ApplyLoop()
	{
		ApplySubdiviser(_loopSubdiviser);
	}

	public void ApplyButterfly()
	{
		ApplySubdiviser(_butterflySubdiviser);
	}

	public static (Vector3[], int[]) SmoothMesh(Vector3[] vertices, int[] triangles)
	{
		MeshGenerator generator = new MeshGenerator();

		for (int i = 0; i < triangles.Length / 3; i++)
		{
			generator.AddTriangle(
				vertices[triangles[i * 3 + 0]],
				vertices[triangles[i * 3 + 1]],
				vertices[triangles[i * 3 + 2]]
			);
		}
		return generator.GetResult();
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