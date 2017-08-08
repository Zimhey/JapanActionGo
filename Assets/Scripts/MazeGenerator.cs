using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void GenerateMaze(int seed)
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

    public void SpawnMaze(MazeNode root)
    {
        // TODO use DFS to search through maze and spawn each piece
    }

}
