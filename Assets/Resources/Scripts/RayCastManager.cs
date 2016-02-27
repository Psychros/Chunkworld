using UnityEngine;
using System.Collections;

public class RayCastManager : MonoBehaviour {

	//Test for something in front of the player with a raycast 
	public static RaycastHit startRayCast(float distance){
		RaycastHit hit;
		Vector3 direction = World.currentWorld.playerTransform.TransformDirection(Vector3.forward);
		Vector3 position = World.currentWorld.playerTransform.position;
		Physics.Raycast(position, direction, out hit, distance);

		return hit;
	}

	//Test for terrain in front of the player with a raycast 
	public static RaycastHit[] startRayCastAllHits(float distance){
		Vector3 direction = World.currentWorld.playerTransform.TransformDirection(Vector3.forward);
		Vector3 position = World.currentWorld.playerTransform.position;

		return Physics.RaycastAll(position, direction, distance);;
	}

	//returns a RaycastHit with a block
	public static RaycastHit rayCastBlock(float distance){
		RaycastHit[] hits = startRayCastAllHits(distance);
		
		foreach(RaycastHit hit in hits){
            Chunk c = hit.transform.gameObject.GetComponent<Chunk>();
			if(c != null){
                return hit;
			}
		}

		return new RaycastHit();
	}
}
