using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;
using MyCraft.Environment.terrainFeature.Plants;

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
        public List<Plant> plants;

        // TODO: Only Debug, remove after
        byte[] debugOreTargets = new byte[] { 14, 15, 16, 56 };

        public (byte, List<BlockEdit>) GenerateBlock(Vector3Int pos, int noiseHeight)
        {
            var additionalBlocks = new List<BlockEdit>();

            byte res;
            if (pos.y == noiseHeight)
            {
                res = surfaceBlockId;
                foreach (var plant in plants)
                {
                    if (isPlacable(pos, plant))
                    {
                        additionalBlocks.AddRange(plant.Generate(pos));
                        break;
                    }
                }
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
                Func<LodeAttribute, bool> inRange = lode => lode.MinHeight <= pos.y && pos.y <= lode.MaxHeight;
                Func<LodeAttribute, bool> blockExistInNoise = lode => NoiseHelper.Get3DPerlinBound(
                    pos,
                    lode.NoiseOffset,
                    lode.NoiseScale,
                    (lode.NoiseThresholdMinBound, lode.NoiseThresholdMaxBound)
                );

                res = (byte)(from lode in lodes
                             where inRange(lode)
                             where blockExistInNoise(lode)
                             select lode.BlockId).Last();

                if (debugOreTargets.Contains(res))
                    Debug.Log($"Ore generated on: {CoordHelper.ToChunkCoord(pos)}");
            }
            return (res, additionalBlocks);
        }

        private static bool isPlacable(Vector3Int pos, Plant plant)
        {
            return NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), 0, plant.NoiseZoneScale) > plant.NoiseZoneThreshold
                && NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), 0, plant.NoisePlacementScale) > plant.NoisePlacementThreshold;
        }
    }

    [Serializable]
    public class LodeAttribute
    {
        public int BlockId;
        public int MinHeight;
        public int MaxHeight;
        public float NoiseScale;
        public float NoiseThresholdMinBound;
        public float NoiseThresholdMaxBound;
        public int NoiseOffset;
    }
}
