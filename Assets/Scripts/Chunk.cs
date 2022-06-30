using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MyCraft.Rendering;
using MyCraft.Utils;
using MyCraft.WorldEnvironment;

namespace MyCraft
{
    public class Chunk
    {
        public static readonly Vector2Int ChunkShape = new Vector2Int(16, 128);
        public Block[,,] BlockMap = new Block[ChunkShape.x, ChunkShape.y, ChunkShape.x];

        protected World world;
        public GameObject gameObj;

        public static BiomeAttribute[] biomes;

        public ChunkCoord coord { get; private set; }
        public Vector3Int chunkWorldPos { get; private set; }
        public bool Initialized { get; private set; }

        private ChunkRenderer renderer;

        private Queue<BlockEdit> editsQueue = new Queue<BlockEdit>();
        public bool ThreadLocked = false;

        public Chunk(ChunkCoord _coord, World _world)
        {
            world = _world;
            coord = _coord;

            gameObj = new GameObject();
            gameObj.name = $"Chunk [{coord.x}, {coord.z}]";
            gameObj.transform.SetParent(world.transform);
            gameObj.transform.position = new Vector3(coord.x * ChunkShape.x, 0f, coord.z * ChunkShape.x);
            chunkWorldPos = Vector3Int.CeilToInt(gameObj.transform.position);

            Initialized = false;
            renderer = new ChunkRenderer(this, world.BlockTable);
        }

        public void Init()
        {
            void GenerateBlocks()
            {
                foreach (var pos in CoordHelper.ChunkIndexIterator())
                    BlockMap[pos.x, pos.y, pos.z] = GenerateBlock(pos);
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
            if (!Initialized)
                return;

            ThreadLocked = true;

            while (0 < editsQueue.Count)
            {
                var e = editsQueue.Dequeue();
                if (e == null)
                    Debug.Log(editsQueue.Count);

                var a = e.ConvertInChunkCoord();
                this[a.pos] = a.block;
            }

            renderer.RefreshMesh();

            ThreadLocked = false;
        }

        public Block GenerateBlock(Vector3Int blockChunkPos)
        {
            (BiomeAttribute, int) strongestBiome(Vector3Int worldPos)
            {
                float sumOfHeights = 0f;
                float strongestWeight = 0f;
                BiomeAttribute res = biomes[0];

                foreach (var biome in biomes)
                {
                    float weight = Utils.NoiseHelper.Get2DPerlin(new Vector2(worldPos.x, worldPos.z), biome.offset, biome.scale);
                    if (weight > strongestWeight)
                    {
                        strongestWeight = weight;
                        res = biome;
                    }
                    float height = biome.terrainHeight * Utils.NoiseHelper.Get2DPerlin(new Vector2(worldPos.x, worldPos.z), 0, biome.terrainScale) * weight;
                    sumOfHeights += height;
                }
                return (res, Mathf.FloorToInt(sumOfHeights / biomes.Length));
            }

            (var biome, var noiseHeight) = strongestBiome(chunkWorldPos + blockChunkPos);
            (var block, var additionalEdits) = biome.GenerateBlock(chunkWorldPos + blockChunkPos, noiseHeight + BiomeAttribute.BASE_GROUND_HEIGHT);

            foreach (var edit in additionalEdits)
                world.EditBlock(edit);

            return block;
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
                    var targetPos = chunkWorldPos + faceCoord;
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
            if (IsVoxelInChunk(chunkPos) && this[chunkPos] != null)
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
        public bool IsEditable => Initialized && !ThreadLocked;
    }
}
