using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft.Rendering
{
    public class ChunkRenderer
    {
        private World world;
        public Chunk chunk;
        private GameObject chunkObj;

        private BlockTable blockTable;

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public ChunkRenderer(Chunk _chunk, BlockTable _blockTable)
        {
            chunk = _chunk;
            chunkObj = chunk.gameObj;
            meshRenderer = chunkObj.AddComponent<MeshRenderer>();
            meshFilter = chunkObj.AddComponent<MeshFilter>();
            meshCollider = chunkObj.AddComponent<MeshCollider>();
            meshRenderer.material = _blockTable.material;

            blockTable = _blockTable;

            world = GameObject.Find("World").GetComponent<World>();
        }

        public void RefreshMesh()
        {
            if (!chunk.Initialized)
                return;

            clearMesh();

            foreach (var pos in CoordHelper.ChunkIndexIterator())
            {
                var block = chunk[pos];
                if (block.isSolid)
                    appendBlockMesh(pos, block.id);
            }

            lock (world.ChunksToDraw)
            {
                world.ChunksToDraw.Enqueue(this);
            }
        }

        private void appendBlockMesh(in Vector3Int inChunkCoord, int blockId)
        {
            for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
            {
                if (!chunk.IsSolidBlock(inChunkCoord + VoxelData.SurfaceNormal[faceIdx]))
                {
                    int vertIdx = verts.Count;
                    for (int i = 0; i < 4; i++)
                        verts.Add(inChunkCoord + VoxelData.Verts[VoxelData.Tris[faceIdx, i]]);

                    foreach (var i in VoxelData.TriIdxOrder)
                        tris.Add(vertIdx + i);

                    uvs.AddRange(blockTable[blockId].GetUv((VoxelFace)faceIdx));
                }
            }
        }

        public void CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }

        private void clearMesh()
        {
            verts.Clear();
            tris.Clear();
            uvs.Clear();
        }
    }
}
