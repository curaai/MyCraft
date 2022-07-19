using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MyCraft.Rendering;
using MyCraft.Utils;
using MyCraft.Environment;

namespace MyCraft
{
    public class Chunk : MonoBehaviour
    {
        public static readonly Vector2Int ChunkShape = new Vector2Int(16, 128);
        [SerializeField] private GameObject dropItemPrefab;
        public byte[,,] BlockMap = new byte[ChunkShape.x, ChunkShape.y, ChunkShape.x];

        protected World world;

        public static BiomeAttribute[] biomes;

        public ChunkCoord coord { get; private set; }
        public Vector3Int chunkWorldPos { get; private set; }
        public bool Initialized { get; private set; }

        private ChunkRenderer chunkRenderer;

        private Queue<BlockEdit> editsQueue = new Queue<BlockEdit>();

        public void Initialize(ChunkCoord coord)
        {
            world = GetComponentInParent<World>();

            name = $"Chunk [{coord.x}, {coord.z}]";
            transform.SetParent(world.transform);
            transform.position = new Vector3(coord.x * ChunkShape.x, 0f, coord.z * ChunkShape.x);
            chunkWorldPos = Vector3Int.CeilToInt(transform.position);

            void GenerateBlocks()
            {
                foreach (var pos in CoordHelper.ChunkIndexIterator())
                    BlockMap[pos.x, pos.y, pos.z] = GenerateBlock(pos);
            }

            GenerateBlocks();
            Initialized = true;

            chunkRenderer = new ChunkRenderer(this, world.BlockTable);
            chunkRenderer.RefreshMesh();
        }

        public void Update()
        {
            void DropItem(BlockEdit e)
            {
                var dropItemObj = Instantiate(dropItemPrefab, Vector3.zero, Quaternion.identity, transform);
                dropItemObj.transform.localPosition = e.pos + new Vector3(0.3f, 0, 0.3f);
                dropItemObj.GetComponent<DropItemComponent>().Init(this[e.pos], 1);
            }

            if (!Initialized)
                return;

            var needMeshUpdate = editsQueue.Count != 0;

            while (0 < editsQueue.Count)
            {
                var e = editsQueue.Dequeue().ConvertInChunkCoord();
                if (this[e.pos] != 0 && e.block == 0)
                    DropItem(e);

                this[e.pos] = e.block;
            }

            if (needMeshUpdate)
                chunkRenderer.RefreshMesh();
        }

        public byte GenerateBlock(Vector3Int blockChunkPos)
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

        public void EnqueueEdit(BlockEdit edit)
        {
            editsQueue.Enqueue(edit);
        }

        public void EditBlock(BlockEdit edit)
        {
            EnqueueEdit(edit);
            Update();
        }

        public bool IsSolidBlock(in Vector3Int chunkPos)
        {
            if (IsVoxelInChunk(chunkPos))
                return world.BlockTable[this[chunkPos]].isSolid;
            else
                return false;
        }

        protected bool IsVoxelInChunk(in Vector3Int v)
        {
            return (0 <= v.x && v.x < ChunkShape.x &&
                    0 <= v.y && v.y < ChunkShape.y &&
                    0 <= v.z && v.z < ChunkShape.x);
        }

        public byte this[Vector3Int v] { get => BlockMap[v.x, v.y, v.z]; protected set => BlockMap[v.x, v.y, v.z] = value; }
    }
}
