using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {


	public static InputManager instance { get; private set; }
    public int actiondistance = 10;  //Minimum distance the player needs to destroy/place a block
    public float timePlaceBlock = 0.25f;
    private float timerPlaceBlock = 0;
    public float timeDestroyBlock = 1f;
    private float timerDestroyBlock = 0;

    //Selectionblock
    private int selectionTexture = 0;
    public const float UV_OFFSET = 0.0001f;
    public const float UV_SIZE = 1f / 5;
    private GameObject selectionBlock;
    public Material selectionMaterial;

    //All KeyCodes
    public KeyCode destroyBlock    = KeyCode.Mouse0;   //Destroy a block
    public KeyCode setBlock        = KeyCode.Mouse1;   //Place a block
    public KeyCode activateLantern = KeyCode.F;        //Activate the lantern of the player

    //Inventoryslots
    public BlockType selectedBlockType = BlockType.Stone;
    public KeyCode slot0 = KeyCode.Alpha0;
    public KeyCode slot1 = KeyCode.Alpha1;
    public KeyCode slot2 = KeyCode.Alpha2;
    public KeyCode slot3 = KeyCode.Alpha3;
    public KeyCode slot4 = KeyCode.Alpha4;
    public KeyCode slot5 = KeyCode.Alpha5;
    public KeyCode slot6 = KeyCode.Alpha6;
    public KeyCode slot7 = KeyCode.Alpha7;
    public KeyCode slot8 = KeyCode.Alpha8;
    public KeyCode slot9 = KeyCode.Alpha9;


    void Start () {
        InputManager.instance = this;

        //Initialize the selectionBlock
        selectionBlock = new GameObject();
        selectionBlock.SetActive(false);
        MeshRenderer mR = selectionBlock.AddComponent<MeshRenderer>();
        selectionBlock.AddComponent<MeshFilter>();
        mR.material = selectionMaterial;

        //Set the selectedBlockType
        selectedBlockType = BlockType.Stone;
    }


    void Update()
    {
        //Destroy a block
        if (Input.GetKey(destroyBlock))                 //The player needs a time for destro
        {
            if (timerDestroyBlock >= timeDestroyBlock)
            {
                removeBlock();
                timerDestroyBlock = 0;
            }

            timerDestroyBlock += Time.deltaTime;
        }
        if (Input.GetKeyUp(destroyBlock))
        {
            timerDestroyBlock = 0;
        }

        //Set a block
        if (Input.GetMouseButtonDown(1))
        {
            placeBlock();
        }
        if (Input.GetMouseButton(1))//The player does't have to click for every block again
        {
            if (timerPlaceBlock >= timePlaceBlock)
            {
                placeBlock();
                timerPlaceBlock = 0;
            }

            timerPlaceBlock += Time.deltaTime;
        }
        if (Input.GetMouseButtonUp(1))
        {
            timerPlaceBlock = 0;
        }


        /*
         * Inventoryslots
         */
        if (Input.GetKeyDown(slot1))
        {
            selectedBlockType = BlockType.Stone;
            print(Block.blockData[(int)BlockType.Stone].xLeft + ", " + Block.blockData[(int)BlockType.Stone].yLeft);
        }
        if (Input.GetKeyDown(slot2))
            selectedBlockType = BlockType.Dirt;
        if (Input.GetKeyDown(slot3))
            selectedBlockType = BlockType.Grass;
        if (Input.GetKeyDown(slot4))
            selectedBlockType = BlockType.Wood;
        if (Input.GetKeyDown(slot5))
            selectedBlockType = BlockType.Leaves;
        if (Input.GetKeyDown(slot6))
            selectedBlockType = BlockType.Glass;


        //Activate the lantern
        if (Input.GetKeyDown(activateLantern))
            World.currentWorld.lanternTransform.SetActive(!World.currentWorld.lanternTransform.activeInHierarchy);


        //Recalculate the selected Block and place the selection
        //recalculateSelectionBlock();
    }

    public static Vector3 substractVector3(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public void removeBlock()
    {
        RaycastHit hit = RayCastManager.rayCastBlock(actiondistance);
        if (hit.point != Vector3.zero)
        {

            Chunk c = hit.transform.gameObject.GetComponent<Chunk>();
            Vector3 pos = substractVector3(hit.point, c.pos);

            //The viewdirection has an influence on the correct blockselecting
            Vector3 direction = World.currentWorld.playerTransform.TransformDirection(Vector3.forward);
            if (direction.x < 0)
                pos.x -= 0.001f;
            if (direction.y < 0)
                pos.y -= 0.001f;
            if (direction.z < 0)
                pos.z -= 0.001f;
            c.destroyBlock(pos, true);

            //Save the chunk
            StartCoroutine(c.saveChunk());
        }
    }

    public void placeBlock()
    {
        RaycastHit hit = RayCastManager.rayCastBlock(actiondistance);
        if (hit.point != Vector3.zero)
        {

            Chunk c = hit.transform.gameObject.GetComponent<Chunk>();
            Chunk d = c;   //Copy of the selected chunk. Both chunks must be updated
            Vector3 pos = substractVector3(hit.point, c.pos);

            //The viewdirection has an influence on the correct blockselecting
            Vector3 direction = World.currentWorld.playerTransform.TransformDirection(Vector3.forward);
            if (direction.x >= 0)
                pos.x -= 0.001f;
            if (direction.y >= 0)
                pos.y -= 0.001f;
            if (direction.z >= 0)
                pos.z -= 0.001f;

            //Select the correct chunk if the selected block is at the edge
            //x-border
            if (pos.x < 0)
            {
                c = World.findChunk(new Vector3(c.pos.x - Chunk.standardSize.x, 0, c.pos.z));
                pos.x = Chunk.standardSize.x - 1;
            }
            else if (pos.x >= Chunk.standardSize.x)
            {
                c = World.findChunk(new Vector3(c.pos.x + Chunk.standardSize.x, 0, c.pos.z));
                pos.x = 0;
            }

            //z-border
            if (pos.z < 0)
            {
                c = World.findChunk(new Vector3(c.pos.x, 0, c.pos.z - Chunk.standardSize.z));
                pos.z = Chunk.standardSize.z - 1;
            }
            else if (pos.z >= Chunk.standardSize.z)
            {
                c = World.findChunk(new Vector3(c.pos.x, 0, c.pos.z + Chunk.standardSize.z));
                pos.z = 0;
            }



            //The player should not be in a block when this block is placed
            Transform t = World.currentWorld.playerTransform;
            Vector3 pos1 = new Vector3((int)(t.position.x - c.pos.x), (int)(t.position.y - .9f - c.pos.y), (int)(t.position.z - c.pos.z));
            Vector3 pos2 = new Vector3((int)(t.position.x - c.pos.x), (int)(t.position.y + .2f - c.pos.y), (int)(t.position.z - c.pos.z));
            Vector3 posEqual = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);

            if (!(posEqual.Equals(pos1) || posEqual.Equals(pos2)))
            {
                //Place the block
                c.setBlock(pos, selectedBlockType, true);

                //Save the chunk
                StartCoroutine(c.saveChunk());

                //Update the neighbourchunk
                if (c != d)
                {
                    StartCoroutine(d.CreateMesh());
                }
            }
        }
    }




    //Recalculate the selected Block and place the selection
    public void recalculateSelectionBlock()
    {
        RaycastHit hit = RayCastManager.rayCastBlock(actiondistance);
        if (hit.point != Vector3.zero)
        {
            //The viewdirection has an influence on the correct blockselecting
            Vector3 direction = World.currentWorld.playerTransform.TransformDirection(Vector3.forward);
            Vector3 pos = hit.point;
            if (direction.x < 0 && pos.x - (int)(pos.x + 0.0001f) <= 0.0001f)
            {
                pos.x--;
            }
            if (direction.y < 0 && pos.y - (int)pos.y <= 0.0001f)
            {
                pos.y--;
            }
            if (direction.z < 0 && pos.z - (int)(pos.z+0.0001f) <= 0.0001f)
            {
                pos.z--;
            }
            if (direction.z > 0 && pos.z - (int)(pos.z + 0.0001f) <= 0.0001f && pos.z >= 0)
            {
                pos.z++;
            }
           

            //Correct the coords if the player is at negative coords
            if (hit.point.x < 0 && direction.x > 0)
            {
                pos.x--;
            }
            if (hit.point.z < 0 && direction.z > 0)
            {
                pos.z--;
            }

            Mesh mesh = new Mesh();
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            Vector3 scale = new Vector3(1f, 1f, 1f);

            // Left wall
            BuildFace(new Vector3(0, 0, 0), Vector3.Scale(Vector3.up, scale), Vector3.Scale(Vector3.forward, scale), false, ref verts, ref uvs, ref tris);
            // Right wall
            BuildFace(new Vector3(1, 0, 0), Vector3.Scale(Vector3.up, scale), Vector3.Scale(Vector3.forward, scale), true, ref verts, ref uvs, ref tris);

            // Bottom wall
            BuildFace(new Vector3(0, 0, 0), Vector3.Scale(Vector3.forward, scale), Vector3.Scale(Vector3.right, scale), false, ref verts, ref uvs, ref tris);
            // Top wall
            BuildFace(new Vector3(0, 1, 0), Vector3.Scale(Vector3.forward, scale), Vector3.Scale(Vector3.right, scale), true, ref verts, ref uvs, ref tris);

            // Back
            BuildFace(new Vector3(0, 0, 0), Vector3.Scale(Vector3.up, scale), Vector3.Scale(Vector3.right, scale), true, ref verts, ref uvs, ref tris);
            // Front
            BuildFace(new Vector3(0, 0, 1), Vector3.Scale(Vector3.up, scale), Vector3.Scale(Vector3.right, scale), false, ref verts, ref uvs, ref tris);


            //Add the information to the mesh
            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            //Add the mesh to the gameobject
            MeshFilter filter = selectionBlock.GetComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            selectionBlock.transform.position = new Vector3((int)(pos.x), (int)(pos.y), (int)(pos.z));
            selectionBlock.SetActive(true);
        }
        else
        {
            selectionBlock.SetActive(false);
        }
    }

    public virtual void BuildFace(Vector3 corner, Vector3 up, Vector3 right, bool reversed, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> tris)
    {
        int index = verts.Count;

        verts.Add(corner);
        verts.Add(corner + up);
        verts.Add(corner + up + right);
        verts.Add(corner + right);

        uvs.Add(new Vector2(selectionTexture * UV_SIZE, 1));
        uvs.Add(new Vector2(selectionTexture * UV_SIZE, 0));
        uvs.Add(new Vector2((selectionTexture + 1) * UV_SIZE - UV_OFFSET, 0));
        uvs.Add(new Vector2((selectionTexture + 1) * UV_SIZE - UV_OFFSET, 1));

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
}