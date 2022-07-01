using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace MyCraft.Players
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
            if (world.InUI)
                return;

            if (Input.GetMouseButton(0) && player.HighlightPos.HasValue) // left Button
            {
                if (destoryBlockAnimator.IsUpdateNow)
                {
                    if (destoryBlockAnimator.Update(player.HighlightPos.Value))
                        world.EditBlock(new BlockEdit(player.HighlightPos.Value, 0));
                }
                else
                {
                    var blockData = world.BlockTable[player.HighlightBlock.Value];
                    var sec = CalcBreakSecond(blockData, 1);
                    destoryBlockAnimator.Init(sec, player.HighlightPos);
                }
            }
            if (Input.GetMouseButtonUp(0)) // left Button
                destoryBlockAnimator.Reset();

            if (Input.GetMouseButtonDown(1) && player.PlacedPos.HasValue)// Right Button
            {
                if (!player.SelectedSlot.isEmpty)
                    world.EditBlock(new BlockEdit(player.PlacedPos.Value, player.SelectedSlot.Take(1).Item1));
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
