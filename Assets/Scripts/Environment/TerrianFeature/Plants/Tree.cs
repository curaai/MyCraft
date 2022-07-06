using System;
using UnityEngine;

namespace MyCraft.Environment.TerrianFeature.Plants
{
    [CreateAssetMenu(menuName = "MyCraft/Plant Attribute/Tree")]
    public class Tree : Plant
    {
        public byte logBlockId;
        public byte leaveBlockId;

        public int logMinHeight;
        public int logMaxHeight;

        public override BlockEdit[] Generate(Vector3Int placePosInWorld)
        {
            throw new NotImplementedException();
        }
    }
}
