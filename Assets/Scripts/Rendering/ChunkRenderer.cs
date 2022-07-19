using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Environment;
using MyCraft.Utils;

namespace MyCraft.Rendering
{
    public class ChunkRenderer
    {
        public Chunk chunk;
        private GameObject chunkObj;

        private BlockTable blockTable;

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();
        private List<int> transparentTris = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public bool ThreadLocked { get; private set; }

        public ChunkRenderer(Chunk _chunk, BlockTable _blockTable)
        {
            chunk = _chunk;
            blockTable = _blockTable;

            meshFilter = chunk.GetComponent<MeshFilter>();
            meshCollider = chunk.GetComponent<MeshCollider>();
        }

        public void RefreshMesh()
        {
            if (!chunk.Initialized)
                return;

            clearMesh();

            foreach (var pos in CoordHelper.ChunkIndexIterator())
            {
                var block = chunk[pos];
                if (blockTable[block].id != 0)
                    appendBlockMesh(pos, block);
            }

            CreateMesh();
        }

        private void appendBlockMesh(Vector3Int inChunkCoord, byte blockId)
        {
            bool isAppendMesh = false;
            for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT && !isAppendMesh; faceIdx++)
                if (!chunk.IsSolidBlock(inChunkCoord + VoxelData.SurfaceNormal[faceIdx]))
                    isAppendMesh = true;

            if (isAppendMesh)
            {
                int vertIdx = verts.Count;

                var toRender = blockTable[blockId].textureModel.renderElements;
                verts.AddRange(toRender.Item1.Select(v => v + inChunkCoord));

                var _tris = toRender.Item2.Select(i => i + vertIdx).ToList();
                if (blockTable[blockId].isTransparent)
                    transparentTris.AddRange(_tris);
                else
                    tris.AddRange(_tris);
                uvs.AddRange(toRender.Item3);
            }
        }

        public void CreateMesh()
        {
            Debug.Log($"{chunk.coord}: Create mesh");

            Mesh mesh = new Mesh();

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.subMeshCount = 4;
            mesh.SetTriangles(tris.ToArray(), 0);
            mesh.SetTriangles(transparentTris.ToArray(), 1);

            mesh.RecalculateNormals();

            meshCollider.sharedMesh = mesh;
            meshFilter.mesh = mesh;
        }

        private void clearMesh()
        {
            verts.Clear();
            tris.Clear();
            transparentTris.Clear();
            uvs.Clear();
        }
    }
}
