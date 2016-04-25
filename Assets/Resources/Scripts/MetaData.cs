using UnityEngine;
using System.Collections.Generic;

public class MetaData : MonoBehaviour {
    public Vector3 pos;
    public int[,,] blocks;
    public List<Vector3> chunks = new List<Vector3>();  //x, y, z

    public MetaData(Vector3 pos, int[,,] blocks)
    {
        this.pos = pos;
        this.blocks = blocks;
    }

    public MetaData(Vector3 pos, int sizeX, int sizeY, int sizeZ)
    {
        this.pos = pos;
        this.blocks = new int[sizeX, sizeY, sizeZ];
    }

    //Calculates all chunks which are in the region of this Structure
    public void calculateChunks()
    {
        int sizeX = blocks.GetLength(0);
        int sizeZ = blocks.GetLength(2);

        //Adds the chunks
        for (int i = 0; i < sizeX / Chunk.standardSize.x; i++)
        {
            for (int j = 0; j < sizeZ / Chunk.standardSize.z; j++)
            {
                //Calculates the chunkPosition of the coordinate
                chunks.Add(Chunk.roundChunkPos(new Vector3(pos.x + i * Chunk.standardSize.x, 0, pos.x + j * Chunk.standardSize.x)));
            }
        }
    }
}
