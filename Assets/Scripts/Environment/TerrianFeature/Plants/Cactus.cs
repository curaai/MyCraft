using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft.Environment.TerrianFeature.Plants
{
    [CreateAssetMenu(menuName = "MyCraft/Plant Attribute/Cactus")]
    public class Cactus : Plant
    {
        public byte BlockId;

        public int MinHeight;
        public int MaxHeight;
        public float HeightNoise;

        public override List<BlockEdit> Generate(Vector3Int placePos)
        {
            var res = new List<BlockEdit>();

            var noise = NoiseHelper.Get2DPerlin(new Vector2(placePos.x, placePos.z), HeightNoise, MinHeight);
            int height = Mathf.FloorToInt(MinHeight + (MaxHeight - MinHeight) * noise);
            height = Math.Max(height, MinHeight);

            var cactus = (
                from i in Enumerable.Range(1, height)
                select new BlockEdit(
                    new Vector3Int(placePos.x,
                                   placePos.y + i,
                                   placePos.z),
                    BlockId)
            );
            res.AddRange(cactus);

            return res;
        }
    }
}
