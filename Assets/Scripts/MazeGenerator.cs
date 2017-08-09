using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    public int Seed;

    public GameObject cell;

    // Use this for initialization
    void Start()
    {
        SpawnMaze(GenTestMaze(5), 5);
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
                maze[i, j] = new MazeNode();
                maze[i, j].Row = i;
                maze[i, j].Col = j;
            }
        }

        for (int i = 0; i < size - 1; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                maze[i, j].Right = maze[i, j + 1];
                maze[i, j + 1].Left = maze[i, j];
                maze[i, j].Forward = maze[i + 1, j];
                maze[i + 1, j].Backward = maze[i, j];
            }
            maze[size - 1, i].Row = size - 1;
            maze[size - 1, i].Col = i;
            maze[i, size - 1].Row = i;
            maze[i, size - 1].Col = size - 1;
        }

        maze[size - 1, size - 1].Backward = maze[size - 2, size - 1];
        maze[size - 2, size - 1].Forward = maze[size - 1, size - 1];
        maze[size - 1, size - 1].Right = maze[size - 1, size - 2];
        maze[size - 1, size - 2].Left = maze[size - 1, size - 1];

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

        // TODO use DFS to find the path

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

            if (visited[node.Row, node.Col])
                continue;

            // Spawn
            Vector3 location = new Vector3(node.Row * 6, 0, node.Col * 6);
            Instantiate(cell, location, new Quaternion());

            // Set Visited
            visited[node.Row, node.Col] = true;

            // Right
            if (node.Right != null && !visited[node.Right.Row, node.Right.Col])
                stack.Push(node.Right);

            // Forward
            if (node.Forward != null && !visited[node.Forward.Row, node.Forward.Col])
                stack.Push(node.Forward);

            // Left
            if (node.Left != null && !visited[node.Left.Row, node.Left.Col])
                stack.Push(node.Left);

            // Backward
            if (node.Backward != null && !visited[node.Backward.Row, node.Backward.Col])
                stack.Push(node.Backward);

        }

    }

}
