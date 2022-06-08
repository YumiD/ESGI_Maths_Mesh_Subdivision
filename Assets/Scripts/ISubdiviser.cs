using UnityEngine;

public interface ISubdiviser
{
	public (Vector3[], int[]) Compute(Vector3[] vertices, int[] triangles);
}
