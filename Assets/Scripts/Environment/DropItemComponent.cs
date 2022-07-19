using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using MyCraft.Rendering;
using MyCraft.Utils;

namespace MyCraft.Environment
{
    public class DropItemComponent : MonoBehaviour
    {
        public static readonly float DespawnTime = 50;

        public byte id;
        public int amount;

        public DateTime SpawnTime;
        public BlockTable table => GameObject.Find("World").GetComponent<World>().BlockTable;

        public void Init(byte id, int amount)
        {
            SpawnTime = DateTime.Now;

            this.id = id;
            this.amount = amount;

            // rendering 
            var mesh = new Mesh();
            mesh.subMeshCount = 2;

            var toRender = table[id].textureModel.renderElements;
            mesh.vertices = toRender.Item1;
            mesh.SetTriangles(toRender.Item2, table[id].isTransparent ? 1 : 0);
            mesh.uv = toRender.Item3;

            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        void Update()
        {
            var diff = DateTime.Now - SpawnTime;

            if (DespawnTime < diff.TotalSeconds)
                Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var inventory = other.GetComponent<Player>().inventory;
                if (inventory.AddItem(id, amount))
                    Destroy(gameObject);
            }
        }
    }
}
