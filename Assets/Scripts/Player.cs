using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft
{
    public class Player : MonoBehaviour
    {
        public float Width = 0.6f;
        public float Height = 2f;
        public float Reach = 8;

        [HideInInspector]
        public Transform cam;
        public ChunkCoord CurChunkCoord => World.ToChunkCoord(transform.position).Item1;

        public Block? HighlightBlock => GetComponent<Players.PlaceBlockComponent>().HighlightBlock;
        public Vector3Int? HighlightPos => GetComponent<Players.PlaceBlockComponent>().HighlightPos;
        public Vector3Int? PlacedPos => GetComponent<Players.PlaceBlockComponent>().PlacedPos;
        public Block SelectedBlock;

        public void Start()
        {
            cam = transform.GetChild(0);
        }
    }
}
