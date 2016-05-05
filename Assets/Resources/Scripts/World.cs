using UnityEngine;
using System.Collections.Generic;
using SimplexNoise;
using System.Diagnostics;

public class World : MonoBehaviour {

    [HideInInspector] public static World currentWorld;
    [HideInInspector] public List<Chunk> world;
    [HideInInspector] public int seed;
    [HideInInspector] public List<Structure> structures = new List<Structure>();
    public Material blockMaterial;
    public Transform playerTransform;
    public Transform playerTransformForDirection;       //Important for the cameradirection
    public GameObject lanternTransform;
    private static Chunk selectedChunk = null;

    //Timer
    private float timerPlayerPosition = 0;
    

    //Only for testing
    Stopwatch watch = new Stopwatch();
    Stopwatch watch2 = new Stopwatch();

    void Start () {
        watch.Start();
        //Make the cursor unvisible
        Cursor.visible = false;

        //Generate the structurearrays
        Tree.generateTreeArray();

        //Generate the startchunks
        generateWorld();
        watch.Stop();

        //Place the player:
        loadPlayerPosition();


        print("Time: " + watch.ElapsedMilliseconds);
        print("Time for loading: " + watch2.ElapsedMilliseconds);
    }
	


	void Update () {
        //Saves the playerposition every second
        if (timerPlayerPosition > 1)
        {
            timerPlayerPosition = 0;
            savePlayerPosition();
        }
        timerPlayerPosition += Time.deltaTime;
    }



    public void generateWorld()
    {
        currentWorld = this;
        seed = Random.Range(0, int.MaxValue);
        Random.seed = seed;
        Noise.generateArray();

        world = new List<Chunk>();

        //Generate startchunks
        for (int x = -5; x <= 5; x++)
        {
            for (int z = -5; z <= 5; z++)
            {
                generateChunk(new Vector3((int)(x * Chunk.standardSize.x), 0, (int)(z * Chunk.standardSize.z)));
            }
        }

        //Calculate the mesh of the chunk
        foreach (Chunk chunk in world)
        {
            StartCoroutine(chunk.CreateMesh());
        }
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
        chunk.gameObject.GetComponent<MeshRenderer>().material = blockMaterial;

        //Add the chunk to the world
        Instantiate(g);
        world.Add(chunk);


        //Generate the chunk if it doesnt exists or load it
        if (SaveManager.fileExists(pos.ToString())){
            watch2.Start();
            StartCoroutine(chunk.loadChunk());
            watch2.Stop();
        }
        else
        {
            chunk.generateArray();

            //Place Structures in the Chunk
            placeStructuresInChunk(ref chunk);

            //Save the chunk
            watch2.Start();
            StartCoroutine(chunk.saveChunk());
            watch2.Stop();
        }
    }



    //Test if there is a structure in the chunk and place the part of the structure
    public void placeStructuresInChunk(ref Chunk chunk)
    {
        //The function ToArray must be called because of an Exception
        //Structure[] s = structures.ToArray();
        foreach (Structure str in structures.ToArray())
        {
            //Vector3[] c = str.chunks.ToArray();
            foreach (Vector3 cPos in str.chunks.ToArray())
            {
                //If the structure is in the chunk
                float distance = World.distance(cPos, chunk.pos);
                if (distance < 1f)
                {
                    //Place the structure in the chunk
                    //watch2.Start();
                    chunk.placeStructure(str);
                    //watch2.Stop();

                    //Remove the chunk from the structure 
                    str.chunks.Remove(cPos);

                    //Test if the structure is completed 
                    if (str.chunks.Count == 0)
                        structures.Remove(str);
                }
            }
        }
    }

    public static float distance(Vector3 a, Vector3 b)
    {
        float x = b.x - a.x;
        float y = b.y - a.y;
        float z = b.z - a.z;

        return Mathf.Sqrt(x*x + y*y + z*z);
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



    public void loadPlayerPosition()
    {
        if(SaveManager.fileExists(SaveManager.filePlayer)){
            string[] playerCoords = SaveManager.readFile(SaveManager.filePlayer).ToArray();
            Vector3 playerPos = new Vector3(float.Parse(playerCoords[0]), float.Parse(playerCoords[1]), float.Parse(playerCoords[2]));
            playerTransform.position = playerPos;
        }
    }

    public void savePlayerPosition()
    {
        SaveManager.writeFile(SaveManager.filePlayer, playerTransform.position.x.ToString(), playerTransform.position.y.ToString(), playerTransform.position.z.ToString());
    }
}
