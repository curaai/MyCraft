using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using MyCraft.WorldEnvironment;
using MyCraft.Utils;

namespace MyCraft
{
    public class World : MonoBehaviour
    {
        public static readonly int WidthByChunk = 100;
        public static readonly int SizeByVoxels = WidthByChunk * Chunk.ChunkShape.x;

        [SerializeField]
        public Transform player;
        [SerializeField]
        private GameObject debugScreen;

        public BlockTable BlockTable;

        public Chunk[,] chunks = new Chunk[WidthByChunk, WidthByChunk];
        public BiomeAttribute[] biomes;
        public Queue<BlockEdit> BlockModifyQueue = new Queue<BlockEdit>();

        private void Awake()
        {
            BlockTable = new BlockTable();
            biomes = Resources.LoadAll<BiomeAttribute>("Table/Biomes");

            Cursor.lockState = CursorLockMode.Locked;

            int ctr = WidthByChunk / 2;
            player.position = new Vector3(ctr * Chunk.ChunkShape.x, Chunk.ChunkShape.y - 20, ctr * Chunk.ChunkShape.x);
        }

        private void Update()
        {
            // TODO: Move checker to preferences class
            if (Input.GetKeyDown(KeyCode.F3))
                debugScreen.SetActive(!debugScreen.activeSelf);

            if (0 < BlockModifyQueue.Count)
                StartCoroutine(ApplyBlockModification());
        }

        public IEnumerator ApplyBlockModification()
        {
            var buffer = new Queue<BlockEdit>();
            while (0 < BlockModifyQueue.Count)
            {
                var mod = BlockModifyQueue.Dequeue();
                buffer.Enqueue(mod);

                var chunkCoord = CoordHelper.ToChunkCoord(mod.pos).Item1;
                if (GetChunk(chunkCoord) == null)
                    chunks[chunkCoord.x, chunkCoord.z] = new Chunk(chunkCoord, this);

                if (buffer.Count > 200)
                {
                    EditBlock(buffer.ToList());
                    buffer.Clear();
                    yield return null;
                }
            }
        }

        public Block GenerateBlock(Vector3Int pos)
        {
            (BiomeAttribute, int) strongestBiome()
            {
                float sumOfHeights = 0f;
                float strongestWeight = 0f;
                BiomeAttribute res = biomes[0];

                foreach (var biome in biomes)
                {
                    float weight = Utils.NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), biome.offset, biome.scale);
                    if (weight > strongestWeight)
                    {
                        strongestWeight = weight;
                        res = biome;
                    }
                    float height = biome.terrainHeight * Utils.NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale) * weight;
                    sumOfHeights += height;
                }
                return (res, Mathf.FloorToInt(sumOfHeights / biomes.Length));
            }
            if (IsSolidBlock(pos)) return GetBlock(pos);

            (var biome, var noiseHeight) = strongestBiome();
            return biome.GenerateBlock(pos, noiseHeight + BiomeAttribute.BASE_GROUND_HEIGHT);
        }

        public void EditBlock(BlockEdit mod)
        {
            GetChunk(mod.pos).EditBlock(new List<BlockEdit>() { mod.ConvertInChunkCoord() });
        }

        public void EditBlock(List<BlockEdit> mods)
        {
            var modGroups = mods.GroupBy(x => CoordHelper.ToChunkCoord(x.pos).Item1).ToList();
            foreach (var modGroup in modGroups)
                GetChunk(modGroup.Key).EditBlock(modGroup.Select(x => x.ConvertInChunkCoord()).ToList());
        }

        public Block GetBlock(Vector3 worldPos)
        {
            var pair = CoordHelper.ToChunkCoord(worldPos);
            return GetChunk(pair.Item1)[pair.Item2];
        }

        public static (ChunkCoord, Vector3Int) ToChunkCoord(in Vector3Int worldPos)
        {
            int x = worldPos.x;
            int y = worldPos.y;
            int z = worldPos.z;

            int chunkX = x / Chunk.ChunkShape.x;
            int chunkZ = z / Chunk.ChunkShape.x;

            x -= chunkX * Chunk.ChunkShape.x;
            z -= chunkZ * Chunk.ChunkShape.x;

            var a = new ChunkCoord(chunkX, chunkZ);
            var b = new Vector3Int(x, y, z);
            return (a, b);
        }
        public Chunk GetChunk(Vector3Int worldPos) => GetChunk(CoordHelper.ToChunkCoord(worldPos).Item1);
        public Chunk GetChunk(ChunkCoord coord) => chunks[coord.x, coord.z];
        public bool IsSolidBlock(in Vector3 worldPos)
        {
            var pos = CoordHelper.ToChunkCoord(worldPos);
            if (pos.Item2.y < 0 || Chunk.ChunkShape.y <= pos.Item2.y)
                return false;

            if (GetChunk(pos.Item1) != null && GetBlock(worldPos) != null)
                return GetBlock(worldPos).isSolid;
            return false;
        }
    }
}
