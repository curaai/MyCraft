using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class AdjacentChunkRenderComponent : MonoBehaviour
    {
        public World world;
        public Transform player;
        public static readonly int ViewDistanceInChunk = 3;

        protected ChunkCoord curPlayerCoord;
        protected ChunkCoord prevPlayerCoord;
        protected List<Chunk> curActivatedChunkList = new List<Chunk>();
        protected List<Chunk> prevActivatedChunkList = new List<Chunk>();

        public void Update()
        {
            curPlayerCoord = world.GetChunkCoord(player.position);

            if (curPlayerCoord != prevPlayerCoord)
            {
                var dist = ViewDistanceInChunk;
                (int x, int z) viewMin = (curPlayerCoord.x - dist, curPlayerCoord.z - dist);
                (int x, int z) viewMax = (curPlayerCoord.x + dist, curPlayerCoord.z + dist);

                prevActivatedChunkList.AddRange(curActivatedChunkList);
                curActivatedChunkList.Clear();

                for (int x = viewMin.x; x < viewMax.x; x++)
                {
                    for (int z = viewMin.z; z < viewMax.z; z++)
                    {
                        ref Chunk chunk = ref world.chunks[x, z];
                        if (chunk == null)
                            chunk = new Chunk(new ChunkCoord(x, z), world);

                        chunk.chunkObject.SetActive(true);
                        curActivatedChunkList.Add(chunk);
                        prevActivatedChunkList.Remove(chunk);
                    }
                }

                foreach (var leftChunk in prevActivatedChunkList)
                    leftChunk.chunkObject.SetActive(false);
            }
            prevPlayerCoord = curPlayerCoord;
        }
    }
}