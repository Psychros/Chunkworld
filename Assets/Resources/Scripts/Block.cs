using UnityEngine;
using System.Collections;

public class Block{

    public static Block[] blockData =
    {
        new Block(-1, -1, true, false),                      //Air
        new Block(0, 1),                                     //Lavastone
        new Block(0, 0),                                     //Stone
        new Block(1, 0),                                     //Dirt
        new Block(2, 0, 2, 0, 3, 0, 3, 0, 2, 0, 2, 0),       //Wood
        new Block(4, 0, true, false),                        //Leaves
        new Block(6, 0, 6, 0, 1, 0, 5, 0, 6, 0, 6, 0),       //Grass
        new Block(7, 0, true, false),                        //Glass
        new Block(8, 0),                                     //Planks
        new Block(1, 1, true, true),                         //Water
        new Block(9, 0),                                     //Sand
    };

    //UV-Coordinates for every Face
    public float xLeft, yLeft, 
               xRight, yRight, 
               xBottom, yBottom, 
               xTop, yTop, 
               xFront, yFront, 
               xBack, yBack;
    public bool isTransparent = false,
                isLiquid = false;

    //Creates a block with the Textur at x/y
	public Block(float x, float y) : this(x, y, x, y, x, y, x, y, x, y, x, y)
    {

    }

    //Creates a block with the Textur at x/y
    public Block(float x, float y, bool isTransparent, bool isLiquid) : this(x, y, x, y, x, y, x, y, x, y, x, y)
    {
        this.isTransparent = isTransparent;
        this.isLiquid = isLiquid;
    }

    public Block(float xLeft, float yLeft, float xRight, float yRight, float xBottom, float yBottom, float xTop, float yTop, float xFront, float yFront, float xBack, float yBack)
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

    public Block(int xLeft, int yLeft, int xRight, int yRight, int xBottom, int yBottom, int xTop, int yTop, int xFront, int yFront, int xBack, int yBack, bool isTransparent, bool isLiquid)
         : this(xLeft, yLeft, xRight, yRight, xBottom, yBottom, xTop, yTop, xFront, yFront, xBack, yBack)
    {
        this.isTransparent = isTransparent;
        this.isLiquid = isLiquid;
    }
}
