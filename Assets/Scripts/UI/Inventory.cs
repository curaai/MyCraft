using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace MyCraft.UI
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private Hotbar hotbar;
        public ItemSlot[] StorageSlots => transform.GetChild(0).GetComponentsInChildren<ItemSlot>();
        public HotbarItemSlot[] InventorySlots => transform.GetChild(1).GetComponentsInChildren<HotbarItemSlot>();

        void Start()
        {
            var world = GameObject.Find("World").GetComponent<World>();
            // for test
            StorageSlots[0].Set(world.BlockTable[1], 32);
            StorageSlots[1].Set(world.BlockTable[1], 32);
            StorageSlots[2].Set(world.BlockTable[3], 8);
            StorageSlots[3].Set(world.BlockTable[1], 16);
        }

        void OnEnable()
        {
            GetComponent<Image>().enabled = true;
            foreach (var slot in StorageSlots)
                slot.enabled = true;
            foreach (var slot in InventorySlots)
                slot.EnableOriginal();
        }
        void OnDisable()
        {
            GetComponent<Image>().enabled = false;
            foreach (var slot in StorageSlots)
                slot.enabled = false;
            foreach (var slot in InventorySlots)
                slot.DisableOriginal();
        }
    }
}