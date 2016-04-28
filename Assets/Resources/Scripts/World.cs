using UnityEngine;
using System.Collections.Generic;
using SimplexNoise;

public class World : MonoBehaviour {

    [HideInInspector] public static World currentWorld;
    [HideInInspector] public List<Chunk> world;
    [HideInInspector] public int seed;
    [HideInInspector] public List<Structure> structures = new List<Structure>();
    public Material blockMaterial;
    public Transform playerTransform;
    private static Chunk selectedChunk = null;


	void Start () {
        //Make the cursor unvisible
        Cursor.visible = false;

        currentWorld = this;
        seed = Random.Range(0, int.MaxValue);
        Noise.generateArray();

        world = new List<Chunk>();

        //Generate startchunks
        for (int x = -3; x <= 3; x++)
        {
            for (int z = -3; z <= 3; z++)
            {
                generateChunk(new Vector3((int)(x*Chunk.standardSize.x), 0, (int)(z * Chunk.standardSize.z)));
            }
        }

        //Calculate the mesh of the chunk
        foreach (Chunk chunk in world) {
            StartCoroutine(chunk.CreateMesh());
        }
    }
	
	void Update () {
	
	}




    public void generateChunk(Vector3 pos)
    {
        GameObject g = new GameObject();
        g.AddComponent<MeshRenderer>();
        g.AddComponent<MeshFilter>();
        g.AddComponent<MeshCollider>();

        //Create a chunk
        Chunk chunk = g.AddComponent<Chunk>();
        chunk.size = Chunk.standardSize;
        chunk.pos = pos;
        chunk.generateArray();
        chunk.gameObject.GetComponent<MeshRenderer>().material = blockMaterial;

        //Add the chunk to the world
        Instantiate(g);

        world.Add(chunk);

        //Place Structures in the Chunk
        placeStructuresInChunk(chunk);
    }



    //Test if there is a structure in the chunk and place the part of the structure
    public void placeStructuresInChunk(Chunk chunk)
    {
        //The function ToArray must be called because of an Exception
        foreach (Structure str in structures.ToArray())
        {
            foreach (Vector3 cPos in str.chunks.ToArray())
            {
                //If the structure is in the chunk
                if (Vector3.Distance(cPos, chunk.pos) < 1f)
                {
                    //Place the structure in the chunk
                    chunk.placeStructure(str);

                    //Remove the chunk from the structure 
                    str.chunks.Remove(cPos);

                    //Test if the structure is completed 
                    if (str.chunks.Count == 0)
                        structures.Remove(str);
                }
            }
        }
    }



    public static Chunk findChunk(Vector3 pos)
    {
        //Test if the searched chunk is selected
        if (selectedChunk != null && pos.Equals(selectedChunk.pos))
        {
            return selectedChunk;
        }
        else
        {
            //Search the chunk
            for (int a = 0; a < currentWorld.world.Count; a++)
            {
                //Posizion of the chunk
                Vector3 cpos = currentWorld.world[a].pos;

                if (pos.Equals(cpos))
                {
                    //Mark the found chunk as selected
                    selectedChunk = currentWorld.world[a];
                    return selectedChunk;
                }
            }
        }

        //No chunk was found
        return null;

    }
}
