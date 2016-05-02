using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager{

    public static string pathWorld = Application.dataPath + "/World/";
    public static string fileTypeWorld = ".world";

    //Creates a file and saves the strings
    public static void writeFile(string file, params string[] data)
    {
        if (!Directory.Exists(pathWorld))
            Directory.CreateDirectory(pathWorld);


            StreamWriter writer = new StreamWriter(pathWorld + file + fileTypeWorld);

        for (int i = 0; i < data.Length; i++)
        {
            writer.WriteLine(data[i]);
        }

        writer.Flush();
        writer.Close();
    }


    public static void writeArray(string file, ref BlockType[,,] array)
    {
        if (!Directory.Exists(pathWorld))
            Directory.CreateDirectory(pathWorld);


        StreamWriter writer = new StreamWriter(pathWorld + file + fileTypeWorld);

        int l1 = array.GetLength(0);
        int l2 = array.GetLength(1);
        int l3 = array.GetLength(2);
        for (int x = 0; x < l1; x++)
        {
            for (int y = 0; y < l2; y++)
            {
                for (int z = 0; z < l3; z++)
                {
                    writer.Write((int)array[x, y, z] + " ");
                }
                writer.WriteLine();
            }
        }

        writer.Flush();
        writer.Close();
    }

    //Reads all lines of a file
    public static List<string> readFile(string file)
    {
        StreamReader reader = new StreamReader(pathWorld + file + fileTypeWorld);

        List<string> list = new List<string>();
        while (!reader.EndOfStream)
            list.Add(reader.ReadLine());

        reader.Close();

        return list;
    }

    public static bool fileExists(string file)
    {
        return File.Exists(pathWorld + file + fileTypeWorld);
    }
}
