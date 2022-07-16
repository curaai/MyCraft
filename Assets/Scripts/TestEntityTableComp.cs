using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyCraft.Environment;
using MyCraft.Rendering;

namespace MyCraft
{
    public class TestEntityTableComp : MonoBehaviour
    {
        public EntityTable table;
        public EntityTable.EntityTextureModel steveModel;

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public void Start()
        {
            table = new EntityTable();
            this.steveModel = table.steve;

            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshRenderer.material = table.material;

            main();

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }

        void main()
        {
            foreach (var bone in steveModel.bones)
            {
                foreach (var cube in bone.cubes)
                {
                    Debug.Log("uv" + cube.uv);
                }
                break;
            }
        }
    }
}
