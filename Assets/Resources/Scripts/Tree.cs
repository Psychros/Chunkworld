using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour {

    public static int[,,] tree;
    public static int treeHalfSize = 3;


    //Generate the Treearray
    public static void generateTreeArray()
    {
        tree = new int[7, 11, 7];

        //Leaves bottom
        for (int k = 2; k < 5; k++)
        {
            for (int j = 2; j < 5; j++)
            {
                tree[k, 5, j] = (int)BlockType.Leaves;
                tree[k, 10, j] = (int)BlockType.Leaves;
            }
        }
        for (int k = 1; k < 6; k++)
        {
            for (int j = 1; j < 6; j++)
            {
                tree[k, 6, j] = (int)BlockType.Leaves;
                tree[k, 9, j] = (int)BlockType.Leaves;
            }
        }
        for (int k = 0; k < 7; k++)
        {
            for (int j = 0; j < 7; j++)
            {
                tree[k, 7, j] = (int)BlockType.Leaves;
                tree[k, 8, j] = (int)BlockType.Leaves;
            }
        }

        //Wood
        for (int j = 0; j < 9; j++)
        {
            tree[3, j, 3] = (int)BlockType.Wood;
        }
    }
}
