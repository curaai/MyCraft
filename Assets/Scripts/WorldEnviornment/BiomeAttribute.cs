using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft.WorldEnvironment
{
    [CreateAssetMenu(fileName = "BiomeAttribute", menuName = "MyCraft/Biome Attribute")]
    public class BiomeAttribute : ScriptableObject
    {
        public static readonly int BASE_GROUND_HEIGHT = 42;

        public int id;
        public float offset;
        public float scale;

        public int terrainHeight;
        public float terrainScale;

        public int surfaceBlockId;
        public int subSurfaceBlockId;
        public int subSurfaceHeight;

        public List<LodeAttribute> lodes;
        public List<TerrianPlantAttribute> plants;

        public Block GenerateBlock(Vector3Int pos, int noiseHeight)
        {
            bool isTreePlacable()
            {
                if (plants.Count == 0) return false;
                else
                {
                    var treeAttr = plants[0];
                    return NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), 0, treeAttr.noiseZoneScale) > treeAttr.noiseZoneThreshold
                        && NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), 0, treeAttr.noisePlacementScale) > treeAttr.noisePlacementThreshold;
                }
            }
            void PlaceTree()
            {
                var stem = new Block(17, true);
                var leave = new Block(18, true);
                // TODO: Replace to queue system
                var world = GameObject.Find("World").GetComponent<World>();
                var attr = plants[0];
                int height = Mathf.FloorToInt(attr.maxHeight * NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), 250f, 3f));
                height = Math.Max(height, attr.minHeight);

                foreach (var i in Enumerable.Range(1, height - 1))
                    world.BlockModifyQueue.Enqueue(new BlockMod(new Vector3Int(pos.x, pos.y + i, pos.z), stem));

                var leaveCoords = (
                    from x in Enumerable.Range(-3, 7)
                    from y in Enumerable.Range(0, 7)
                    from z in Enumerable.Range(-3, 7)
                    select pos + new Vector3Int(x, height + y, z)
                );
                foreach (var v in leaveCoords)
                    world.BlockModifyQueue.Enqueue(new BlockMod(v, leave));
            }

            Block res = new Block() { isSolid = true };
            if (pos.y == noiseHeight)
            {
                res.id = surfaceBlockId;
                if (isTreePlacable()) PlaceTree();
            }
            else if (noiseHeight - subSurfaceHeight <= pos.y && pos.y < noiseHeight)
            {
                res.id = subSurfaceBlockId;
            }
            else if (noiseHeight < pos.y)
            {
                return new Blocks.Air();
            }
            else
            {
                Func<LodeAttribute, bool> inRange = lode => lode.minHeight <= pos.y && pos.y <= lode.maxHeight;
                Func<LodeAttribute, bool> blockExistInNoise = lode => NoiseHelper.Get3DPerlin(pos, lode.noiseOffset, lode.noiseScale, lode.noiseThreshold);

                res.id = (from lode in lodes
                          where inRange(lode)
                          where blockExistInNoise(lode)
                          select lode.blockId).Last();
            }
            return res;
        }
    }

    [Serializable]
    public class LodeAttribute
    {
        public int blockId;
        public int minHeight;
        public int maxHeight;
        public float noiseScale;
        public float noiseThreshold;
        public int noiseOffset;
    }

    // TODO: Now only support tree, it need refactor for use flower or weeds
    [Serializable]
    public class TerrianPlantAttribute
    {
        public float noiseZoneScale;
        [Range(0.1f, 1f)]
        public float noiseZoneThreshold;
        public float noisePlacementScale;
        [Range(0.1f, 1f)]
        public float noisePlacementThreshold;

        public int minHeight;
        public int maxHeight;
    }
}
