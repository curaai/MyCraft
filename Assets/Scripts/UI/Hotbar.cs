using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft.UI
{
    public class Hotbar : MonoBehaviour
    {
        public ItemSlot[] ItemSlots => GetComponentsInChildren<ItemSlot>().Reverse().ToArray();

        [SerializeField]
        public RectTransform highlightTransform;

        World world;
        Player player;

        public ItemSlot SelectedSlot => ItemSlots[SlotIndex];

        public int SlotIndex
        {
            get { return _slotIndex; }
            protected set
            {
                if (value > ItemSlots.Length - 1)
                    _slotIndex = ItemSlots.Length - 1;
                else if (value < 0)
                    _slotIndex = 0;
                else
                    _slotIndex = value;

                highlightTransform.position = ItemSlots[SlotIndex].transform.position;
            }
        }
        private int _slotIndex = 0;


        private void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            player = world.player.GetComponent<Player>();

            ItemSlots[0].Set(world.BlockTable[1], ItemSlot.MAX_AMOUNT);
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
                SlotIndex += scroll > 0 ? -1 : 1;
        }
    }
}