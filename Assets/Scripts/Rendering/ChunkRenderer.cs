using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private List<int> transparentTris = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public bool ThreadLocked { get; private set; }

        public ChunkRenderer(Chunk _chunk, BlockTable _blockTable)
        {
            chunk = _chunk;
            chunkObj = chunk.gameObj;
            meshRenderer = chunkObj.AddComponent<MeshRenderer>();
            meshFilter = chunkObj.AddComponent<MeshFilter>();
            meshCollider = chunkObj.AddComponent<MeshCollider>();
            meshRenderer.materials = new Material[] { _blockTable.material, _blockTable.transparentMaterial };

            blockTable = _blockTable;

            world = GameObject.Find("World").GetComponent<World>();
        }

        public void RefreshMesh()
        {
            if (!chunk.Initialized)
                return;

            ThreadLocked = true;

            clearMesh();

            foreach (var pos in CoordHelper.ChunkIndexIterator())
            {
                var block = chunk[pos];
                if (world.BlockTable[block].isSolid)
                    appendBlockMesh(pos, block);
            }

            lock (world.ChunksToDraw)
            {
                world.ChunksToDraw.Enqueue(this);
            }

            ThreadLocked = false;
        }

        private void appendBlockMesh(Vector3Int inChunkCoord, int blockId)
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
            ThreadLocked = true;

            Mesh mesh = new Mesh();

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.subMeshCount = 2;
            mesh.SetTriangles(tris.ToArray(), 0);
            mesh.SetTriangles(transparentTris.ToArray(), 1);

            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;

            ThreadLocked = false;
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
