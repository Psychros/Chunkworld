using UnityEngine;
using System.Collections;

public class Block{

    public static Block[] blockData =
    {
        new Block(-1f, -1f, true),                        //Air
        new Block(0, 0),                                //Stone
        new Block(1f, 0),                                //Dirt
        new Block(2f, 0, 2f, 0, 3f, 0, 3f, 0, 2f, 0, 2f, 0),  //Wood
        new Block(4f, 0, true),                          //Leaves
        new Block(6f, 0, 6f, 0, 1f, 0, 5f, 0, 6f, 0, 6f, 0),  //Grass
        new Block(7f, 0, true),                          //Glass
    };

    //UV-Coordinates for every Face
    public float xLeft, yLeft, 
               xRight, yRight, 
               xBottom, yBottom, 
               xTop, yTop, 
               xFront, yFront, 
               xBack, yBack;
    public bool isTransparent = false;

    //Creates a block with the Textur at x/y
	public Block(float x, float y) : this(x, y, x, y, x, y, x, y, x, y, x, y)
    {

    }

    //Creates a block with the Textur at x/y
    public Block(float x, float y, bool isTransparent) : this(x, y, x, y, x, y, x, y, x, y, x, y)
    {
        this.isTransparent = isTransparent;
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

    public Block(int xLeft, int yLeft, int xRight, int yRight, int xBottom, int yBottom, int xTop, int yTop, int xFront, int yFront, int xBack, int yBack, bool isTransparent)
         : this(xLeft, yLeft, xRight, yRight, xBottom, yBottom, xTop, yTop, xFront, yFront, xBack, yBack)
    {
        this.isTransparent = isTransparent;
    }
}
