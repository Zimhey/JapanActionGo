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

    public static MazeNode GenerateMaze(int seed, int columns, int rows)
    {
        int rowCounter, columnCounter;
        MazeNode[,] maze = new MazeNode[column,rows];
        MazeNode n;

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                maze[i, j] = new MazeNode(i, j);
            }
        }

        for (columnCounter = 0; columnCounter < columns; columnCounter++)
        {
            for(rowCounter = 0; rowCounter < rows; rowCounter++)
            {
                n = maze[columnCounter, rowCounter];
                if (columnCounter > 0)
                    n.connectLeft(maze[columnCounter - 1, rowCounter]);
                if (columnCounter < columns - 1)
                    n.connectRight(maze[columnCounter + 1, rowCounter]);
                if (rowCounter > 0)
                    n.connectForward(maze[columnCounter, rowCounter - 1]);
                if (rowCounter < rows - 1)
                    n.connectBackward(maze[columnCounter, rowCounter + 1]);
            }
        }

        Divide(0, columns - 1, 0, rows - 1, seed, maze);
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
        int randLine = rand.Next(xStart, xEnd) + 1;
        print(randLine);
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
        int randLine = rand.Next(yStart, yEnd) + 1;
        print(randLine);
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
