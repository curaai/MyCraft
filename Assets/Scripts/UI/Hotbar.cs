using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft.UI
{
    public class Hotbar : MonoBehaviour
    {
        [SerializeField] private RectTransform highlightTransform;

        World world;
        Player player;
        Inventory inventory;

        public HotbarItemSlot[] ItemSlots => inventory.InventorySlots;
        public HotbarItemSlot SelectedSlot => ItemSlots[SlotIndex];

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

                var slotContainers = transform.GetChild(0);
                highlightTransform.position = slotContainers.GetChild(SlotIndex).transform.position;
            }
        }
        private int _slotIndex = 0;

        private void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            player = world.player.GetComponent<Player>();
            inventory = GameObject.Find("Inventory").GetComponent<Inventory>();

            ItemSlots[0].Set(world.BlockTable[1], ItemSlot.MAX_AMOUNT);
            ItemSlots[1].Set(world.BlockTable[5], ItemSlot.MAX_AMOUNT);
            ItemSlots[2].Set(world.BlockTable[12], ItemSlot.MAX_AMOUNT);
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
                SlotIndex += scroll > 0 ? -1 : 1;
        }
    }
}
