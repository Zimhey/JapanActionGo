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
        MazeNode root = DFSMazeGenerator.GenerateMaze(0, size, size);
        List<MazeNode> sectionroots = GenerateSections(root, sections, size, size);
        /*
        root = GenTestMaze(size);
        root.Right.Left = null;
        root.Right = null;
        root.Forward.Right = null;
        root = RecursiveMazeGenerator.GenerateMaze(0, size, size);
        */
        foreach (MazeNode r in sectionroots)
        {
            GenerateLoops(r, 2, size);
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
            n.OnExitPath = true;
        }
    }

    public static void setDirectionValues(MazeNode root)
    {
        foreach (MazeNode n in root.GetAdjacentNodes())
        {
            int total = 1;
            setDirectionValuesHelper(root, n);
            total += n.forwardNodes;
            total += n.backwardNodes;
            total += n.leftNodes;
            total += n.rightNodes;
            if (n.Equals(root.Left))
                root.leftNodes = total;
            else if (n.Equals(root.Right))
                root.rightNodes = total;
            else if (n.Equals(root.Forward))
                root.forwardNodes = total;
            else
                root.backwardNodes = total;
        }
    }

    public static void setDirectionValuesHelper(MazeNode previous, MazeNode current)
    {
        foreach(MazeNode n in current.GetAdjacentNodes())
        {
            int total = 1;
            if (!n.Equals(previous))
            {
                setDirectionValuesHelper(current, n);
                total += n.forwardNodes;
                total += n.backwardNodes;
                total += n.leftNodes;
                total += n.rightNodes;
            }
            if(n.Equals(previous))
            {
                total = 0;
            }
            if (n.Equals(current.Left))
                current.leftNodes = total;
            else if (n.Equals(current.Right))
                current.rightNodes = total;
            else if (n.Equals(current.Forward))
                current.forwardNodes = total;
            else
                current.backwardNodes = total;
        }
    }

    public static int getDirectionalValue(MazeNode node, MazeNode other)
    {
        if(node.Col == other.Col)
        {
            if (node.Row == other.Row + 1)
            {
                //print("Made it backwards!");
                return node.backwardNodes;
            }
            else
            {
                //print("Made it forwards!");
                return node.forwardNodes;
            }
        }
        else if(node.Row == other.Row)
        {
            if (node.Col == other.Col + 1)
            {
                //print("Made it left!");
                return node.leftNodes;
            }
            else
            {
                //print("Made it right!");
                return node.rightNodes;
            }
        }
        else
        {
            //print("Made it here!");
            return 0;
        }
    }

    public static List<MazeNode> GenerateSections(MazeNode root, int sections, int rows, int cols)
    {
        int leftover = rows * cols;
        MazeNode endNode = new MazeNode(cols - 1, rows - 1);
        LinkedList<MazeNode> path = GetPath(root, endNode);
        SetAsExitPath(path);
        List<MazeNode> sectionRoots = new List<MazeNode>();
        sectionRoots.Add(root);
        MazeNode cutoff = root;
        MazeNode next = root;
        MazeNode previous = root;
        int sectionSize = rows * cols / sections;
        setDirectionValues(root);

        foreach (MazeNode n in path)
        {
            if (n != root)
            {
                //print("here i am");
                next = n;
                //print(cutoff.Col + " " + cutoff.Row + " ");
                //print(next.Col + " " + next.Row + " ");
                int wouldBeRemoved = getDirectionalValue(cutoff, next);
                //print(wouldBeRemoved);
                if (wouldBeRemoved < leftover - sectionSize)
                {
                    //cutoff.RemoveEdge(next);
                    previous.RemoveEdge(cutoff);
                    sectionRoots.Add(cutoff);
                    cutoff = next;
                    leftover = wouldBeRemoved;
                }
                if (leftover < sectionSize)
                    break;
                previous = cutoff;
                cutoff = next;
            }
        }

        return sectionRoots;
    }

    public static MazeNode findNode(MazeNode n, int direction)
    {
        int deltaX;
        int deltaY;
        MazeNode current;
        Stack<MazeNode> visited = new Stack<MazeNode>();
        Stack<MazeNode> visited2 = new Stack<MazeNode>();
        visited.Push(n);
        visited2.Push(n);

        if (direction == 0)
        {
            deltaX = -1;
            deltaY = 0;
        }
        else if (direction == 1)
        {
            deltaX = 0;
            deltaY = 1;
        }
        else if (direction == 2)
        {
            deltaX = 1;
            deltaY = 0;
        }
        else if (direction == 3)
        {
            deltaX = 0;
            deltaY = -1;
        }
        else
        {
            return null;
        }

        while (visited.Count > 0)
        {
            current = visited.Pop();
            foreach (MazeNode node in current.GetAdjacentNodes())
            {
                if (!visited2.Contains(node))
                {
                    //print("made it here");
                    visited.Push(node);
                    visited2.Push(node);
                    if (node.Col == n.Col + deltaX && node.Row == n.Row + deltaY)
                    {
                        //print("made it here");
                        return node;
                    }
                }
            }
        }

        return null;
    }

    public static int getConnectedNodes(MazeNode node)
    {
        //print("Column: " + node.Col + " Row: " + node.Row + " Forward " + node.forwardNodes + " Backwards " + node.backwardNodes + " Left " + node.leftNodes + " Right " + node.rightNodes);
        return node.leftNodes + node.rightNodes + node.forwardNodes + node.backwardNodes + 1;
    }

    public static MazeNode getNodeOnPath(MazeNode root, int index)
    {
        int current = 0;
        MazeNode node = root;
        while(current < index)
        {
            foreach(MazeNode n in node.GetAdjacentNodes())
            {
                if(n.OnExitPath)
                {
                    current++;
                    node = n;
                    break;
                }
            }
        }
        return node;
    }

    public static void GenerateLoops(MazeNode root, int loops, int size)
    {
        print(root.Col + " " + root.Row);
        // TODO Generate Loops
        MazeNode current = root;
        MazeNode next = root;
        MazeNode previous = null;
        int[,] disconnectingWalls = new int[size * size, 4];
        int pathNumber = 0;
        int counter;
        int counter2;

        for(counter = 0; counter < size; counter++)
            for(counter2 = 0; counter2 < 4; counter2++)
                disconnectingWalls[counter, counter2] = 0;

        while (!current.Equals(previous))
        {
            for (counter = 0; counter < 4; counter++)
            {
                int disconnected;
                if (counter == 0 && current.Left == null || counter == 1 && current.Forward == null || counter == 2 && current.Right == null || counter == 3 && current.Backward == null)
                {
                    //print(current.Col + " " + current.Row + " " + counter);
                    MazeNode disconnectedNode = findNode(current, counter);
                    if (disconnectedNode != null)
                    {
                        print(current.Col + " " + current.Row + " " + counter);
                        disconnected = getConnectedNodes(disconnectedNode);
                        if(disconnected != 0)
                        {
                            //print(disconnected);
                        }
                        disconnectingWalls[pathNumber, counter] = disconnected;
                    }
                }
            }
            foreach (MazeNode n in current.GetAdjacentNodes())
            {
                if (n.OnExitPath && !n.Equals(previous))
                {
                    pathNumber++;
                    next = n;
                    break;
                }
            }
            previous = current;
            current = next;
        }

        while (loops > 0)
        {
            int largestDisconnection = 0;
            int largestIndexPath = 0;
            int largetIndexDirection = 0;
            int i;
            int j;
            for(i = 0; i < size; i++)
            {
                for(j = 0; j < 4; j++)
                {
                    if(disconnectingWalls[i, j] > largestDisconnection)
                    {
                        //print("made it here");
                        largestDisconnection = disconnectingWalls[i, j];
                        largestIndexPath = i;
                        largetIndexDirection = j;
                    }
                }
            }

            MazeNode mn = getNodeOnPath(root, largestIndexPath);
            //print(largestDisconnection + " " + largestIndexPath + " " + largetIndexDirection + " " + mn.Col + " " + mn.Row);
            //mn.AddEdge(findNode(mn, largetIndexDirection));
            loops--;
            disconnectingWalls[largestIndexPath, largetIndexDirection] = 0;
            largetIndexDirection = 0;
            largestIndexPath = 0;
            largestDisconnection = 0;
        }
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
