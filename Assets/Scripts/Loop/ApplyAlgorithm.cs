using UnityEngine;

namespace Loop
{
    public class ApplyAlgorithm : MonoBehaviour
    {
        public void Apply()
        {
            MeshFilter filter = GetComponent<MeshFilter>();

            // Require a mesh to weld (require to remove duplicated vertices)
            Mesh unfilteredMesh = filter.mesh;
            Mesh filteredMesh =
                LoopAlgorithm.RemoveDuplicateVertices(unfilteredMesh, float.Epsilon, unfilteredMesh.bounds.size.x);

            Mesh mesh = LoopAlgorithm.Subdivide(
                filteredMesh, // a filtered mesh
                1, // subdivision count
                false // a result mesh is welded or not
            );
            filter.sharedMesh = mesh;
        }
    }
}