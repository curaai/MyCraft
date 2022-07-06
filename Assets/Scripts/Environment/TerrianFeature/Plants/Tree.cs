using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft.Environment.TerrianFeature.Plants
{
    [CreateAssetMenu(menuName = "MyCraft/Plant Attribute/Tree")]
    public class Tree : Plant
    {
        public byte logBlockId;
        public byte leaveBlockId;

        public int logMinHeight;
        public int logMaxHeight;
        public float logHeightNoise;

        public override List<BlockEdit> Generate(Vector3Int placePos)
        {
            var res = new List<BlockEdit>();

            var noise = NoiseHelper.Get2DPerlin(new Vector2(placePos.x, placePos.z), logHeightNoise, logMinHeight);
            int height = Mathf.FloorToInt(logMinHeight + (logMaxHeight - logMinHeight) * noise);
            height = Math.Max(height, logMinHeight);

            var logs = (
                from i in Enumerable.Range(1, height)
                select new BlockEdit(
                    new Vector3Int(placePos.x,
                                   placePos.y + i,
                                   placePos.z),
                    logBlockId)
            );
            res.AddRange(logs);

            var leaves = (
                from x in Enumerable.Range(-3, 7)
                from y in Enumerable.Range(0, 7)
                from z in Enumerable.Range(-3, 7)
                select new BlockEdit(
                    placePos + new Vector3Int(x, height + y, z),
                    leaveBlockId)
            );
            res.AddRange(leaves);

            return res;
        }
    }
}
