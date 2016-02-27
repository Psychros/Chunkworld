using UnityEngine;
using System.Collections;

public class Block{

    public static Block[] blockData =
    {
        null,
        new Block(0, 0),                                //Stone
        new Block(1, 0),                                //Dirt
        new Block(2, 0, 2, 0, 3, 0, 3, 0, 2, 0, 2, 0),  //Wood
        new Block(4, 0),                                //Leaves
        new Block(6, 0, 6, 0, 1, 0, 5, 0, 6, 0, 6, 0),  //Grass
    };

    //UV-Coordinates for every Face
    public int xLeft, yLeft, 
               xRight, yRight, 
               xBottom, yBottom, 
               xTop, yTop, 
               xFront, yFront, 
               xBack, yBack;

    //Creates a block with the Textur at x/y
	public Block(int x, int y) : this(x, y, x, y, x, y, x, y, x, y, x, y)
    {

    }

    public Block(int xLeft, int yLeft, int xRight, int yRight, int xBottom, int yBottom, int xTop, int yTop, int xFront, int yFront, int xBack, int yBack)
    {
        this.xLeft = xLeft;
        this.yLeft = yLeft;

        this.xRight = xRight;
        this.yRight = yRight;

        this.xBottom = xBottom;
        this.yBottom = yBottom;

        this.xTop = xTop;
        this.yTop = yTop;

        this.xFront = xFront;
        this.yFront = yFront;

        this.xBack = xBack;
        this.yBack = yBack;
    }
}
