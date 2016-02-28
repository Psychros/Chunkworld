using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class InputManager : MonoBehaviour {


	public static InputManager instance { get; private set; }
    public int actiondistance = 10;  //Minimum distance the player needs to destroy/place a block

	//All KeyCodes
	public KeyCode destroyBlock = KeyCode.Mouse0;
    public KeyCode setBlock     = KeyCode.Mouse1;

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
    }


    void Update()
    {
        //Destroy a block
        if (Input.GetKeyDown(destroyBlock))
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
                c.destroyBlock(pos);
            }
        }

        //Set a block
        if (Input.GetMouseButtonDown(1))
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

                //Place the block
                c.setBlock(pos, selectedBlockType);

                //Update the neighbourchunk
                if (c != d)
                    StartCoroutine(d.CreateMesh());
            }
        }


        /*
         * Inventoryslots
         */
        if (Input.GetKeyDown(slot1))
            selectedBlockType = BlockType.Stone;
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
    }

    public static Vector3 substractVector3(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
}