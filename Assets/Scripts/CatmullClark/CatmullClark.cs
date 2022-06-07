using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullClark : MonoBehaviour
{
    Mesh _mesh;
    Vector3[] _vertices;
    Vector3[] _normals;
    void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _vertices = _mesh.vertices;
        _normals = _mesh.normals;
    }

    void Update()
    {

    }
}
