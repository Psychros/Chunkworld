using UnityEngine;
using System.Collections.Generic;
using SimplexNoise;
using System.Diagnostics;
using System.IO;
using System.Collections;

public class World : MonoBehaviour {

    [HideInInspector] public static World currentWorld;
    [HideInInspector] public List<Chunk> world;
    [HideInInspector] public int seed;
    public Material blockMaterial;
    public Transform playerTransform;
    public Transform playerTransformForDirection;       //Important for the cameradirection
    public GameObject lanternTransform;
    private static Chunk selectedChunk = null;

    //Important for chunkloading
    private static Vector3 currentChunkPos;
    private static System.Func<int, int, Vector3> createVector1 = (int a, int b) => new Vector3(a * Chunk.standardSize.x, 0, b * Chunk.standardSize.z);
    private static System.Func<int, int, Vector3> createVector2 = (int a, int b) => new Vector3(b * Chunk.standardSize.x, 0, a * Chunk.standardSize.z);

    //Timer
    private float timerPlayerPosition = 0;
    

    //Only for testing
    Stopwatch watch = new Stopwatch();
    Stopwatch watch2 = new Stopwatch();

    void Start () {
        watch.Start();
        //Make the cursor unvisible
        Cursor.visible = false;

        //Generate the directories
        SaveManager.generateAllDirectories();

        //Generate the structurearrays
        Tree.generateTreeArray();

        //Place the player:
        loadPlayerPosition();

        //Generate the startchunks
        generateWorld();
        watch.Stop();


        print("Time: " + watch.ElapsedMilliseconds);
        print("Time for loading: " + watch2.ElapsedMilliseconds);
    }
	


	void Update () {
        //Saves the playerposition every second
        if (timerPlayerPosition > 1)
        {
            timerPlayerPosition = 0;
            savePlayerPosition();

            //Load chunks if the player walks
            //testForChunkLoading();
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
                generateChunk(Chunk.roundChunkPos(playerTransform.position) + new Vector3((int)(x * Chunk.standardSize.x), 0, (int)(z * Chunk.standardSize.z)));
            }
        }

        //Calculate the mesh of the chunk
        foreach (Chunk chunk in world)
        {
            StartCoroutine(chunk.CreateMesh());
        }
    }



    public Chunk generateChunk(Vector3 pos)
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
        currentWorld.world.Add(chunk);


        //Generate the chunk if it doesnt exists or load it
        if (File.Exists(SaveManager.pathWorld + pos.ToString() + SaveManager.fileTypeWorld)){
            watch2.Start();
            chunk.loadChunk();
            //chunk.saveChunk();
            watch2.Stop();
        }
        else
        {
            chunk.generateArray();

            //Place Structures in the Chunk
            placeStructuresInChunk(ref chunk);

            //Save the chunk
            watch2.Start();
            chunk.saveChunk();
            watch2.Stop();
        }


        return chunk;
    }



    //Test if there is a structure in the chunk and place the part of the structure
    public void placeStructuresInChunk(ref Chunk chunk)
    {
        string file = SaveManager.pathStructures + chunk.pos.ToString();

        if (File.Exists(SaveManager.pathWorld + file + SaveManager.fileTypeWorld))
        {
            //Place all structures
            List<string> structures = SaveManager.readFileWorld(file);
            foreach (string s in structures)
            {
                string[] structure = s.Trim().Split(',', ' ');
                Vector3 pos = new Vector3(float.Parse(structure[0]), float.Parse(structure[1]), float.Parse(structure[2]));
                chunk.placeStructure(new Structure(pos, ref Tree.tree));
            }

            //Delete the file
            File.Delete(SaveManager.pathWorld + file + SaveManager.fileTypeWorld);
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
            foreach(Chunk c in currentWorld.world)
            {
                if (pos.Equals(c.pos))
                {
                    //Mark the found chunk as selected
                    selectedChunk = c;
                    return selectedChunk;
                }
            }
        }

        //No chunk was found
        return null;
    }



    public void loadPlayerPosition()
    {
        if(File.Exists(SaveManager.pathWorld + SaveManager.filePlayer + SaveManager.fileTypeWorld)){
            string[] playerCoords = SaveManager.readFileWorld(SaveManager.filePlayer).ToArray();
            Vector3 playerPos = new Vector3(float.Parse(playerCoords[0]), float.Parse(playerCoords[1]), float.Parse(playerCoords[2]));
            playerTransform.position = playerPos;
        }
    }

    public void savePlayerPosition()
    {
        SaveManager.writeFileWorld(SaveManager.filePlayer, playerTransform.position.x.ToString(), playerTransform.position.y.ToString(), playerTransform.position.z.ToString());
    }

    public void addStructure(Structure s)
    {
        foreach (Vector3 pos in s.chunks.ToArray())
            SaveManager.addLine(SaveManager.pathStructures + pos.ToString(), SaveManager.fileTypeWorld, s.pos.x + "," + s.pos.y + "," + s.pos.z + " Tree");
    }


    //Loads the chunks if the player walks
    public void testForChunkLoading()
    {
        Vector3 posPlayer = Chunk.roundChunkPos(playerTransform.position);

        //Load/Generate chunks
        if (!Vector3.Equals(posPlayer, currentChunkPos))
        {
            //x-direction
            if (posPlayer.x > currentChunkPos.x)
            {
                StartCoroutine(loadChunks(posPlayer, true, createVector1));
            }
            else if (posPlayer.x < currentChunkPos.x)
            {
                StartCoroutine(loadChunks(posPlayer, false, createVector1));
            }

            //Z-direction
            if (posPlayer.z > currentChunkPos.z)
            {
                StartCoroutine(loadChunks(posPlayer, true, createVector2));
            }
            else if (posPlayer.x < currentChunkPos.x)
            {
                StartCoroutine(loadChunks(posPlayer, false, createVector2));
            }

            //Actualize the currentChunkPos
            currentChunkPos = posPlayer;
        }
    }

    //Is needed fore compact code
    private IEnumerator loadChunks(Vector3 posPlayer, bool isPositive, System.Func<int, int, Vector3> func)
    {
        Chunk[] chunks = new Chunk[22];

        int a = isPositive ? 5 : -5;
        int b = isPositive ? a - 1 : a + 1;
        int a2 = isPositive ? -5 : 5;

        //Load the chunks
        for (int i = -5; i <= 5; i++)
        {
            Chunk c = generateChunk(posPlayer + func(a, i));
            chunks[i + 5] = c;
            Chunk d = findChunk(posPlayer + func(b, i));
            chunks[i + 16] = d;

            //Remove the old chunks
            Chunk e = findChunk(posPlayer + func(a2, i));
            if (e != null)
            {
                world.Remove(e);
                Destroy(e.gameObject);
            } else
            {
                print((posPlayer + func(a2, i)));
            }
        }

        //Create the meshs
        foreach (Chunk c in chunks)
            if (c != null)
            {
                StartCoroutine(c.CreateMesh());
                yield return null;
            }    

        yield return 0;
    }
}
