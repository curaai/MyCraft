using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class AdjacentChunkRenderComponent : MonoBehaviour
    {
        public static readonly int ViewDistanceInChunk = 3;

        [SerializeField]
        public World world;
        [SerializeField]
        public Transform player;

        protected ChunkCoord playerCoord;
        protected ChunkCoord lastPlayerCoord;
        protected List<Chunk> activatedList = new List<Chunk>();
        protected List<Chunk> lastActivatedList = new List<Chunk>();

        protected Queue<Chunk> toInitQueue = new Queue<Chunk>();
        protected bool initializeNow = false;

        public void Start()
        {
            void InitOnFirst()
            {
                playerCoord = World.ToChunkCoord(player.position).Item1;
                CreateAdjacentChunk();
                foreach (var chunk in activatedList)
                    chunk.Init();
            }

            InitOnFirst();
        }

        public void Update()
        {
            playerCoord = World.ToChunkCoord(player.position).Item1;
            if (playerCoord != lastPlayerCoord)
                CreateAdjacentChunk();
            if (toInitQueue.Count != 0)
                StartCoroutine("InitChunks");
        }

        public void CreateAdjacentChunk()
        {
            lastPlayerCoord = playerCoord;
            var dist = ViewDistanceInChunk;
            (int x, int z) viewMin = (playerCoord.x - dist, playerCoord.z - dist);
            (int x, int z) viewMax = (playerCoord.x + dist, playerCoord.z + dist);

            lastActivatedList.AddRange(activatedList);
            activatedList.Clear();

            for (int x = viewMin.x; x < viewMax.x; x++)
            {
                for (int z = viewMin.z; z < viewMax.z; z++)
                {
                    ref Chunk chunk = ref world.chunks[x, z];
                    if (chunk == null)
                    {
                        chunk = new Chunk(new ChunkCoord(x, z), world);
                        toInitQueue.Enqueue(chunk);
                    }

                    chunk.Activated = true;
                    activatedList.Add(chunk);
                    lastActivatedList.Remove(chunk);
                }
            }

            foreach (var chunk in lastActivatedList)
                chunk.Activated = false;
        }

        IEnumerator InitChunks()
        {
            initializeNow = true;
            while (toInitQueue.Count != 0)
            {
                var chunk = toInitQueue.Dequeue();
                chunk.Init();
                yield return null;
            }
            initializeNow = false;
        }
    }
}