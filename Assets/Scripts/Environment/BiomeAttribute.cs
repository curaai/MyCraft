using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;
using MyCraft.Environment;

namespace MyCraft.Environment
{
    [CreateAssetMenu(fileName = "BiomeAttribute", menuName = "MyCraft/Biome Attribute")]
    public class BiomeAttribute : ScriptableObject
    {
        public static readonly int BASE_GROUND_HEIGHT = 42;
        public static readonly byte stem = 17;
        public static readonly byte leave = 18;

        public int id;
        public float offset;
        public float scale;

        public int terrainHeight;
        public float terrainScale;

        public byte surfaceBlockId;
        public byte subSurfaceBlockId;
        public int subSurfaceHeight;

        public List<LodeAttribute> lodes;
        public List<MyCraft.Environment.TerrianFeature.Plants.Plant> plants;

        public (byte, List<BlockEdit>) GenerateBlock(Vector3Int pos, int noiseHeight)
        {
            var additionalBlocks = new List<BlockEdit>();

            byte res;
            if (pos.y == noiseHeight)
            {
                res = surfaceBlockId;
            }
            else if (noiseHeight - subSurfaceHeight <= pos.y && pos.y < noiseHeight)
            {
                res = subSurfaceBlockId;
            }
            else if (noiseHeight < pos.y)
            {
                return (0, additionalBlocks);
            }
            else
            {
                Func<LodeAttribute, bool> inRange = lode => lode.minHeight <= pos.y && pos.y <= lode.maxHeight;
                Func<LodeAttribute, bool> blockExistInNoise = lode => NoiseHelper.Get3DPerlin(pos, lode.noiseOffset, lode.noiseScale, lode.noiseThreshold);

                res = (byte)(from lode in lodes
                             where inRange(lode)
                             where blockExistInNoise(lode)
                             select lode.blockId).Last();
            }
            return (res, additionalBlocks);
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
}
