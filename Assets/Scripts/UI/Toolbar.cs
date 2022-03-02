using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class Toolbar : MonoBehaviour
    {
        public ItemSlot[] itemSlots = new ItemSlot[9];

        World world;
        Player player;

        public int slotIndex
        {
            get { return _slotIndex; }
            protected set
            {
                highlightTransform.position = itemSlots[slotIndex].Icon.transform.position;
                var block = new Block();
                block.IsSolid = true;
                player.SelectedBlock = block;

                // TODO: Implement initialize block part
            }
        }
        private int _slotIndex = 0;

        public RectTransform highlightTransform;

        private void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            player = world.player.GetComponent<Player>();
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
            {
                if (scroll > 0)
                    slotIndex--;
                else
                    slotIndex++;

                if (slotIndex > itemSlots.Length - 1)
                    slotIndex = 0;
                if (slotIndex < 0)
                    slotIndex = itemSlots.Length - 1;
            }
        }
    }
}