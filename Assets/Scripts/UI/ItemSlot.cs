using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MyCraft.UI
{
    public class ItemSlot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static readonly int MAX_AMOUNT = 64;

        public static ItemSlot? CurDragSlot;
        public static Vector3? CurBeginDragPos;

        [SerializeField] protected Text amountText;
        [SerializeField] protected Image iconImage;

        public BlockData? block;
        public int amount { get; private set; }
        public bool isEmpty => amount == 0;

        public void Set(BlockData? block, int amount)
        {
            if (amount > MAX_AMOUNT)
                throw new NotSupportedException();

            this.block = block;
            this.amount = amount;

            refresh();
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
                refresh();
                return (new Block(_block.id), amt);
            }
        }

        protected virtual void refresh()
        {
            if (block is BlockData _block)
            {
                iconImage.sprite = _block.iconSprite;
                iconImage.color = new Color(1, 1, 1, 1);
                amountText.text = amount.ToString();
            }
            else
            {
                clear();
            }
        }

        protected virtual void clear()
        {
            amount = 0;
            block = null;

            iconImage.sprite = null;
            iconImage.color = new Color(0, 0, 0, 0);
            amountText.text = "";
        }

        public void OnEnable()
        {
            refresh();
            iconImage.enabled = true;
            amountText.enabled = true;
        }
        public void OnDisable()
        {
            iconImage.enabled = false;
            amountText.enabled = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            void Combine()
            {
                var diff = MAX_AMOUNT - amount;
                var append = CurDragSlot.Take(diff).Item2;
                amount += append;
                refresh();
            }
            void Move()
            {
                this.Set(CurDragSlot.block.Value, CurDragSlot.amount);
                CurDragSlot.Set(null, 0);
            }
            void Swap()
            {
                var tempBlock = this.block;
                var tempAmount = this.amount;
                this.Set(CurDragSlot.block.Value, CurDragSlot.amount);
                CurDragSlot.Set(tempBlock.Value, tempAmount);
            }

            if (block?.id == CurDragSlot?.block?.id && block.HasValue)
                Combine();
            else if (this.isEmpty)
                Move();
            else
                Swap();
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CurDragSlot = this;
            CurBeginDragPos = transform.position;
            GetComponent<CanvasGroup>().blocksRaycasts = false; // blocking ondrop self
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.position = CurBeginDragPos.Value;

            CurDragSlot = null;
            CurBeginDragPos = null;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }
}