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

        [SerializeField] public Transform player;
        [SerializeField] private GameObject debugScreen;

        public BlockTable BlockTable;

        public Chunk[,] chunks = new Chunk[WidthByChunk, WidthByChunk];
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
            BlockTable = new BlockTable();
            biomes = Resources.LoadAll<BiomeAttribute>("Table/Biomes");
            Chunk.biomes = Resources.LoadAll<BiomeAttribute>("Table/Biomes");

            InUI = false;

            int ctr = WidthByChunk / 2;
            player.position = new Vector3(ctr * Chunk.ChunkShape.x, Chunk.ChunkShape.y - 20, ctr * Chunk.ChunkShape.x);
        }

        void Update()
        {
            fetchPlayerInputs();

            if (0 < blockEditQueue.Count)
                ApplyBlockModification();

            if (0 < chunksToUpdate.Count)
                UpdateChunks();
            if (0 < ChunksToDraw.Count)
            {
                lock (ChunksToDraw)
                {
                    if (ChunksToDraw.Peek().chunk.IsEditable)
                        ChunksToDraw.Dequeue().CreateMesh();
                }
            }
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
                    chunk = chunks[chunkCoord.x, chunkCoord.z] = new Chunk(chunkCoord, this);
                if (!chunksToUpdate.Contains(chunk))
                    chunksToUpdate.Add(chunk);

                GetChunk(chunkCoord).EditBlock(mod);
            }
        }

        public void UpdateChunks()
        {
            int idx = 0;
            while (idx < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[idx].IsEditable)
                {
                    chunksToUpdate[idx].Update();
                    chunksToUpdate.RemoveAt(idx);
                    return;
                }
                else
                {
                    idx++;
                }
            }
        }

        public void EditBlock(BlockEdit edit)
        {
            blockEditQueue.Enqueue(edit);
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
