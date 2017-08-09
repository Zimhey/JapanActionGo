using System.Collections;
using System.Collections.Generic;
using System;
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

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                maze[i, j] = new MazeNode(i, j);
            }
        }

        for (rowCounter = 0; rowCounter < rows; rowCounter++)
        {
            for(columnCounter = 0; columnCounter < columns; columnCounter++)
            {
                n = maze[rowCounter, columnCounter];
                if (rowCounter > 0)
                    n.connectLeft(maze[rowCounter - 1, columnCounter]);
                if (rowCounter < rows)
                    n.connectRight(maze[rowCounter + 1, columnCounter]);
                if (columnCounter > 0)
                    n.connectBackward(maze[rowCounter, columnCounter + 1]);
                if (columnCounter < columns)
                    n.connectForward(maze[rowCounter, columnCounter - 1]);
            }
        }

        Divide(0, rows - 1, 0, columns - 1, seed, maze);
        return maze[0, 0];
    }

    public static void Divide(int xStart, int xEnd, int yStart, int yEnd, int seed, MazeNode[,] maze)
    {
        if (xEnd - xStart < 2 && yEnd - yStart < 2)
            return;
        else if (xEnd - xStart >= yEnd - yStart)
            DivideVertically(xStart, xEnd, yStart, yEnd, seed, maze);
        else
            DivideHorizontally(xStart, xEnd, yStart, yEnd, seed, maze);
    }

    public static void DivideVertically(int xStart, int xEnd, int yStart, int yEnd, int seed, MazeNode[,] maze)
    {
        System.Random rand = new System.Random();
        int randLine = rand.Next(xStart, xEnd);
        int randHole = rand.Next(yStart, yEnd);
        int counter;
        for(counter = yStart; counter < yEnd; counter++)
        {
            if(counter != randHole)
            {
                maze[randLine - 1, counter].DisconnectRight();
                maze[randLine, counter].DisconnectLeft();
            }
        }
        Divide(xStart, randLine, yStart, yEnd, seed, maze);
        Divide(randLine, xEnd, yStart, yEnd, seed, maze);
    }

    public static void DivideHorizontally(int xStart, int xEnd, int yStart, int yEnd, int seed, MazeNode[,] maze)
    {
        System.Random rand = new System.Random();
        int randLine = rand.Next(yStart, yEnd);
        int randHole = rand.Next(xStart, xEnd);
        int counter;
        for(counter = xStart; counter < xEnd; counter++)
        {
            if(counter != randHole)
            {
                maze[counter, randLine - 1].DisconnectBackward();
                maze[counter, randLine].DisconnectForward();
            }
        }
        Divide(xStart, xEnd, yStart, randLine, seed, maze);
        Divide(xStart, xEnd, randLine, yEnd, seed, maze);
    }
}
