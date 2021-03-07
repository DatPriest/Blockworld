using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;


    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;

    }

    void Update()
    {
        string debugText = "test";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n\n";
        debugText += "XYZ: \n" +
            (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " +
            (Mathf.FloorToInt(world.player.transform.position.y)) + " / " +
            (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\nChunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks)  + " / " + (world.playerChunkCoord.z - halfWorldSizeInChunks);




        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
            timer += Time.deltaTime;
    }
}
