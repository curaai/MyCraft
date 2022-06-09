using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class ChunkExistsManagerComponent : MonoBehaviour
    {
        [SerializeField]
        public int ViewDistanceInChunk;

        [SerializeField]
        public Transform player;

        protected ChunkCoord playerCoord => World.ToChunkCoord(player.position).Item1;
        protected ChunkCoord lastPlayerCoord;
        protected List<Chunk> activatedList = new List<Chunk>();

        protected Queue<Chunk> toInitQueue = new Queue<Chunk>();

        private World world;

        public void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            void InitOnFirst()
            {
                CreateAdjacentChunk();
                while (0 < toInitQueue.Count)
                    toInitQueue.Dequeue().Init();
            }

            InitOnFirst();
        }

        public void Update()
        {
            if (playerCoord != lastPlayerCoord)
                CreateAdjacentChunk();
            if (0 < toInitQueue.Count)
                StartCoroutine(InitChunks());
            lastPlayerCoord = playerCoord;
        }

        public void CreateAdjacentChunk()
        {
            var toDeactivate = new List<Chunk>();
            toDeactivate.AddRange(activatedList);

            activatedList.Clear();

            var targets = adjacentChunks();
            foreach (var chunk in targets)
            {
                if (!chunk.Activated)
                    chunk.Activated = true;
                if (!chunk.Initialized)
                    toInitQueue.Enqueue(chunk);

                activatedList.Add(chunk);
                toDeactivate.Remove(chunk);
            }

            foreach (var chunk in toDeactivate)
                chunk.Activated = false;
        }

        IEnumerator InitChunks()
        {
            while (0 < toInitQueue.Count)
            {
                var chunk = toInitQueue.Dequeue();
                chunk.Init();
                yield return null;
            }
        }

        private List<Chunk> adjacentChunks()
        {
            var dist = ViewDistanceInChunk;
            (int x, int z) viewMin = (playerCoord.x - dist, playerCoord.z - dist);
            (int x, int z) viewMax = (playerCoord.x + dist, playerCoord.z + dist);
            var res = new List<Chunk>();

            for (int x = viewMin.x; x < viewMax.x; x++)
            {
                for (int z = viewMin.z; z < viewMax.z; z++)
                {
                    ref Chunk chunk = ref world.chunks[x, z];
                    if (chunk == null)
                        chunk = new Chunk(new ChunkCoord(x, z), world);
                    res.Add(chunk);
                }
            }
            return res;
        }
    }
}