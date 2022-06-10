using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Catmull.Models;

public class CatmullClark : MonoBehaviour
{
    [SerializeField] public GameObject _point;

    Mesh _mesh;

    void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        if(_mesh == null){
            Debug.Log("Mesh non trouvé");
            return;
        }
        _mesh = applyCatmullClark(_mesh);
    }


    Mesh applyCatmullClark(Mesh meshBase) {
        Mesh meshSubdivised = new Mesh();

        Mesh meshClean = RemoveDuplicateVertices(meshBase.vertices, meshBase.triangles, float.Epsilon, meshBase.bounds.size.x);
        Vector3[] vertices = meshClean.vertices;
        int[] triangles = meshClean.triangles;
        List<Triangle> facesList = Triangle.CreateListFromMesh(meshClean);
        List<Edge> edgeList = Edge.CreateListFromMesh(meshClean);

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        
        // Get Faces Center
        foreach (Triangle face in facesList) {
            newVertices.Add(face.GetCenterPosition(vertices));
        }

        // Get Edges Point
        foreach (Edge edge in edgeList) {
            List<Triangle> edgeFaceNeighbors = facesList.FindAll( f => f.Contains(edge));
            newVertices.Add(GetEdgePointPosition(edge, edgeFaceNeighbors, vertices));
        }

        foreach (var face in facesList) {
            Vector3 center = face.GetCenterPosition(vertices);
            int centerIndex = newVertices.FindIndex(verticePos => verticePos == center);

            //CreateNewFaces(centerIndex, newVertices, newTriangles);
            
        }

        meshSubdivised.vertices = newVertices.ToArray();    
        meshSubdivised.triangles = newTriangles.ToArray();
        return meshSubdivised;
    }

    public Vector3 TransformVertice(int vertexIndex, Vector3[] vertices, List<Triangle> facesList, List<Edge> edgeList){

        Vector3 verticeTransformed = new Vector3();

        List<Triangle> facesNeighbors = facesList.FindAll(face => face.Contains(vertexIndex));

        // Q : the average of all new face points of v
        Vector3 Q = new Vector3(0,0,0);
        foreach (Triangle face in facesNeighbors){
            Q += face.GetCenterPosition(vertices);
        }
        Q = Q / facesNeighbors.Count;

        // R : the average of all mid-points of vertex v
        var edgesNeighbors = edgeList.FindAll(edge => edge.AI == vertexIndex || edge.BI == vertexIndex);
        Vector3 R = new Vector3(0,0,0);
        foreach (Edge edge in edgesNeighbors){
            R += edge.GetCenterPosition(vertices);
        }
        R = R / edgesNeighbors.Count;

        // n : # of incident edges of v
        int n = facesNeighbors.Count;
        verticeTransformed = Q/n + (2*R)/n + ((n-3)*vertices[vertexIndex])/n;

        return verticeTransformed;
    }

    public Vector3 GetEdgePointPosition(Edge edge, List<Triangle> edgeNeighbors, Vector3[] vertices){
        return( (vertices[edge.AI] + vertices[edge.BI] + edgeNeighbors[0].GetCenterPosition(vertices) + edgeNeighbors[1].GetCenterPosition(vertices) ) / 4f);
    }

    public static Mesh RemoveDuplicateVertices(Vector3[] oldVertices, int[] oldTriangle, float threshold, float bucketStep)
    {
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2New = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            buckets[x, y, z] ??= new List<int>();

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2New[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2New[i] = newSize;
            newSize++;

            skip: ;
        }

        // Make new triangles
        int[] newTriangle = new int[oldTriangle.Length];
        for (int i = 0; i < oldTriangle.Length; i++)
        {
            newTriangle[i] = old2New[oldTriangle[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
        {
            finalVertices[i] = newVertices[i];
        }

        Mesh newMesh = new Mesh
        {
            vertices = finalVertices,
            triangles = newTriangle
        };

        return newMesh;
    }

}