using System;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft.UI
{
    public class ItemSlot : MonoBehaviour
    {
        public static readonly int MAX_AMOUNT = 64;
        [SerializeField]
        private Text countText;
        [SerializeField]
        private Image iconImage;

        private BlockData? block;
        private int amount;
        public bool isEmpty => amount == 0;

        public void Set(BlockData block, int amount)
        {
            if (amount > MAX_AMOUNT)
                throw new NotSupportedException();

            this.block = block;
            this.amount = amount;

            update();
        }

        public (Block, int) Take(int amt)
        {
            if (!block.HasValue)
                throw new InvalidOperationException("Cannot take item from empty slot");

            var _block = (BlockData)block;
            if (amt >= amount)
            {
                var _amt = amount;
                clear();
                return (new Block(_block.id), _amt);
            }
            else
            {
                amount -= amt;
                update();
                return (new Block(_block.id), amt);
            }
        }

        private void update()
        {
            if (block is BlockData _block)
            {
                iconImage.sprite = _block.iconSprite;
                iconImage.enabled = true;
                countText.text = amount.ToString();
            }
            else
            {
                clear();
            }
        }

        private void clear()
        {
            amount = 0;
            block = null;
            iconImage.sprite = null;
            iconImage.enabled = false;
            countText.text = "";
        }
    }
}