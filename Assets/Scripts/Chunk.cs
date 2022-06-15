using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCraft.Rendering;
using MyCraft.Utils;

namespace MyCraft
{
    public class Chunk
    {
        public Block[,,] BlockMap = new Block[ChunkShape.x, ChunkShape.y, ChunkShape.x];
        public Block this[Vector3Int v] { get => BlockMap[v.x, v.y, v.z]; protected set => BlockMap[v.x, v.y, v.z] = value; }
        public static readonly Vector2Int ChunkShape = new Vector2Int(16, 128);

        protected World world;
        public GameObject gameObj;

        public ChunkCoord coord;
        protected Transform transform => gameObj.transform;
        public Vector3Int chunkPos;
        public bool Activated
        {
            get => gameObj.activeSelf;
            set => gameObj.SetActive(value);
        }

        public ChunkRenderer renderer;

        public bool Initialized { get; private set; }
        public Chunk(ChunkCoord _coord, World _world)
        {
            world = _world;
            coord = _coord;

            gameObj = new GameObject();
            gameObj.name = $"Chunk [{coord.x}, {coord.z}]";
            transform.SetParent(world.transform);
            transform.position = new Vector3(coord.x * ChunkShape.x, 0f, coord.z * ChunkShape.x);
            chunkPos = Vector3Int.CeilToInt(transform.position);

            Initialized = false;
            renderer = new ChunkRenderer(this, world.BlockTable);
        }

        public void Init()
        {
            void GenerateBlocks()
            {
                foreach (var pos in CoordHelper.ChunkIndexIterator())
                    BlockMap[pos.x, pos.y, pos.z] = world.GenerateBlock(chunkPos + pos);
            }

            if (Initialized)
                return;

            Initialized = true;
            GenerateBlocks();
            renderer.RefreshMesh();
        }

        public void EditBlock(List<BlockEdit> mods)
        {
            void UpdateSurroundedChunks(Vector3Int coord)
            {
                for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
                {
                    var faceCoord = coord + VoxelData.SurfaceNormal[faceIdx];
                    // ! this line makes program slow
                    // if (!IsVoxelInChunk(faceCoord))
                    //     world.GetChunk(chunkPos + faceCoord)?.renderer.RefreshMesh();
                }
            }

            foreach (var mod in mods)
            {
                var coord = mod.pos;
                BlockMap[coord.x, coord.y, coord.z] = mod.block;
                UpdateSurroundedChunks(coord);
            }
            renderer.RefreshMesh();
        }

        public bool IsSolidBlock(in Vector3Int vp)
        {
            if (IsVoxelInChunk(vp)) return this[vp].isSolid;
            return world.IsSolidBlock(chunkPos + vp);
        }

        protected bool IsVoxelInChunk(in Vector3Int v)
        {
            return (0 <= v.x && v.x < ChunkShape.x &&
                    0 <= v.y && v.y < ChunkShape.y &&
                    0 <= v.z && v.z < ChunkShape.x);
        }
    }
}
