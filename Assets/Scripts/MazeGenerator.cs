using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    public int Seed;

    // Use this for initialization
    void Start()
    {
        int size = 5;
        MazeNode root = RecursiveMazeGenerator.GenerateMaze(0, size, size);
        SpawnMaze(root, size);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public MazeNode GenTestMaze(int size)
    {
        MazeNode[,] maze = new MazeNode[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                maze[i, j] = new MazeNode(i, j);
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if(j < size - 1)
                    maze[i, j].AddEdge(maze[i, j + 1]);
                if(i < size - 1)
                    maze[i, j].AddEdge(maze[i + 1, j]);
            }
        }

        return maze[0, 0];
    }

    public void GenerateMaze(int seed, int size)
    {
        // TODO 
        // Generate a Perfect Maze calling the static functions in other classes
        // Save root MazeNode
        // Get path to exit
        // Set exit path
        // Generate sections
        // Generate loops for each section
        // Spawn each maze piece for each section

    }

    public static LinkedList<MazeNode> GetPath(MazeNode start, MazeNode end)
    {
        LinkedList<MazeNode> path = new LinkedList<MazeNode>();
        return path;
        
    }

    public static void SetAsExitPath(LinkedList<MazeNode> path)
    {
        // TODO Set the ExitNode flag to true for each MazeNode
    }

    public static List<MazeNode> GenerateSections(MazeNode root, int sections, int rows, int cols)
    {
        List<MazeNode> sectionRoots = new List<MazeNode>();
        // TODO Generate Sections

        return sectionRoots;
    }

    public static void GenerateLoops(MazeNode root, int loops)
    {
        // TODO Generate Loops
    }

    public void SpawnMaze(MazeNode root, int size)
    {
        // TODO use DFS to search through maze and spawn each piece
        bool[,] visited = new bool[size, size];

        Stack<MazeNode> stack = new Stack<MazeNode>();

        stack.Push(root);

        while(stack.Count != 0)
        {
            MazeNode node = stack.Pop();

            // Spawn
            SpawnPiece(node);

            // Set Visited
            visited[node.Row, node.Col] = true;

            // visit each adjecent node
            foreach(MazeNode adjecentNode in node.GetAdjacentNodes())
            {
                if(!visited[adjecentNode.Row, adjecentNode.Col])
                {
                    stack.Push(adjecentNode);
                    visited[adjecentNode.Row, adjecentNode.Col] = true;
                }
            }

        }

    }

    public void SpawnPiece(MazeNode node)
    {
        Vector3 location = new Vector3(node.Row * 6, 0, node.Col * 6);

        Instantiate(Resources.Load(node.GetPrefabName()), location, node.GetRotation());
    }

}
