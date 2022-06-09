using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullClark : MonoBehaviour
{
    Mesh _mesh;
    /*Vector3[] _vertices;
    Vector3[] _normals;
    int[] _triangles;*/
    void Start()
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        if(_mesh == null){
            Debug.Log("Mesh non trouvé");
            return;
        }
        
        

        /*_vertices = _mesh.vertices;
        _normals = _mesh.normals;
        _triangles = _mesh.triangles;
        for(int i = 0; i < _normals.Length; i++)
            Debug.Log(_normals[i]);
        Debug.Log("_________________________");
        for(int i = 0; i < _vertices.Length; i++)
            Debug.Log(_vertices[i]);
        Debug.Log("_________________________");
        for(int i = 0; i < _triangles.Length; i++)
            Debug.Log(_triangles[i]);
        Debug.Log("_________________________");
        Debug.Log("_normals.Length : " + _normals.Length);
        Debug.Log("_vertices.Length : " + _vertices.Length);
        Debug.Log("_triangles.Length : " + _triangles.Length);*/
    }

    void Update()
    {

    }
}