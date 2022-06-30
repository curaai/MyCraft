using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft.UI
{
    public class DebugScreen : MonoBehaviour
    {
        [SerializeField]
        Player player;
        World world;
        Text textUI;

        float fpsCalcTimer;
        float fps;

        void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            textUI = GetComponent<Text>();
        }
        void Update()
        {
            var texts = new List<string>();
            texts.Add("Curaai Minecraft CloneCoding");
            texts.Add($"{fps}fps");
            var voxelPos = player.transform.position;
            texts.Add($"XYZ: {voxelPos.x}/{voxelPos.y}/{voxelPos.z}");
            var chunkCoord = player.CurChunkCoord;
            texts.Add($"Chunk: {chunkCoord.x}/{chunkCoord.z}");

            textUI.text = String.Join("\n", texts);
            if (1 < fpsCalcTimer)
            {
                fps = (int)(1f / Time.unscaledDeltaTime);
                fpsCalcTimer = 0;
            }
            else
                fpsCalcTimer += Time.deltaTime;
        }
    }
}