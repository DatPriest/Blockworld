using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public BlockType[] blockTypes;

    public Chunk[,] chunkList = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public int countActiveChunks;

    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    List<Chunk> chunksToUpdate = new List<Chunk>();

    bool applyingModifications = false;

    Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    public GameObject debugScreen;

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, biome.terrainHeight + 50f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // Only update the chunks if the player has moved from the chunk they were previously on
        
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (modifications.Count > 0 && !applyingModifications)
            StartCoroutine(ApplyModifications());

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToUpdate.Count > 0)
            UpdateChunks();

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);

        // DEBUG
        countActiveChunks = activeChunks.Count;
        //Debug.Log(countActiveChunks);
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunkList[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();

            ChunkCoord c = GetChunkCoordFromVector3(v.position);

            if (chunkList[c.x, c.z] == null)
            {
                chunkList[c.x, c.z] = new Chunk(c, this, true);
                activeChunks.Add(c);
            }

            chunkList[c.x, c.z].modifications.Enqueue(v);

            if (!chunksToUpdate.Contains(chunkList[c.x, c.z]))
                chunksToUpdate.Add(chunkList[c.x, c.z]);
        }

        for (int i = 0; i < chunksToUpdate.Count; i++)
        {
            chunksToUpdate[0].UpdateChunk();
            chunksToUpdate.RemoveAt(0);
        }

        player.position = spawnPosition;
    }

    void CreateChunk ()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunkList[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        while(!updated && index < chunksToUpdate.Count - 1)
        {
            if (chunksToUpdate[index].isVoxelMapPopulated)
            {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
                index++;
        }
    }

    IEnumerator ApplyModifications ()
    {
        applyingModifications = true;
        int count = 0;

        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();

            ChunkCoord c = GetChunkCoordFromVector3(v.position);

            if (chunkList[c.x, c.z] == null)
            {
                chunkList[c.x, c.z] = new Chunk(c, this, true);
                activeChunks.Add(c);
            }

            chunkList[c.x, c.z].modifications.Enqueue(v);

            if (!chunksToUpdate.Contains(chunkList[c.x, c.z]))
                chunksToUpdate.Add(chunkList[c.x, c.z]);

            count++;
            if (count > 200)
            {
                count = 0;
                yield return null;
            }
        }

        applyingModifications = false;
    }



    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3( Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunkList[x, z];
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        playerLastChunkCoord = playerChunkCoord;

        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunkList[x, z] == null)
                    {
                        chunkList[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    } else if (!chunkList[x, z].isActive)
                    {
                        chunkList[x, z].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x, z));

                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(ChunkCoord c in previouslyActiveChunks)
        {
            chunkList[c.x, c.z].isActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunkList[thisChunk.x, thisChunk.z] != null && chunkList[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blockTypes[chunkList[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunkList[thisChunk.x, thisChunk.z] != null && chunkList[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blockTypes[chunkList[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent;

        return blockTypes[GetVoxel(pos)].isTransparent;
    }

    public byte GetVoxel(Vector3 pos )
    {
        int yPos = Mathf.FloorToInt(pos.y);


        /* IMMUTABLE PASS */

        // If outside of world return air.
        if (!IsVoxelInWorld(pos))
            return 0;

        // if bottom block of chunk return bedrock
        if (yPos == 0)
        {
            return 1;
        }

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight* Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale) ) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
        {
            voxelValue = 3;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = 5;
        }
        else if (yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 2;
        }

        /* SECOND PASS */

        if (voxelValue == 2)
        {
            foreach(Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockId;
                    }
                }
            }
        }

        /* TREE PASS */

        if (yPos == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treePlacementScale) > biome.treePlacementThreshold)
                {
                    Structure.MakeTree(pos, modifications, biome.minTreeHeight, biome.maxTreeHeight);
                }
            }
        }

        return voxelValue;

    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return 
                true;
        else 
            return 
                false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return 
                true;
        else 
            return 
                false;
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public bool isTransparent;
    public Sprite icon;

    [Header("Texture Values")]
    public byte backFaceTexture;
    public byte frontFaceTexture;
    public byte topFaceTexture;
    public byte bottomFaceTexture;
    public byte leftFaceTexture;
    public byte rightFaceTexture;

    public byte GetTextureId( byte faceIndex )
    {
        switch( faceIndex )
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureId; invalid face index");
                return 0;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}
