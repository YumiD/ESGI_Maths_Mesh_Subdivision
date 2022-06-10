using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectFaces : MonoBehaviour
{
    private float amount = 0.5f;
    private Vector3[] surfaceNormals;
    private Vector3[] newVertices ;

    void Start()
    {
        var myMesh = GetComponent<MeshFilter>().mesh;
        var myVertices = myMesh.vertices;
        var myTriangles = myMesh.triangles;
        var myUV = myMesh.uv;
        var myNormals = myMesh.normals;
        // Set up new arrays to use with rebuilt mesh
        newVertices = new Vector3[myTriangles.Length];
        var newUV = new Vector2[myTriangles.Length];
        var newNormals = new Vector3[myTriangles.Length];
        var newTriangles = new int[myTriangles.Length];

        // Rebuild mesh so that every triangle has unique vertices
        for (var i = 0; i < myTriangles.Length; i++) {
            newVertices[i] = myVertices[myTriangles[i]];
            newUV[i] = myUV[myTriangles[i]];
            newNormals[i] = myNormals[myTriangles[i]];
            newTriangles[i] = i;
        }

        // Assign new mesh
        myMesh.vertices = newVertices;
        myMesh.uv = newUV;
        myMesh.normals = newNormals;
        myMesh.triangles = newTriangles;

        // Get array of surface normals for each triangle
        surfaceNormals = new Vector3[myTriangles.Length/3];
        var idx = 0;
        for (var i = 0; i < surfaceNormals.Length; i++) {
            var v0 = newVertices[idx++];
            surfaceNormals[i] = Vector3.Cross(newVertices[idx++]-v0, newVertices[idx++]-v0).normalized;
        }

        ExplodeMesh(amount);
    }
 
    void ExplodeMesh (float amount) {
        var idx = 0;
        var myMesh = GetComponent<MeshFilter>().mesh;
        var thisVertices = myMesh.vertices;
        for (var i = 0; i < surfaceNormals.Length; i++) {
            thisVertices[idx] = newVertices[idx++] + surfaceNormals[i] * amount;
            thisVertices[idx] = newVertices[idx++] + surfaceNormals[i] * amount;
            thisVertices[idx] = newVertices[idx++] + surfaceNormals[i] * amount;
        }
        myMesh.vertices = thisVertices;
    }
}
