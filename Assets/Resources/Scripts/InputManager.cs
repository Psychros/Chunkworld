using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class InputManager : MonoBehaviour {


	public static InputManager instance { get; private set; }
    public int actiondistance = 10;  //Minimum distance the player needs to destroy/place a block

	//All KeyCodes
	public KeyCode destroyBlock = KeyCode.Mouse0;


	void Start () {
        InputManager.instance = this;
    }


    void Update () {
		//Destroy a block
		if(Input.GetKeyDown(destroyBlock)){
            RaycastHit hit = RayCastManager.rayCastBlock(actiondistance);
            if (hit.point != Vector3.zero)
            {

                Chunk c = hit.transform.gameObject.GetComponent<Chunk>();
                Vector3 pos = substractVector3(hit.point, c.pos);

                //The viewdirection has an influence on the correct blockselecting
                Vector3 direction = World.currentWorld.playerTransform.TransformDirection(Vector3.forward);
                if (direction.x < 0)
                    pos.x -= 0.1f;
                if (direction.y < 0)
                    pos.y -= 0.1f;
                if (direction.z < 0)
                    pos.z -= 0.1f;
                c.destroyBlock(pos);
            }
        }
    }

    public static Vector3 substractVector3(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
}