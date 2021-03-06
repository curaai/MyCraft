using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using MyCraft.Environment;
using MyCraft.Utils;

namespace MyCraft
{
    public class World : MonoBehaviour
    {
        public static readonly int WidthByChunk = 100;
        public static readonly int SizeByVoxels = WidthByChunk * Chunk.ChunkShape.x;

        [SerializeField] public Transform player;
        [SerializeField] private GameObject debugScreen;
        [SerializeField] private GameObject chunkPrefab;

        public BlockTable BlockTable;
        public EntityTable EntityTable;

        public Chunk[,] chunks = new Chunk[WidthByChunk, WidthByChunk];
        public Dictionary<ChunkCoord, Chunk> _chunks = new Dictionary<ChunkCoord, Chunk>();

        private BiomeAttribute[] biomes;
        private Queue<BlockEdit> blockEditQueue = new Queue<BlockEdit>();
        private List<Chunk> chunksToUpdate = new List<Chunk>();
        public Queue<Rendering.ChunkRenderer> ChunksToDraw = new Queue<Rendering.ChunkRenderer>();

        public bool InUI
        {
            get { return _InUI; }
            private set
            {
                _InUI = value;
                if (_InUI)
                {
                    Cursor.lockState = CursorLockMode.None;
                    GameObject.Find("Inventory").GetComponent<UI.Inventory>().enabled = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    GameObject.Find("Inventory").GetComponent<UI.Inventory>().enabled = false;
                }
            }
        }
        private bool _InUI = false;

        void Awake()
        {
            Application.targetFrameRate = 120;

            BlockTable = new BlockTable();
            EntityTable = new EntityTable();
            biomes = Resources.LoadAll<BiomeAttribute>("Table/Biomes");
            Chunk.biomes = Resources.LoadAll<BiomeAttribute>("Table/Biomes");

            InUI = false;

            int ctr = WidthByChunk / 2;
            player.position = new Vector3(ctr * Chunk.ChunkShape.x, Chunk.ChunkShape.y - 60, ctr * Chunk.ChunkShape.x);
        }

        void Update()
        {
            fetchPlayerInputs();

            if (0 < blockEditQueue.Count)
                ApplyBlockModification();
        }

        private void fetchPlayerInputs()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                debugScreen.SetActive(!debugScreen.activeSelf);
            if (Input.GetKeyDown(KeyCode.I))
                InUI = !InUI;
        }

        public void ApplyBlockModification()
        {
            while (0 < blockEditQueue.Count)
            {
                var mod = blockEditQueue.Dequeue();

                var chunkCoord = CoordHelper.ToChunkCoord(mod.pos).Item1;
                var chunk = GetChunk(chunkCoord);
                if (chunk == null)
                    // chunk = chunks[chunkCoord.x, chunkCoord.z] = new Chunk(chunkCoord, this);
                    if (!chunksToUpdate.Contains(chunk))
                        chunksToUpdate.Add(chunk);

                GetChunk(chunkCoord).EnqueueEdit(mod);
            }
        }

        public void EditBlock(BlockEdit edit)
        {
            blockEditQueue.Enqueue(edit);
        }

        public byte GetBlock(Vector3 worldPos)
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
        public Chunk GetChunk(ChunkCoord coord)
        {
            if (!_chunks.ContainsKey(coord))
            {
                var chunkObj = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
                _chunks[coord] = chunkObj.GetComponent<Chunk>();
                chunkObj.SetActive(false);
            }

            return _chunks[coord];
        }
        public bool IsSolidBlock(in Vector3 worldPos)
        {
            var pos = CoordHelper.ToChunkCoord(worldPos);
            if (pos.Item2.y < 0 || Chunk.ChunkShape.y <= pos.Item2.y)
                return false;

            if (GetChunk(pos.Item1) != null)
                return BlockTable[GetBlock(worldPos)].isSolid;
            return false;
        }
    }
}
