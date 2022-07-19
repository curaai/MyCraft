using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft
{
    public class ChunkExistsManagerComponent : MonoBehaviour
    {
        [SerializeField]
        public int ViewDistanceInChunk;

        [SerializeField]
        public Transform player;

        protected ChunkCoord playerCoord => CoordHelper.ToChunkCoord(player.position).Item1;
        protected ChunkCoord lastPlayerCoord;
        protected List<Chunk> activatedList = new List<Chunk>();

        protected Queue<Chunk> toInitQueue = new Queue<Chunk>();

        private World world;

        public void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();

            CreateAdjacentChunk();
        }

        public void Update()
        {
            if (playerCoord != lastPlayerCoord)
                CreateAdjacentChunk();

            lastPlayerCoord = playerCoord;
        }

        public void CreateAdjacentChunk()
        {
            var toDeactivate = new List<Chunk>();
            toDeactivate.AddRange(activatedList);

            activatedList.Clear();

            var targets = adjacentChunkCoords();
            foreach (var coord in targets)
            {
                var chunk = world.GetChunk(coord);

                if (!chunk.Initialized)
                    chunk.Initialize(coord);

                if (!chunk.gameObject.activeSelf)
                    chunk.gameObject.SetActive(true);

                activatedList.Add(chunk);
                toDeactivate.Remove(chunk);
            }

            foreach (var chunk in toDeactivate)
                chunk.gameObject.SetActive(false);
        }

        private List<ChunkCoord> adjacentChunkCoords()
        {
            return (from x in Enumerable.Range(playerCoord.x - ViewDistanceInChunk, ViewDistanceInChunk * 2)
                    from z in Enumerable.Range(playerCoord.z - ViewDistanceInChunk, ViewDistanceInChunk * 2)
                    select new ChunkCoord(x, z)).ToList();
        }
    }
}