using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCraft.Rendering;

namespace MyCraft
{
    public class Chunk
    {
        public Block[,,] BlockMap = new Block[ChunkShape.x, ChunkShape.y, ChunkShape.x];
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
                foreach (var pos in BlockFullIterator())
                    BlockMap[pos.x, pos.y, pos.z] = world.GenerateBlock(chunkPos + pos);
            }

            if (Initialized)
                return;
            GenerateBlocks();
            renderer.RefreshMesh();
            Initialized = true;
        }

        public void EditBlock(List<BlockMod> mods)
        {
            void UpdateSurroundedChunks(Vector3Int coord)
            {
                for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
                {
                    var faceCoord = coord + VoxelData.SurfaceNormal[faceIdx];
                    if (!IsVoxelInChunk(faceCoord))
                        world.GetChunk(chunkPos + faceCoord)?.renderer.RefreshMesh();
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
            if (IsVoxelInChunk(vp)) return GetBlock(vp).isSolid;
            return world.IsSolidBlock(chunkPos + vp);
        }

        public Block GetBlock(in Vector3Int v) => BlockMap[v.x, v.y, v.z];

        public static IEnumerable<Vector3Int> BlockFullIterator()
        {
            for (int x = 0; x < ChunkShape.x; x++)
                for (int y = 0; y < ChunkShape.y; y++)
                    for (int z = 0; z < ChunkShape.x; z++)
                        yield return new Vector3Int(x, y, z);
        }
        protected bool IsVoxelInChunk(in Vector3Int v)
        {
            return (0 <= v.x && v.x < ChunkShape.x &&
                    0 <= v.y && v.y < ChunkShape.y &&
                    0 <= v.z && v.z < ChunkShape.x);
        }
    }
}
