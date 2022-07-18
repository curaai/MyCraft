using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Environment;
using MyCraft.Rendering;

namespace MyCraft.Players
{
    public class RenderComponent : MonoBehaviour
    {
        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public void Start()
        {
            var entityTable = GameObject.Find("World").GetComponent<World>().EntityTable;

            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshRenderer.material = entityTable.material;

            var elem = entityTable["steve"].renderElements;
            verts.AddRange(elem.Item1);
            tris.AddRange(elem.Item2);
            uvs.AddRange(elem.Item3);

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }
    }
}
