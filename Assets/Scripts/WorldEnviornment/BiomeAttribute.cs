using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldEnvironment
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

        public List<Lode> lodes;

        public Block GenerateBlock(Vector3Int pos, int height)
        {
            Block res = new Block() { isSolid = true };
            if (pos.y == height)
                res.id = surfaceBlockId;
            else if (height - subSurfaceHeight <= pos.y && pos.y < height)
                res.id = subSurfaceBlockId;
            else if (height < pos.y)
                return new Blocks.Air();
            else
            {
                Func<Lode, bool> inRange = lode => lode.minHeight <= pos.y && pos.y <= lode.maxHeight;
                Func<Lode, bool> blockExistInNoise = lode => NoiseHelper.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold);

                res.id = (from lode in lodes
                          where inRange(lode)
                          where blockExistInNoise(lode)
                          select lode.blockId).Last();
            }
            return res;
        }
    }

    [Serializable]
    public class Lode
    {
        public int blockId;
        public int minHeight;
        public int maxHeight;
        public float scale;
        public float threshold;
        public int noiseOffset;
    }
}
