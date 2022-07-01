using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCraft.Utils;
using MyCraft.Players;
using MyCraft.UI;

namespace MyCraft
{
    public class Player : MonoBehaviour
    {
        public float Width = 0.6f;
        public float Height = 2f;
        public float Reach = 8;

        [HideInInspector]
        public Transform cam;
        public ChunkCoord CurChunkCoord => CoordHelper.ToChunkCoord(transform.position).Item1;

        public byte? HighlightBlock => GetComponent<PlaceBlockComponent>().HighlightBlock;
        public Vector3Int? HighlightPos => GetComponent<PlaceBlockComponent>().HighlightPos;
        public Vector3Int? PlacedPos => GetComponent<PlaceBlockComponent>().PlacedPos;

        public ItemSlot SelectedSlot => uiObj.GetComponentInChildren<Hotbar>().SelectedSlot;

        private GameObject uiObj;

        public void Start()
        {
            cam = transform.GetChild(0);
            uiObj = GameObject.Find("PlayerUI");
        }
    }
}
