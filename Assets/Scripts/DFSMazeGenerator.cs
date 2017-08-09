using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFSMazeGenerator : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public static MazeNode GenerateMaze(int rows, int columns, int seed)
    {
        MazeNode[,] maze = new MazeNode[rows, columns];
        int i, j;
        for(i = 0; i < rows; i++)
        {
            for(j = 0; j < rows; j++)
            {
                maze[i, j].Row = i;
                maze[i, j].Col = j;
            }
        }

        int xCurrent = 0;
        int yCurrent = 0;
        int xNext = 0;
        int yNext = 0;
        Stack visited = new Stack();
        int spacesVisited = 0;
        System.Random rand = new System.Random();
        while(spacesVisited < rows * columns)
        {
            int next;
            bool nextAvailable = false;
            bool leftAvailable = (xCurrent != 0 && !visited.Contains(maze[xCurrent - 1, yCurrent]));
            bool rightAvailable = (xCurrent != rows && !visited.Contains(maze[xCurrent + 1, yCurrent]));
            bool forwardAvailable = (yCurrent != 0 && !visited.Contains(maze[xCurrent, yCurrent - 1]));
            bool backwardAvailable = (yCurrent != columns && !visited.Contains(maze[xCurrent, yCurrent + 1]));
            bool noneAvailable = !leftAvailable && !rightAvailable && !forwardAvailable && !backwardAvailable;
            while(noneAvailable)
            {
                leftAvailable = (xCurrent != 0 && !visited.Contains(maze[xCurrent - 1, yCurrent]));
                rightAvailable = (xCurrent != rows && !visited.Contains(maze[xCurrent + 1, yCurrent]));
                forwardAvailable = (yCurrent != 0 && !visited.Contains(maze[xCurrent, yCurrent - 1]));
                backwardAvailable = (yCurrent != columns && !visited.Contains(maze[xCurrent, yCurrent + 1]));
                noneAvailable = !leftAvailable && !rightAvailable && !forwardAvailable && !backwardAvailable;
                MazeNode n = (MazeNode) visited.Pop();
                xCurrent = n.Row;
                yCurrent = n.Col;
            }
            while(!nextAvailable)
            {
                next = rand.Next(1, 5);
                if (next == 1)
                    if (leftAvailable)
                    {
                        xNext = xCurrent - 1;
                        yNext = yCurrent;
                        nextAvailable = true;
                    }
                if (next == 2)
                    if (forwardAvailable)
                    {
                        xNext = xCurrent;
                        yNext = yCurrent - 1;
                        nextAvailable = true;
                    }
                if (next == 3)
                    if (rightAvailable)
                    {
                        xNext = xCurrent + 1;
                        yNext = yCurrent;
                        nextAvailable = true;
                    }
                if (next == 4)
                    if (backwardAvailable)
                    {
                        xNext = xCurrent;
                        yNext = yCurrent + 1;
                        nextAvailable = true;
                    }
            }
            spacesVisited++;
            visited.Push(maze[xNext, yNext]);
            xCurrent = xNext;
            yCurrent = yNext;
        }
        return maze[0, 0];
    }
}
