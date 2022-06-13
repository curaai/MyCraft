using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft.UI
{
    public class Toolbar : MonoBehaviour
    {
        [SerializeField]
        public ItemSlot[] ItemSlots;
        [SerializeField]
        public RectTransform highlightTransform;

        World world;
        Player player;

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

                highlightTransform.position = ItemSlots[SlotIndex].Icon.transform.position;
                player.SelectedBlock = new Block(ItemSlots[SlotIndex].ItemID, true);
            }
        }
        private int _slotIndex = 0;


        private void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            player = world.player.GetComponent<Player>();

            int i = 0;
            foreach (var v in world.BlockTable.DataDict.Values)
            {
                if (8 < i)
                    break;

                var texture = world.BlockTable[v.id].GetTexture(VoxelFace.SOUTH);
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                ItemSlots[i].ItemID = v.id;
                ItemSlots[i].Icon.enabled = true;
                ItemSlots[i].Icon.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                i++;
            }
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
                SlotIndex += scroll > 0 ? -1 : 1;
        }
    }
    [Serializable]
    public class ItemSlot
    {
        // * Item class can be replace int type of property ItemID 
        public int ItemID;
        public Image Icon;
    }
}