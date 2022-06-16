using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using MyCraft.Rendering;
using MyCraft.Utils;

namespace MyCraft
{
    public class Chunk
    {
        public Block[,,] BlockMap = new Block[ChunkShape.x, ChunkShape.y, ChunkShape.x];
        public static readonly Vector2Int ChunkShape = new Vector2Int(16, 128);

        protected World world;
        public GameObject gameObj;

        public ChunkCoord coord { get; private set; }
        public Vector3Int worldPos { get; private set; }
        public bool Initialized { get; private set; }

        private ChunkRenderer renderer;

        private Queue<BlockEdit> editsQueue = new Queue<BlockEdit>();
        private bool threadLocked = false;

        public Chunk(ChunkCoord _coord, World _world)
        {
            world = _world;
            coord = _coord;

            gameObj = new GameObject();
            gameObj.name = $"Chunk [{coord.x}, {coord.z}]";
            gameObj.transform.SetParent(world.transform);
            gameObj.transform.position = new Vector3(coord.x * ChunkShape.x, 0f, coord.z * ChunkShape.x);
            worldPos = Vector3Int.CeilToInt(gameObj.transform.position);

            Initialized = false;
            renderer = new ChunkRenderer(this, world.BlockTable);
        }

        public void Init()
        {
            void GenerateBlocks()
            {
                foreach (var pos in CoordHelper.ChunkIndexIterator())
                    BlockMap[pos.x, pos.y, pos.z] = world.GenerateBlock(worldPos + pos);
            }

            if (Initialized)
                return;

            GenerateBlocks();
            Initialized = true;
            _update();
        }

        public void Update()
        {
            var thread = new Thread(new ThreadStart(_update));
            thread.Start();
        }

        public void _update()
        {
            threadLocked = true;

            while (0 < editsQueue.Count)
            {
                var e = editsQueue.Dequeue().ConvertInChunkCoord();
                this[e.pos] = e.block;
            }

            renderer.RefreshMesh();

            threadLocked = false;
        }

        public void EditBlock(BlockEdit edit)
        {
            editsQueue.Enqueue(edit);

            void UpdateSurroundedChunks(Vector3Int coord)
            {
                for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
                {
                    // except y changes
                    if ((VoxelFace)faceIdx == VoxelFace.UP || (VoxelFace)faceIdx == VoxelFace.DOWN)
                        continue;

                    var faceCoord = coord + VoxelData.SurfaceNormal[faceIdx];
                    var targetPos = worldPos + faceCoord;
                    var chunk = world.GetChunk(CoordHelper.ToChunkCoord(targetPos).Item1);
                    if (chunk != null && chunk.Initialized && !IsVoxelInChunk(faceCoord))
                    {
                        var dummyPos = targetPos + VoxelData.SurfaceNormal[faceIdx]; // avoid infinite call
                        var dummyReqForUpdate = new BlockEdit(dummyPos, chunk[CoordHelper.ToChunkCoord(dummyPos).Item2]);
                        world.EditBlock(dummyReqForUpdate);
                    }
                }
            }
            UpdateSurroundedChunks(edit.ConvertInChunkCoord().pos);
        }

        public bool IsSolidBlock(in Vector3Int chunkPos)
        {
            if (IsVoxelInChunk(chunkPos))
                return this[chunkPos].isSolid;
            else
                return false;
        }

        protected bool IsVoxelInChunk(in Vector3Int v)
        {
            return (0 <= v.x && v.x < ChunkShape.x &&
                    0 <= v.y && v.y < ChunkShape.y &&
                    0 <= v.z && v.z < ChunkShape.x);
        }

        public Block this[Vector3Int v] { get => BlockMap[v.x, v.y, v.z]; protected set => BlockMap[v.x, v.y, v.z] = value; }
        public bool Activated { get => gameObj.activeSelf; set => gameObj.SetActive(value); }
        public bool IsEditable => Initialized && !threadLocked;
    }
}
