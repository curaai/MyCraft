using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Rendering
{
    public class ChunkRenderer
    {
        Chunk chunk;
        protected GameObject chunkObj;

        private BlockTable blockTable;

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();
        protected MeshRenderer meshRenderer;
        protected MeshFilter meshFilter;
        protected MeshCollider meshCollider;

        public ChunkRenderer(Chunk _chunk, BlockTable _blockTable)
        {
            chunk = _chunk;
            chunkObj = chunk.gameObj;
            meshRenderer = chunkObj.AddComponent<MeshRenderer>();
            meshFilter = chunkObj.AddComponent<MeshFilter>();
            meshCollider = chunkObj.AddComponent<MeshCollider>();
            meshRenderer.material = _blockTable.material;

            blockTable = _blockTable;
        }

        public void RefreshMesh()
        {
            ClearMesh();

            foreach (var pos in Chunk.BlockFullIterator())
            {
                var block = chunk.GetBlock(pos);
                if (block.isSolid)
                    AppendBlockMesh(pos, block.id);
            }

            CreateMesh();
        }

        protected void AppendBlockMesh(in Vector3Int inChunkCoord, int blockId)
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

                    uvs.AddRange(blockTable.GetTextureUv(blockId, (VoxelFace)faceIdx));
                }
            }
        }

        protected void CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }

        protected void ClearMesh()
        {
            verts.Clear();
            tris.Clear();
            uvs.Clear();
        }
    }
}
