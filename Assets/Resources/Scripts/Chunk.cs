using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Chunk : MonoBehaviour {

    public static Vector3 standardSize = new Vector3(20, 120, 20);
    public static int minHeight = 10;
    public static float UVX_OFFSET = 0.001f;
    public static float UVY_OFFSET = 0.001f;
    public static float UV_SIZE = 0.0625f;

    public Vector3 size {get; set;}
    public Vector3 pos {get; set;}
    public BlockType[,,] blocks;
    public Mesh mesh{get; private set;}

    private List<Vector3> vertices;
    private List<int> faces;
    private List<Vector2> uvs;
    private int faceCount = 0;




    public Chunk()
    {
        
    }
    public Chunk(Vector3 v)
    {
        this.pos = v;
        this.size = standardSize;

        generateArray();
    }
    public Chunk(Vector3 v, Vector3 size)
    {
        this.pos = v;
        this.size = size;

        generateArray();
    }
    public Chunk(int x, int y, int z)
    {
        this.pos = new Vector3(x, y, z);
        this.size = standardSize;

        generateArray();
    }
    public Chunk(int x, int y, int z, Vector3 size)
    {
        this.pos = new Vector3(x, y, z);
        this.size = size;

        generateArray();
    }



    //Removes a block
    public void destroyBlock(Vector3 pos, bool recalculateMesh)
    {
        if (blocks[(int)(pos.x), (int)(pos.y), (int)(pos.z)] != BlockType.Lavastone)
        {
            blocks[(int)(pos.x), (int)(pos.y), (int)(pos.z)] = BlockType.Air;
            StartCoroutine(CreateMesh());

            //If it is a block at the chunkedge a neighbourchunk must be updated too
            Chunk c = findNeighbourChunk(pos);
            if (c != null)
            {
                StartCoroutine(c.CreateMesh());
            }
        }
    }



    //Sets a block
    public void setBlock(Vector3 pos, BlockType id, bool recalculateMesh)
    {
        //Removes the block if it is air
        if (id == BlockType.Air)
        {
            destroyBlock(pos, recalculateMesh);
            return;
        }

        //Sets a new block if there is no other block
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        if (blocks[x, y, z] == 0)
        {
            blocks[x, y, z] = id;
            if(recalculateMesh)
                StartCoroutine(CreateMesh());
        }
    }

    //Sets a block
    public void setBlockIgnoringAir(Vector3 pos, BlockType id, bool recalculateMesh)
    {
        //Sets a new block if there is no other block
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        if (blocks[x, y, z] == 0)
        {
            blocks[x, y, z] = id;
            if (recalculateMesh)
                StartCoroutine(CreateMesh());
        }
    }


    /*
     * If the block is at the chunkedge it returns the neighbourchunk
     */
    public Chunk findNeighbourChunk(Vector3 pos)
    {
        //x-border
        if ((int)pos.x == 0)
        {
            return World.findChunk(new Vector3(this.pos.x - size.x, 0, this.pos.z));
        }
        if ((int)pos.x == size.x - 1)
        {
            return World.findChunk(new Vector3(this.pos.x + size.x, 0, this.pos.z));
        }

        //z-border
        if ((int)pos.z == 0)
        {
            return World.findChunk(new Vector3(this.pos.x, 0, this.pos.z - size.z));
        }
        if ((int)pos.z == size.z - 1)
        {
            return World.findChunk(new Vector3(this.pos.x, 0, this.pos.z + size.z));
        }

        //The block is not at the chunkedge
        return null;
    }


    //Create the blockarray
    public void generateArray()
    {
        blocks = new BlockType[(int)size.x, (int)size.y, (int)size.z];

        //Set the seed
        Random.seed = World.currentWorld.seed;
        Vector3 offset = new Vector3(Random.value * 10000, Random.value * 10000, Random.value * 10000);

        //Create the Landscape with a SimplexNoise
        for (int x = 0; x < size.x; x++)
        {
            float noiseX = Mathf.Abs((x + (int)pos.x + offset.x) / size.x);
            for (int z = 0; z < size.z; z++)
            {
                float noiseZ = Mathf.Abs((z + (int)pos.z + offset.z) / size.z);

                float noiseValue = Mathf.PerlinNoise(noiseX/4, noiseZ/4);
                noiseValue *= (30 - minHeight);
                noiseValue += minHeight;

                //Generate Lavastone
                blocks[x, 0, z] = BlockType.Lavastone;

                //Generate Stone
                for (int y = 1; y < noiseValue-3; y++)
                {
                    blocks[x, y, z] = BlockType.Stone;
                }

                //Generate Dirt
                for (int y = (int)(noiseValue - 3); y < noiseValue; y++)
                {
                    blocks[x, y, z] = BlockType.Dirt;
                }

                //Generate Grass
                blocks[x, (int)noiseValue, z] = BlockType.Grass;
            }
        }

        generateTrees(0, 15);
       
    }

    //Generate trees
    public void generateTrees(int minNumber, int maxNumber)
    {
        Random.seed += (int)(pos.x + pos.z);
        int r = minNumber + (int)(Random.value * (maxNumber-minNumber));

        for (int i = 0; i < r; i++)
        {
            int x = (int)(Random.value * size.x) ;
            int z = (int)(Random.value * size.z);
            int y = calculateHeight(x, z);

            //If the height is to high there will be an exception
            //Trees should not spawn on other trees
            if (y < size.y - 6 && blocks[x, y, z] != BlockType.Wood && blocks[x, y, z] != BlockType.Leaves)
            {
                //Create a new structure for the tree
                Vector3 structurePos = new Vector3(x + this.pos.x, y, z + this.pos.z);
                World.currentWorld.structures.Add(new Structure(structurePos, ref Tree.tree));
            }
        }
    }


    public virtual IEnumerator CreateMesh()
    {
        mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        //Create the mesh for every visible face
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    if (blocks[x, y, z] == 0) continue;
                    
                    //Blockid
                    int id = (int)blocks[x, y, z];

                    // Left wall
                    if (IsTransparent(x - 1, y, z, x, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y, z + (int)pos.z), Vector3.up, Vector3.forward, false, ref verts, ref uvs, ref tris, Block.blockData[id].xLeft, Block.blockData[id].yLeft);
                    // Right wall
                    if (IsTransparent(x + 1, y, z, x, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x + 1, y + (int)pos.y, z + (int)pos.z), Vector3.up, Vector3.forward, true, ref verts, ref uvs, ref tris, Block.blockData[id].xRight, Block.blockData[id].yRight);

                    // Bottom wall
                    if (IsTransparent(x, y - 1, z, x, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y, z + (int)pos.z), Vector3.forward, Vector3.right, false, ref verts, ref uvs, ref tris, Block.blockData[id].xBottom, Block.blockData[id].yBottom);
                    // Top wall
                    if (IsTransparent(x, y + 1, z, x, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y + 1, z + (int)pos.z), Vector3.forward, Vector3.right, true, ref verts, ref uvs, ref tris, Block.blockData[id].xTop, Block.blockData[id].yTop);

                    // Back
                    if (IsTransparent(x, y, z - 1, x, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y, z + (int)pos.z), Vector3.up, Vector3.right, true, ref verts, ref uvs, ref tris, Block.blockData[id].xBack, Block.blockData[id].yBack);
                    // Front
                    if (IsTransparent(x, y, z + 1, x, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y, z + (int)pos.z + 1), Vector3.up, Vector3.right, false, ref verts, ref uvs, ref tris, Block.blockData[id].xFront, Block.blockData[id].yFront);
                }
            }
        }

        //Add the information to the mesh
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //Add the mesh to the gameobject
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        yield return 0;

    }


    public virtual void BuildFace(int brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> tris, float uvX, float uvY)
    {
        int index = verts.Count;
        faceCount++;

        verts.Add(corner);
        verts.Add(corner + up);
        verts.Add(corner + up + right);
        verts.Add(corner + right);

        uvs.Add(new Vector2(uvX * UV_SIZE + UVX_OFFSET, 1f - (uvY + 1f) * UV_SIZE + UVY_OFFSET));
        uvs.Add(new Vector2(uvX * UV_SIZE + UVX_OFFSET, 1f - uvY * UV_SIZE - UVY_OFFSET));
        uvs.Add(new Vector2((uvX + 1f) * UV_SIZE - UVX_OFFSET, 1f - uvY * UV_SIZE - UVY_OFFSET));
        uvs.Add(new Vector2((uvX + 1f) * UV_SIZE - UVX_OFFSET, 1f - (uvY + 1f) * UV_SIZE + UVY_OFFSET));

        if (reversed)
        {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
        }
        else
        {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
        }

    }

    //x/y/z: The Neighbourblock
    //x2´/y2/z2: The selected Block
    public virtual bool IsTransparent(int x, int y, int z, int x2, int y2, int z2)
    {
        int brick = GetByte(x, y, z);
        int brick2 = GetByte(x2, y2, z2);

        if((brick == (int)BlockType.Air) || 
           (Block.blockData[brick].isTransparent && brick != brick2))
                return true;
        else
            return false;
    }

    /*
     * Returns 0 or 1.
     * 1 is air or the worldborder 
     * 0 is a Block
     */
    public virtual int GetByte(int x, int y, int z)
    {
        //Left
        if((x < 0))
        {
            Chunk c = World.findChunk(new Vector3(pos.x-size.x, 0, pos.z));
            if (c != null)
            {
                return c.GetByte((int)size.x - 1, y, z);
            }
            else
            {
                return 1;
            }
        }
        //Right
        if ((x >= size.x))
        {
            Chunk c = World.findChunk(new Vector3(pos.x + size.x, 0, pos.z));
            if (c != null)
            {
                return c.GetByte(0, y, z);
            }
            else
            {
                return 1;
            }
        }

        //Bottom    
        if ((y < 0))
            return 1;
        //Top
        if ((y >= size.y))
            return 0;

        //Back
        if((z < 0))
        {
            Chunk c = World.findChunk(new Vector3(pos.x, 0, pos.z - size.z));
            if (c != null)
            {
                return c.GetByte(x, y, (int)size.z - 1);
            }
            else
            {
                return 1;
            }
        }
        //Front
        if ((z >= size.z))
        {
            Chunk c = World.findChunk(new Vector3(pos.x, 0, pos.z + size.z));
            if (c != null)
            {
               return c.GetByte(x, y, 0);
            }
            else
            {
                return 1;
            }
        }
        if ((x < 0) || (y < 0) || (z < 0) || (x >= size.x) || (y >= size.y) || (z >= size.z))
            return 0;

        //No Border
        return (int)blocks[x, y, z];
    }


    //Calculates the ypos of the highest block at x/z
    public int calculateHeight(int x, int z)
    {
        for (int y = (int)size.y-1; y > 0; y--)
        {
            if (blocks[x, y, z] != BlockType.Air)
                return y;
        }

        return 0;
    }


    //Returns the ChunkPosition of this coordinate. There must be no existing chunk with this position
    public static Vector3 roundChunkPos(Vector3 pos)
    {
        float x = 0;
        float z = 0;

        if(pos.x < 0 && pos.x % standardSize.x < 0)
        {
            //The first part cuts the decimals
            x = (int)(pos.x / standardSize.x) * standardSize.x - standardSize.x;
        }
        else
        {
            x = (int)(pos.x / standardSize.x) * standardSize.x;
        }

        if (pos.z < 0 && pos.z % standardSize.z < 0)
        {
            //The first part cuts the decimals
            z = (int)(pos.z / standardSize.z) * standardSize.z - standardSize.z;
        }
        else
        {
            z = (int)(pos.z / standardSize.z) * standardSize.z;
        }

        return new Vector3(x, 0, z);
    }



    //Place a part of the structure in the chunk
    public void placeStructure(Structure structure)
    {
        //chunk position in the structurearray
        Vector3 startPosition = pos - structure.pos;
        //print("Startposition: " + startPosition + "  Chunkposition: " + pos + "  Structureposition: " + structure.pos);
        float xNegative = 0,
             zNegative = 0; 

        //The values should only be positive
        if (startPosition.x < 0) {
            xNegative = -startPosition.x;
            startPosition.x = 0;
        }
        if (startPosition.y < 0)
        {
            startPosition.y = -startPosition.y;
        }
        if (startPosition.z < 0) {
            zNegative = -startPosition.z;
            startPosition.z = 0;
        }

        //Calculates how big the structurepart in the chunk is
        float xLength = structure.blocks.GetLength(0) - startPosition.x,
              yLength = structure.blocks.GetLength(1),
              zLength = structure.blocks.GetLength(2) - startPosition.z;
        if (xLength > standardSize.x) xLength = standardSize.x;
        if (yLength + startPosition.y >= standardSize.y) yLength = standardSize.y - startPosition.y;
        if (zLength > standardSize.z) zLength = standardSize.z;
       
        //Adds this values to the blockposition when the structure doesn't start in the left bottom corner
        float xStart = 0, zStart = 0;
        if (xNegative != 0) {
            xStart = xNegative;
            xLength = Chunk.standardSize.x - xNegative;
            //print("xNegative");
            if (xLength > structure.blocks.GetLength(0)) xLength = structure.blocks.GetLength(0);
        }
        if (zNegative != 0) {
            zStart = zNegative;
            zLength = Chunk.standardSize.z - zNegative;
            //print("zNegative");
            if (zLength > structure.blocks.GetLength(2)) zLength = structure.blocks.GetLength(2);
        }


        /*
         *Place the structure
         The Vector posBlock is defined here for optimization
         */    
        Vector3 posBlock;
        for (int x = 0; x < xLength; x++)
        {
            int arrayX = (int)(startPosition.x + x);
            posBlock.x = x + xStart;

            for (int y = 0; y < yLength; y++)
            {
                posBlock.y = startPosition.y + y;

                for (int z = 0; z < zLength; z++)
                {
                    posBlock.z = z + zStart;

                    //Set the block at the arrayPosition x, y, z in the chunk
                    if (blocks[(int)posBlock.x, (int)posBlock.y, (int)posBlock.z] == (int)BlockType.Air)
                    {
                        BlockType blockType = (BlockType)structure.blocks[arrayX, y, (int)(startPosition.z + z)];
                        if (blockType != BlockType.Air)
                            setBlockIgnoringAir(posBlock, blockType, false);
                    }
                }
            }
        }
    }



    //Save the chunkData
    public virtual IEnumerator saveChunk()
    {
        SaveManager.writeArray(pos.ToString(), ref blocks);
        yield return 0;
    }

    public virtual IEnumerator loadChunk()
    {
        //Generate the blockarray
        blocks = new BlockType[(int)size.x, (int)size.y, (int)size.z];

        //Read the file
        List<string> list = SaveManager.readFile(pos.ToString());
        string[] array = list.ToArray();

        //Position in the array
        int x = 0;
        int y = 0;
        int z = 0;

        //Convert the list to a 3d-array
        foreach (string yRow in array)
        {
            string[] values = yRow.Trim().Split(' ');
            foreach (string value in values)
            {
                string[] blocksBlock = value.Trim().Split('x');
                int id = System.Int32.Parse(blocksBlock[0]);
                int number;

                //Tests how many blocks are in the selected block
                if (blocksBlock.Length == 1)
                    number = 1;
                else
                    number = System.Int32.Parse(blocksBlock[1]);

                /*
                 * Sets the blocks in the selected block into the blocksarray
                 */
                //Airblocks shouldn't be placed because of the performance
                if (id == (int)BlockType.Air)
                {
                    y += (int)(number / standardSize.z);
                    int deltaZ = (int)(number % standardSize.z);
                    for (int i = 0; i < deltaZ; i++)
                    {
                        z++;
                        if (z >= standardSize.z)
                        {
                            y++;
                            z = 0;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < number; i++)
                    {
                        blocks[x, y, z] = (BlockType)id;

                        //Actualize the position
                        z++;
                        if (z >= standardSize.z)
                        {
                            y++;
                            z = 0;
                        }
                    }
                }
            }

            //Next line
            x++;
            y = 0;
            z = 0;
        }
        yield return 0;
    }
}
