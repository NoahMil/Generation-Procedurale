using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenerationMesh
{
    public class ProceduralQuad : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            GenerateMesh();
        }

        [ContextMenu("Generate Mesh")]
        private void GenerateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "Procedural Quad";
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector4> tangents = new List<Vector4>();
            
            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(0, 1, 0));
            vertices.Add(new Vector3(1, 0, 0));
            vertices.Add(new Vector3(1, 1, 0));
            
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            
            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(3);

            
            normals.Add(Vector3.back);
            normals.Add(Vector3.back);
            normals.Add(Vector3.back);
            normals.Add(Vector3.back);

            
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));

            
            tangents.Add(new Vector4(1, 0, 0, -1));
            tangents.Add(new Vector4(1, 0, 0, -1));
            tangents.Add(new Vector4(1, 0, 0, -1));
            tangents.Add(new Vector4(1, 0, 0, -1));
            
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.tangents = tangents.ToArray();
            
            _meshFilter.mesh = mesh;
        }
    }
}
