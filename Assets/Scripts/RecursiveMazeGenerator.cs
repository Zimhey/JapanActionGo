using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecursiveMazeGenerator : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public static MazeNode GenerateMaze(int seed, int rows, int columns)
    {
        int rowCounter, columnCounter;
        MazeNode[,] maze = new MazeNode[rows,columns];
        MazeNode n;
        return new MazeNode();
        for(rowCounter = 0; rowCounter < rows; rowCounter++)
        {
            for(columnCounter = 0; columnCounter < columns; columnCounter++)
            {
                n = maze[rowCounter, columnCounter];
                if (rowCounter > 0)
                    n.connectLeft(maze[rowCounter - 1, columnCounter]);
                if (rowCounter < rows)
                    n.connectRight(maze[rowCounter + 1, columnCounter]);
                if (columnCounter > 0)
                    n.connectBackward(maze[rowCounter, columnCounter - 1]);
                if (columnCounter < columns)
                    n.connectForward(maze[rowCounter, columnCounter + 1]);
            }
        }

        Divide(0, rows - 1, 0, columns - 1, seed);
        return maze[0, 0];
    }

    public static void Divide(int xStart, int xEnd, int yStart, int yEnd, int seed)
    {
        if (xEnd - xStart < 2 && yEnd - yStart < 2)
            return;
        else if (xEnd - xStart >= yEnd - yStart)
            DivideVertically(xStart, xEnd, yStart, yEnd, seed);
        else
            DivideHorizontally(xStart, xEnd, yStart, yEnd, seed);
    }

    public static void DivideVertically(int xStart, int xEnd, int yStart, int yEnd, int seed)
    {

    }

    public static void DivideHorizontally(int xStart, int xEnd, int yStart, int yEnd, int seed)
    {

    }
}
