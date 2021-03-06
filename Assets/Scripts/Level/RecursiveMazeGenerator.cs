﻿using System.Collections;
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
        MazeNode[,] maze = new MazeNode[columns,rows];
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
                {
                    n.AddEdge(maze[columnCounter - 1, rowCounter]);
                    //maze[columnCounter - 1, rowCounter].connectForward(n);
                }
                if (columnCounter < columns - 1)
                {
                    n.AddEdge(maze[columnCounter + 1, rowCounter]);
                    //maze[columnCounter + 1, rowCounter].connectBackward(n);
                }
                if (rowCounter > 0)
                {
                    n.AddEdge(maze[columnCounter, rowCounter - 1]);
                    //maze[columnCounter, rowCounter - 1].connectRight(n);
                }
                if (rowCounter < rows - 1)
                {
                    n.AddEdge(maze[columnCounter, rowCounter + 1]);
                    //maze[columnCounter, rowCounter + 1].connectLeft(n);
                }
            }
        }

        Divide(0, columns - 1, 0, rows - 1, seed, maze); // 0, 4, 0, 4
        return maze[0, 0];
    }

    public static void Divide(int xStart, int xEnd, int yStart, int yEnd, int seed, MazeNode[,] maze)
    {
        if (xEnd - xStart < 1 && yEnd - yStart < 1)
        {
            //print(xStart + " " + xEnd + " " + yStart + " " + yEnd);
            return;
        }
        else if (xEnd - xStart >= yEnd - yStart)
            DivideVertically(xStart, xEnd, yStart, yEnd, seed, maze);
        else
            DivideHorizontally(xStart, xEnd, yStart, yEnd, seed, maze);
    }

    public static void DivideVertically(int xStart, int xEnd, int yStart, int yEnd, int seed, MazeNode[,] maze)
    {
        // xStart = 0, xEnd = 4, yStart = 0, yEnd = 4
        System.Random rand = new System.Random();
        int randLine = rand.Next(xStart, xEnd) + 1; // returns between 1 and 4 on first call
        print(randLine);
        int randHole = rand.Next(yStart, yEnd + 1); // returns between 0 and 3
        int counter;
        for(counter = yStart; counter <= yEnd; counter++) // 0, 1, 2, 3, 4
        {
            if(counter != randHole)
            {
                maze[randLine - 1, counter].RemoveEdge(maze[randLine, counter]);
            }
        }
        //print(randLine + " " + randHole);
        //return;
        if (xEnd - xStart == 1)
        {
            Divide(xStart, xStart, yStart, yEnd, seed, maze);
        }
        else
        {
            Divide(xStart, randLine - 1, yStart, yEnd, seed, maze);
            Divide(randLine, xEnd, yStart, yEnd, seed, maze);
        }
    }

    public static void DivideHorizontally(int xStart, int xEnd, int yStart, int yEnd, int seed, MazeNode[,] maze)
    {
        System.Random rand = new System.Random();
        int randLine = rand.Next(yStart, yEnd) + 1;
        print(randLine);
        int randHole = rand.Next(xStart, xEnd + 1);
        int counter;
        for(counter = xStart; counter <= xEnd; counter++)
        {
            if(counter != randHole)
            {
                maze[counter, randLine - 1].RemoveEdge(maze[counter, randLine]);
            }
        }
        //print(randLine + " " + randHole);
        //return;
        if (yEnd - yStart == 1)
        {
            Divide(xStart, xEnd, yStart, yStart, seed, maze);
        }
        else
        {
            Divide(xStart, xEnd, yStart, randLine - 1, seed, maze);
            Divide(xStart, xEnd, randLine, yEnd, seed, maze);
        }
    }
}
