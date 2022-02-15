using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class FocusBlockComponent : MonoBehaviour
    {
        protected float checkIncrement = 0.1f;

        [SerializeField]
        public Transform HighlightBlock;

        protected Player player;
        protected World world;

        public Block? CurSelectedBlock { get; protected set; }
        protected Transform Cam => player.cam;

        public void Start()
        {
            player = GetComponent<Player>();
            world = GameObject.Find("World").GetComponent<World>();
        }

        private void Update()
        {
            float curReach = checkIncrement;
            Vector3 targetPos = transform.position;

            while (curReach < player.Reach)
            {
                targetPos = Cam.position + Cam.forward * curReach;
                var tempBlock = world.GetBlock(targetPos);
                if (tempBlock.IsSolid)
                {
                    Vector3 blockPos = new Vector3(Mathf.FloorToInt(targetPos.x), Mathf.FloorToInt(targetPos.y), Mathf.FloorToInt(targetPos.z));
                    HighlightBlock.position = blockPos;
                    HighlightBlock.gameObject.SetActive(true);

                    CurSelectedBlock = tempBlock;
                    return;
                }

                curReach += checkIncrement;
            }

            HighlightBlock.gameObject.SetActive(false);
        }
    }
}