using UnityEngine;
using System.Collections.Generic;

public class Structure{
    public Vector3 pos;
    public int[,,] blocks;
    public List<Vector3> chunks = new List<Vector3>();  //x, y, z

    public Structure(Vector3 pos, int[,,] blocks)
    {
        this.pos = pos;
        this.blocks = blocks;

        calculateChunks();
    }

    public Structure(Vector3 pos, int sizeX, int sizeY, int sizeZ)
    {
        this.pos = pos;
        this.blocks = new int[sizeX, sizeY, sizeZ];
    }

    //Calculates all chunks which are in the region of this Structure
    public void calculateChunks()
    {
        int sizeX = blocks.GetLength(0);
        int sizeZ = blocks.GetLength(2);

        /*
         * Calculates in how much chunks the structure exists
         */
        int x = (int)(sizeX / Chunk.standardSize.x);
        int z = (int)(sizeZ / Chunk.standardSize.z);

        //This if is important for structures which are on chunkedges
        Vector3 cPosLastNormal = Chunk.roundChunkPos(pos);
        Vector3 cPosLastReal   = Chunk.roundChunkPos(pos + new Vector3(sizeX-1, 0, sizeZ-1));
        if (cPosLastNormal.x + x * Chunk.standardSize.x != cPosLastReal.x)
            x++;
        if (cPosLastNormal.z + z * Chunk.standardSize.z != cPosLastReal.z)
            z++;


        //Adds the chunks
        for (int i = 0; i <= x; i++)
        {
            for (int j = 0; j <= z; j++)
            {
                //Calculates the chunkPosition of the coordinate
                Vector3 chunkPos = Chunk.roundChunkPos(new Vector3(pos.x + i * Chunk.standardSize.x, 0, pos.z + j * Chunk.standardSize.z));
                Debug.Log("Structure: " + this.pos + "   Chunk: " + chunkPos + "i: " + i + "  j: " + j);
                chunks.Add(chunkPos);
            }
        }
        /*string s = "";
        foreach (Vector3 item in chunks.ToArray())
        {
            s += item + ", ";
        }
        Debug.Log("Position: " + pos + "   Chunks: " + s);*/
    }
}
