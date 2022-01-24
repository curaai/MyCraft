using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class World : MonoBehaviour
{
    public Material material;
    public Chunk chunk;

    private void Start()
    {
        chunk = new Chunk(this);
    }

    public Block GetVoxel(Vector3 pos)
    {
        var block = new Block();

        void SetTerrarian()
        {
            int yPos = Mathf.FloorToInt(pos.y);
            float noise = Noise.Perlin(new Vector2(pos.x, pos.z), 0f, 0.1f);
            var terrianHeight = Mathf.FloorToInt(noise * Chunk.Height);
            if (yPos <= terrianHeight)
                block.IsSolid = true;
            else
                block.IsSolid = false;
        }

        // SetTerrarian();
        block.IsSolid = true;

        return block;
    }
}
