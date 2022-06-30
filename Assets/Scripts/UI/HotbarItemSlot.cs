using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft.UI
{
    public class HotbarItemSlot : ItemSlot
    {
        [SerializeField] private Text hotbarAmountText;
        [SerializeField] private Image hotbarIconImage;

        protected override void refresh()
        {
            base.refresh();

            if (block is BlockData _block)
            {
                hotbarIconImage.sprite = _block.iconSprite;
                hotbarIconImage.color = new Color(1, 1, 1, 1);
                hotbarAmountText.text = amount.ToString();
            }
        }

        protected override void clear()
        {
            base.clear();

            hotbarIconImage.sprite = null;
            hotbarIconImage.color = new Color(0, 0, 0, 0);
            hotbarAmountText.text = "";
        }

        public void EnableOriginal()
        {
            base.OnEnable();
        }
        public void DisableOriginal()
        {
            base.OnDisable();
        }
    }
}
