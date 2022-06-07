using UnityEngine;

namespace Loop
{
    public class ApplyAlgorithm : MonoBehaviour
    {
        public void Apply()
        {
            MeshFilter filter = GetComponent<MeshFilter>();

            // Require a mesh to weld (require to remove duplicated vertices)
            Mesh welded = LoopAlgorithm.Weld(filter.mesh, float.Epsilon, filter.mesh.bounds.size.x);

            Mesh mesh = LoopAlgorithm.Subdivide(
                welded, // a welded mesh
                2, // subdivision count
                false // a result mesh is welded or not
            );
            filter.sharedMesh = mesh;
        }
    }
}