using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
namespace Players
{
    public class MouseBlockComponent : MonoBehaviour
    {
        private DestoryBlockAnimator destoryBlockAnimator;

        protected Player player;
        protected World world;

        private void Start()
        {
            player = GetComponent<Player>();
            world = GameObject.Find("World").GetComponent<World>();
            destoryBlockAnimator = new DestoryBlockAnimator(player.GetComponent<PlaceBlockComponent>().DestroyMaterial);
        }

        public void Update()
        {
            if (Input.GetMouseButton(0) && player.HighlightPos.HasValue) // Right Button
            {
                if (destoryBlockAnimator.IsUpdateNow)
                {
                    if (destoryBlockAnimator.Update(player.HighlightPos.Value))
                        world.EditBlock(player.HighlightPos.Value, new Blocks.Air());
                }
                else
                {
                    var blockData = world.BlockTable.DataDict[player.HighlightBlock.id];
                    var sec = CalcBreakSecond(blockData, 1);
                    destoryBlockAnimator.Init(sec, player.HighlightPos);
                }
            }
            if (Input.GetMouseButtonUp(0)) // Right Button
                destoryBlockAnimator.Reset();

            if (Input.GetMouseButtonDown(1) && player.PlacedPos.HasValue) // Right Button
            {
                world.EditBlock(player.PlacedPos.Value, player.SelectedBlock);
            }
        }

        public static float CalcBreakSecond(BlockData block, float toolSpeed)
        {
            var damage = toolSpeed / block.hardness;
            damage /= 100;
            var tick = 1 / damage;
            var sec = tick / 20;
            return sec;
        }
    }
}