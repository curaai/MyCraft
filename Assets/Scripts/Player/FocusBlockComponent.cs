using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusBlockComponent : MonoBehaviour
{
    protected float checkIncrement = 0.1f;
    protected Player player;
    protected Transform cam { get { return player.cam; } }
    protected World world;
    public Transform HighlightBlock;

    public Block? CurSelectedBlock { get; protected set; }

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
            targetPos = cam.position + cam.forward * curReach;
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