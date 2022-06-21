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
        public ItemSlot[] storageSlots => GetComponentsInChildren<ItemSlot>();

        void Start()
        {
            var world = GameObject.Find("World").GetComponent<World>();
            // for test
            storageSlots[0].Set(world.BlockTable[1], 32);
            storageSlots[1].Set(world.BlockTable[1], 32);
            storageSlots[2].Set(world.BlockTable[3], 8);
            storageSlots[3].Set(world.BlockTable[1], 16);
        }

        void OnEnable()
        {
            GetComponent<Image>().enabled = true;
            foreach (var slot in storageSlots)
                slot.enabled = true;
        }
        void OnDisable()
        {
            GetComponent<Image>().enabled = false;
            foreach (var slot in storageSlots)
                slot.enabled = false;
        }
    }
}