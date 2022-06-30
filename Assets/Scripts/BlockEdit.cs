using System;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft
{
    public class BlockEdit
    {
        public Vector3Int pos;
        public Block block;

        public bool worldCoord;

        public BlockEdit(Vector3Int _pos, Block _block, bool _worldCoord = true)
        {
            pos = _pos;
            block = _block;
            worldCoord = _worldCoord;
        }

        public BlockEdit ConvertInChunkCoord()
        {
            if (worldCoord)
                return new BlockEdit(CoordHelper.ToChunkCoord(pos).Item2, block, false);
            else
                return this;
        }
    }
}
