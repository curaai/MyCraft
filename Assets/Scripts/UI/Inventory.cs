using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft.UI
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private Hotbar hotbar;
        public ItemSlot[] StorageSlots => transform.GetChild(0).GetComponentsInChildren<ItemSlot>();
        public HotbarItemSlot[] HotbarSlots => transform.GetChild(1).GetComponentsInChildren<HotbarItemSlot>();
        private MyCraft.Environment.BlockTable table => GameObject.Find("World").GetComponent<World>().BlockTable;

        void Awake()
        {
            // for test
            StorageSlots[0].Set(table[1], 32);
            StorageSlots[1].Set(table[1], 32);
            StorageSlots[2].Set(table[3], 8);
            StorageSlots[3].Set(table[1], 16);

            OnDisable();
        }

        public bool AddItem(byte id, int amount)
        {
            if (amount < 1)
                return false;
            var existingSlot = StorageSlots.FirstOrDefault(s => s.block?.id == id && s.amount + amount < 65);
            if (existingSlot != null)
                existingSlot.Add(amount);
            else
            {
                var left = StorageSlots.FirstOrDefault(s => s.amount == 0);
                if (left != null)
                    left.Set(table[id], amount);
                else
                    return false;
            }
            return true;
        }

        void OnEnable()
        {
            GetComponent<Image>().enabled = true;
            foreach (var slot in StorageSlots)
                slot.enabled = true;
            foreach (var slot in HotbarSlots)
            {
                slot.enabled = true;
                slot.EnableOriginal();
            }
        }
        void OnDisable()
        {
            GetComponent<Image>().enabled = false;
            foreach (var slot in StorageSlots)
                slot.enabled = false;
            foreach (var slot in HotbarSlots)
                slot.DisableOriginal();
        }
    }
}