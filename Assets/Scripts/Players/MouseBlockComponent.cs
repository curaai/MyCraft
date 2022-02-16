using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class MouseBlockComponent : MonoBehaviour
    {
        protected Player player;
        protected World world;
        public Block selectedBlock;

        private void Start()
        {
            player = GetComponent<Player>();
            world = GameObject.Find("World").GetComponent<World>();

            selectedBlock = new Block();
            selectedBlock.type = BlockType.Grass;
            selectedBlock.IsSolid = true;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) && player.HighlightPos.HasValue) // Right Button
            {
                var air = new Blocks.Air();
                world.EditBlock(player.HighlightPos.Value, air);
            }

            if (Input.GetMouseButtonDown(1) && player.PlacedPos.HasValue) // Right Button
            {
                world.EditBlock(player.PlacedPos.Value, selectedBlock);
            }
        }
    }
}