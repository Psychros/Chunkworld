using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager{

    public static string pathWorld = Application.dataPath + "/World/";
    public static string pathStructures = "Structures/";
    public static string fileTypeWorld = ".world";

    public static string filePlayer = "Player";

    //Creates a file and saves the strings
    public static void writeFileWorld(string file, params string[] data)
    {
        writeFile(file, fileTypeWorld, data);
    }

    //Creates a file and saves the strings
    public static void writeFile(string file, string fileExtension, params string[] data)
    {
        if (!Directory.Exists(pathWorld))
            Directory.CreateDirectory(pathWorld);

        using (StreamWriter writer = new StreamWriter(pathWorld + file + fileExtension))
        {
            for (int i = 0; i < data.Length; i++)
            {
                writer.WriteLine(data[i]);
            }
        }
    }



    //Writes a chunkarray into a textfile
    public static void writeArray(string file, ref BlockType[,,] array)
    {
        if (!Directory.Exists(pathWorld))
            Directory.CreateDirectory(pathWorld);


        using (StreamWriter writer = new StreamWriter(pathWorld + file + fileTypeWorld))
        {
            int number = 0;
            int id = (int)BlockType.Air;

            int l1 = array.GetLength(0);
            int l2 = array.GetLength(1);
            int l3 = array.GetLength(2);
            for (int x = 0; x < l1; x++)
            {
                //Reset the blockdata
                id = -1;
                number = 0;

                for (int y = 0; y < l2; y++)
                {
                    for (int z = 0; z < l3; z++)
                    {
                        //Makes the chunks compact that the loading time is short
                        int id2 = (int)array[x, y, z];
                        if (id2 == id)
                            number++;
                        else
                        {
                            //Is this the first block in this row?
                            if(id < 0)
                            {
                                id = id2;
                                number++;
                            }
                            else
                            {
                                writeBlocks(writer, number, id);

                                //Uses the new id
                                id = id2;
                                number = 1;
                            }
                        }
                    }
                }

                //Writes the rest of the blocks
                writeBlocks(writer, number, id);
                writer.WriteLine();
            }
        }
    }



    //sums up the blocks with the same id
    private static void writeBlocks(StreamWriter writer, int number, int id)
    {
        if (number > 1)
            writer.Write(id + "x" + number + " ");
        else
            writer.Write(id + " ");
    }



    //Adds the text in a new line to the file
    public static void addLine(string file, string fileExtension, string text)
    {
        File.AppendAllText(pathWorld + file + fileExtension, text + System.Environment.NewLine);
    }


    //Reads all lines of a file
    public static List<string> readFileWorld(string file)
    {
        return readFile(file, fileTypeWorld);
    }

    //Reads all lines of a file
    public static List<string> readFile(string file, string fileExtension)
    {
        List<string> list = new List<string>();

        using (StreamReader reader = new StreamReader(pathWorld + file + fileExtension))
        {
            while (!reader.EndOfStream)
                list.Add(reader.ReadLine());
        }
        return list;
    }




    public static bool fileExistsWorld(string file)
    {
        return fileExists(pathWorld + file, fileTypeWorld);
    }

    public static bool fileExists(string file, string fileExtension)
    {
        return File.Exists(pathWorld + file + fileExtension);
    }


    //Generates all Folders
    public static void generateAllDirectories()
    {
        generateDirectory(pathWorld);
        generateDirectory(pathWorld + pathStructures);

    }

    public static void generateDirectory(string file)
    {
        if (!Directory.Exists(file))
            Directory.CreateDirectory(file);
    }
}
