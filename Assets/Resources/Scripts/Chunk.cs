using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;

public class Chunk : MonoBehaviour {

    public static Vector3 standardSize = new Vector3(20, 60, 20);
    public static int minHeight = 10;
    public const float UV_OFFSET = 0.001f;
    public const float UV_SIZE = 1f / 16;
    public Vector3 size
    {
        get;
        set;
    }
    public Vector3 pos
    {
        get;
        set;
    }
    public BlockType[,,] blocks;

    public Mesh mesh
    {
        get;
        private set;
    }
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
    public void destroyBlock(Vector3 pos)
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



    //Sets a block
    public void setBlock(Vector3 pos, BlockType id)
    {
        //Removes the block if it is air
        if (id == BlockType.Air)
        {
            destroyBlock(pos);
            return;
        }

        //Sets a new block
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        blocks[x, y, z] = id;
        StartCoroutine(CreateMesh());
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

                float noiseValue = Noise.Generate(noiseX/6, noiseZ/6);
                noiseValue += 1;
                noiseValue *= (30 - minHeight)/2;
                noiseValue += minHeight;

                //Generate Stone
                for (int y = 0; y < noiseValue-3; y++)
                {
                    blocks[x, y, z] = BlockType.Stone;
                }
                //Generate Dirt
                for (int y = (int)(noiseValue - 3); y < noiseValue; y++)
                {
                    blocks[x, y, z] = BlockType.Dirt;
                }
                blocks[x, (int)noiseValue, z] = BlockType.Grass;
            }
        }

        generateTrees(5, 15);
       
    }

    //Generate trees
    public void generateTrees(int minNumber, int maxNumber)
    {
        int r = minNumber + (int)(Random.value * (maxNumber-minNumber));
        for (int i = 0; i < r; i++)
        {
            int x = (int)(Random.value * size.x);
            int z = (int)(Random.value * size.z);

            int y = calculateHeight(x, z);

            //If the height is to high there will be an exception
            //Trees should not spawn on other trees
            if (y < size.y - 6 && blocks[x, y, z] != BlockType.Wood && blocks[x, y, z] != BlockType.Leaves)
            {
                //Leaves
                for (int x2 = -2; x2 < 2; x2++)
                {
                    for (int z2 = -2; z2 < 2; z2++)
                    {
                        if (x + x2 >= 0 && x + x2 < size.x && z + z2 >= 0 && z + z2 < size.z)
                        {
                            for (int y2 = 4; y2 < 7; y2++)
                            {
                                blocks[x + x2, y + y2, z + z2] = BlockType.Leaves;
                            }
                        }
                    }
                }
                //Wood
                for (int j = 1; j < 6; j++)
                {
                    blocks[x, y + j, z] = BlockType.Wood;
                }
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
                    if (IsTransparent(x - 1, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y, z + (int)pos.z), Vector3.up, Vector3.forward, false, ref verts, ref uvs, ref tris, Block.blockData[id].xLeft, Block.blockData[id].yLeft);
                    // Right wall
                    if (IsTransparent(x + 1, y, z))
                        BuildFace(id, new Vector3(x + (int)pos.x + 1, y + (int)pos.y, z + (int)pos.z), Vector3.up, Vector3.forward, true, ref verts, ref uvs, ref tris, Block.blockData[id].xRight, Block.blockData[id].yRight);

                    // Bottom wall
                    if (IsTransparent(x, y - 1, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y, z + (int)pos.z), Vector3.forward, Vector3.right, false, ref verts, ref uvs, ref tris, Block.blockData[id].xBottom, Block.blockData[id].yBottom);
                    // Top wall
                    if (IsTransparent(x, y + 1, z))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y + 1, z + (int)pos.z), Vector3.forward, Vector3.right, true, ref verts, ref uvs, ref tris, Block.blockData[id].xTop, Block.blockData[id].yTop);

                    // Back
                    if (IsTransparent(x, y, z - 1))
                        BuildFace(id, new Vector3(x + (int)pos.x, y + (int)pos.y, z + (int)pos.z), Vector3.up, Vector3.right, true, ref verts, ref uvs, ref tris, Block.blockData[id].xBack, Block.blockData[id].yBack);
                    // Front
                    if (IsTransparent(x, y, z + 1))
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
    public virtual void BuildFace(int brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> tris, int uvX, int uvY)
    {
        int index = verts.Count;
        faceCount++;

        verts.Add(corner);
        verts.Add(corner + up);
        verts.Add(corner + up + right);
        verts.Add(corner + right);

        uvs.Add(new Vector2(uvX*UV_SIZE, 1-((uvY+1)* UV_SIZE - UV_OFFSET)));
        uvs.Add(new Vector2(uvX* UV_SIZE, 1-uvY* UV_SIZE - UV_OFFSET));
        uvs.Add(new Vector2((uvX + 1) * UV_SIZE - UV_OFFSET, 1 - uvY * UV_SIZE - UV_OFFSET));
        uvs.Add(new Vector2((uvX + 1) * UV_SIZE - UV_OFFSET, 1-((uvY+1)* UV_SIZE - UV_OFFSET)));

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
    public virtual bool IsTransparent(int x, int y, int z)
    {
        int brick = GetByte(x, y, z);

        if(brick == (int)BlockType.Air || brick == (int)(BlockType.Glass) || brick == (int)(BlockType.Leaves))
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
        int n = 0;
        for (int y = (int)size.y-1; y > 0; y--)
        {
            if (blocks[x, y, z] != BlockType.Air)
                return y;
        }

        return 0;
    }
}
