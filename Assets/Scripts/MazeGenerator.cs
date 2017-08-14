using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{

    public int Seed;
    public bool DebugLabelsOn;
    private NavMeshSurface surface;

    // Use this for initialization
    void Start()
    {
        int size = 10;
        int sections = 3;
        MazeNode root = RecursiveMazeGenerator.GenerateMaze(0, size, size);
        List<MazeNode> sectionroots = GenerateSections(root, sections + 1, size, size);
        /*
        root = GenTestMaze(size);
        root.Right.Left = null;
        root.Right = null;
        root.Forward.Right = null;
        root = RecursiveMazeGenerator.GenerateMaze(0, size, size);
        */
        foreach (MazeNode r in sectionroots)
        {
          SpawnMaze(r, size);
        }
        //SpawnMaze(root, size);
        surface = GetComponent<NavMeshSurface>();
        if (surface != null)
            surface.BuildNavMesh();
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
                if (j < size - 1)
                    maze[i, j].AddEdge(maze[i, j + 1]);
                if (i < size - 1)
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
        Stack visited = new Stack();
        LinkedList<MazeNode> path = GetPathHelper(start, end, visited);
        //print(path.Count);
        return path;
    }

    public static LinkedList<MazeNode> GetPathHelper(MazeNode start, MazeNode end, Stack visited)
    {
        List<MazeNode> adjacents = start.GetAdjacentNodes();
        bool leftAvailable = (start.Left != null && !visited.Contains(start.Left));
        bool rightAvailable = (start.Right != null && !visited.Contains(start.Right));
        bool forwardAvailable = (start.Forward != null && !visited.Contains(start.Forward));
        bool backwardAvailable = (start.Backward != null && !visited.Contains(start.Backward));
        LinkedList<MazeNode> path = new LinkedList<MazeNode>();
        if (start.Row == end.Row && start.Col == end.Col)
        {
            path.AddFirst(start);
            return path;
        }
        if (!leftAvailable && !rightAvailable && !forwardAvailable && !backwardAvailable)
            return null;
        foreach (MazeNode node in adjacents)
        {
            if (!visited.Contains(node))
            {
                visited.Push(node);
                path = GetPathHelper(node, end, visited);
                if (path != null)
                {
                    path.AddFirst(start);
                    return path;
                }
            }
        }
        return null;
    }

    public static void SetAsExitPath(LinkedList<MazeNode> path)
    {
        foreach (MazeNode n in path)
        {
            n.ExitNode = true;
        }
    }

    public static List<MazeNode> GenerateSections(MazeNode root, int sections, int rows, int cols)
    {
        MazeNode endNode = new MazeNode(cols - 1, rows - 1);
        SetAsExitPath(GetPath(root, endNode));
        List<MazeNode> sectionRoots = new List<MazeNode>();
        sectionRoots.Add(root);
        int searchedAreas = 0;
        int sectionSize = rows * cols / sections;
        while (searchedAreas < rows * cols)
        {
            searchedAreas++;
            if (searchedAreas == rows * cols)
            {
                sectionRoots.Add(root);
                break;
            }
            Queue<MazeNode> border = new Queue<MazeNode>();
            border.Enqueue(root);

            Stack<MazeNode> visited = new Stack<MazeNode>();
            visited.Push(root);

            int visitedNumber = 1;

            while (visitedNumber < sectionSize)
            {
                foreach (MazeNode n in border.Dequeue().GetAdjacentNodes())
                {
                    if (!visited.Contains(n))
                    {
                        visited.Push(n);
                        border.Enqueue(n);
                        visitedNumber++;
                        searchedAreas++;
                    }
                }
            }

            if (searchedAreas == rows * cols)
            {
                sectionRoots.Add(root);
                break;
            }

            MazeNode lastVisitedBottleneck = null;

            foreach (MazeNode n in visited)
            {
                if (n.ExitNode)
                {
                    lastVisitedBottleneck = n;
                    break;
                }
            }
            if (lastVisitedBottleneck == null)
                break;
            foreach (MazeNode n in lastVisitedBottleneck.GetAdjacentNodes())
            {
                if (n.ExitNode && !visited.Contains(n))
                {
                    if (n.Equals(lastVisitedBottleneck.Left))
                    {
                        lastVisitedBottleneck.DisconnectLeft();
                        n.DisconnectRight();
                        root = n;
                    }
                    if (n.Equals(lastVisitedBottleneck.Right))
                    {
                        lastVisitedBottleneck.DisconnectRight();
                        n.DisconnectLeft();
                        root = n;
                    }
                    if (n.Equals(lastVisitedBottleneck.Forward))
                    {
                        lastVisitedBottleneck.DisconnectForward();
                        n.DisconnectBackward();
                        root = n;
                    }
                    if (n.Equals(lastVisitedBottleneck.Backward))
                    {
                        lastVisitedBottleneck.DisconnectBackward();
                        n.DisconnectForward();
                        root = n;
                    }
                }
            } 
            while (border.Count != 0)
            {
                foreach (MazeNode n in border.Dequeue().GetAdjacentNodes())
                {
                    if (!visited.Contains(n))
                    {
                        visited.Push(n);
                        border.Enqueue(n);
                    }
                    searchedAreas++;
                }
            }
            sectionRoots.Add(root);
        }
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

    int piecesSpawned;

    public void SpawnPiece(MazeNode node)
    {
        Vector3 location = new Vector3(node.Col * 6 + 8, 0, node.Row * 6 + 8);

        GameObject obj = Instantiate(Resources.Load(node.GetPrefabName()), location, node.GetRotation()) as GameObject;
        obj.transform.parent = this.transform;

        if(DebugLabelsOn)
        {
            GameObject textObj = Instantiate(Resources.Load("Prefabs/CellTextPrefab"), location + new Vector3(0, 0.5f, -1), new Quaternion()) as GameObject;
            textObj.transform.parent = obj.transform;

            TextMesh t = textObj.GetComponentInChildren<TextMesh>();
            if (t != null)
                t.text = "R: " + node.Row + " C: " + node.Col;

            textObj = Instantiate(Resources.Load("Prefabs/CellTextPrefab"), location + new Vector3(0, 0.5f, 0), new Quaternion()) as GameObject;
            textObj.transform.parent = obj.transform;

            t = textObj.GetComponentInChildren<TextMesh>();
            if (t != null)
                t.text = "P" + piecesSpawned++;
        }

    }

}
