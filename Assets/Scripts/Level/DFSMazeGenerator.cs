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

    public static MazeNode GenerateMaze(int seed, int columns, int rows, int floor)
    {
        MazeNode[,] maze = new MazeNode[columns, rows];
        int i, j;
        for(i = 0; i < columns; i++)
        {
            for(j = 0; j < rows; j++)
            {
                maze[i, j] = new MazeNode(i, j);
                maze[i, j].Floor = floor;
            }
        }

        int xCurrent = 0;
        int yCurrent = 0;
        int xNext = 0;
        int yNext = 0;
        Stack visited = new Stack();
        visited.Push(maze[0, 0]);
        Stack backtracked = new Stack();
        int spacesVisited = 1;
        System.Random rand = new System.Random(seed);
        while(spacesVisited < rows * columns)
        {
            while (xCurrent == columns - 1 && yCurrent == rows - 1)
            {
                MazeNode n = (MazeNode)visited.Pop();
                backtracked.Push(n);
                xCurrent = n.Row;
                yCurrent = n.Col;
            }
            int next;
            bool nextAvailable = false;
            bool leftAvailable = (xCurrent != 0 && !visited.Contains(maze[xCurrent - 1, yCurrent]) && !backtracked.Contains(maze[xCurrent - 1, yCurrent]));
            bool rightAvailable = (xCurrent != columns - 1 && !visited.Contains(maze[xCurrent + 1, yCurrent]) && !backtracked.Contains(maze[xCurrent + 1, yCurrent]));
            bool backwardAvailable = (yCurrent != 0 && !visited.Contains(maze[xCurrent, yCurrent - 1]) && !backtracked.Contains(maze[xCurrent, yCurrent - 1]));
            bool forwardAvailable = (yCurrent != rows - 1 && !visited.Contains(maze[xCurrent, yCurrent + 1]) && !backtracked.Contains(maze[xCurrent, yCurrent + 1]));
            bool noneAvailable = !leftAvailable && !rightAvailable && !forwardAvailable && !backwardAvailable;
            
            while(noneAvailable)
            {
                leftAvailable = (xCurrent != 0 && !visited.Contains(maze[xCurrent - 1, yCurrent]) && !backtracked.Contains(maze[xCurrent - 1, yCurrent]));
                rightAvailable = (xCurrent != columns - 1 && !visited.Contains(maze[xCurrent + 1, yCurrent]) && !backtracked.Contains(maze[xCurrent + 1, yCurrent]));
                backwardAvailable = (yCurrent != 0 && !visited.Contains(maze[xCurrent, yCurrent - 1]) && !backtracked.Contains(maze[xCurrent, yCurrent - 1]));
                forwardAvailable = (yCurrent != rows - 1 && !visited.Contains(maze[xCurrent, yCurrent + 1]) && !backtracked.Contains(maze[xCurrent, yCurrent + 1]));
                noneAvailable = !leftAvailable && !rightAvailable && !forwardAvailable && !backwardAvailable;
                if (!noneAvailable)
                    break;
                MazeNode n = (MazeNode) visited.Pop();
                backtracked.Push(n);
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
                        break;
                    }
                if (next == 2)
                    if (forwardAvailable)
                    {
                        xNext = xCurrent;
                        yNext = yCurrent + 1;
                        nextAvailable = true;
                        break;
                    }
                if (next == 3)
                    if (rightAvailable)
                    {
                        xNext = xCurrent + 1;
                        yNext = yCurrent;
                        nextAvailable = true;
                        break;
                    }
                if (next == 4)
                    if (backwardAvailable)
                    {
                        xNext = xCurrent;
                        yNext = yCurrent - 1;
                        nextAvailable = true;
                        break;
                    }
            }
            spacesVisited++;
            visited.Push(maze[xNext, yNext]);
            maze[xCurrent, yCurrent].AddEdge(maze[xNext, yNext]);
            xCurrent = xNext;
            yCurrent = yNext;
        }
        return maze[0, 0];
    }
}
