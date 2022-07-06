using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Environment.TerrianFeature.Plants
{
    [CreateAssetMenu(menuName = "MyCraft/Plant Attribute/Flower")]
    public class Flower : Plant
    {
        public byte FlowerBlockId;

        public override List<BlockEdit> Generate(Vector3Int placePos)
        {
            var pos = placePos + Vector3Int.up;
            return new List<BlockEdit> { new BlockEdit(pos, FlowerBlockId) };
        }
    }
}
