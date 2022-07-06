
using System;
using UnityEngine;

namespace MyCraft.Environment.TerrianFeature.Plants
{
    [CreateAssetMenu(menuName = "MyCraft/Plant Attribute/Flower")]
    public class Flower : Plant
    {
        public byte flowerBlockId;

        public override BlockEdit[] Generate(Vector3Int placePosInWorld)
        {
            throw new NotImplementedException();
        }
    }
}
