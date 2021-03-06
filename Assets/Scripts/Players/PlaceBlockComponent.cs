using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Players
{
    public class PlaceBlockComponent : MonoBehaviour
    {
        protected float checkIncrement = 0.1f;

        [SerializeField]
        public Transform HighlightBlockTransform;

        protected Player player;
        protected World world;
        protected Transform Cam => player.cam;

        public byte? HighlightBlock { get; protected set; }
        public Vector3Int? HighlightPos { get; protected set; }
        public Vector3Int? PlacedPos { get; protected set; }
        public Material DestroyMaterial => HighlightBlockTransform.GetChild(0).GetComponent<MeshRenderer>().materials[1];

        public void Start()
        {
            player = GetComponent<Player>();
            world = GameObject.Find("World").GetComponent<World>();
        }

        private void Update()
        {
            float curReach = checkIncrement;
            var targetPos = transform.position;

            while (curReach < player.Reach)
            {
                targetPos = Cam.position + Cam.forward * curReach;

                var blockPos = Vector3Int.FloorToInt(targetPos);
                var tempBlock = world.GetBlock(blockPos);
                if (world.BlockTable[tempBlock].isSolid)
                {
                    HighlightBlockTransform.position = blockPos;
                    HighlightBlockTransform.gameObject.SetActive(true);

                    HighlightBlock = tempBlock;
                    HighlightPos = blockPos;
                    PlacedPos = Vector3Int.FloorToInt(targetPos - Cam.forward * checkIncrement);
                    return;
                }

                curReach += checkIncrement;
            }

            HighlightBlockTransform.gameObject.SetActive(false);
            HighlightBlock = null;
            HighlightPos = null;
            PlacedPos = null;
        }
    }
}
