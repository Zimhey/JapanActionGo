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
        List<MazeNode> sectionroots = GenerateSections3(root, sections, size, size);
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
                print("Made it backwards!");
                return node.backwardNodes;
            }
            else
            {
                print("Made it forwards!");
                return node.forwardNodes;
            }
        }
        else if(node.Row == other.Row)
        {
            if (node.Col == other.Col + 1)
            {
                print("Made it left!");
                return node.leftNodes;
            }
            else
            {
                print("Made it right!");
                return node.rightNodes;
            }
        }
        else
        {
            print("Made it here!");
            return 0;
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
                if (n.OnExitPath)
                {
                    lastVisitedBottleneck = n;
                    break;
                }
            }
            if (lastVisitedBottleneck == null)
                break;

            MazeNode possibleRoot = new MazeNode();

            foreach (MazeNode n in lastVisitedBottleneck.GetAdjacentNodes())
            {
                if (n.OnExitPath && !visited.Contains(n))
                {
                    lastVisitedBottleneck.RemoveEdge(n);
                    root = n;
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
                        visitedNumber++;
                        searchedAreas++;
                    }
                }
            }

            setDirectionValues(root);

            //if (searchedAreas == rows * cols)
                //break;

            int remove = visitedNumber - sectionSize;
            MazeNode beingRemoved = lastVisitedBottleneck;
            int counter = 1;
            int visitedNumberCopy = visitedNumber;
            int searchedAreasCopy = searchedAreas;

            while(remove > 0 && counter < 1000)
            {
                visitedNumber = visitedNumberCopy;
                searchedAreas = searchedAreasCopy;
                remove = visitedNumber - sectionSize;
                foreach(MazeNode n in beingRemoved.GetAdjacentNodes())
                {
                    if(getDirectionalValue(beingRemoved, n) == 0)
                    {
                        remove -= getDirectionalValue(n, beingRemoved);
                        visitedNumber -= getDirectionalValue(n, beingRemoved);
                        searchedAreas -= getDirectionalValue(n, beingRemoved);
                        beingRemoved.AddEdge(root);
                        root = beingRemoved;
                        beingRemoved = n;
                        n.RemoveEdge(beingRemoved);
                        break;
                    }
                }
                counter++;
            }
            print(beingRemoved.Col + " " + beingRemoved.Row + " " + root.Col + " " + root.Row);
            if (counter >= 1000)
                print("counter timed out");

            /*
            int minRemoval;
            int maxRemoval;
            MazeNode beingRemoved = lastVisitedBottleneck;
            bool enoughRemoved = false;            
            
            if(visitedNumber - sectionSize > sectionSize * .1)
            {
                minRemoval = (int)(visitedNumber - sectionSize - sectionSize * .1);
                maxRemoval = (int)(visitedNumber - sectionSize + sectionSize * .1);

                while (!enoughRemoved)
                {
                    List<MazeNode> disconnected = new List<MazeNode>();
                    int wouldBeRemoved = 0;
                    foreach (MazeNode n in beingRemoved.GetAdjacentNodes())
                    {
                        wouldBeRemoved = getDirectionalValue(beingRemoved, n);
                        if (wouldBeRemoved < minRemoval && wouldBeRemoved != 0)
                        {
                            beingRemoved.RemoveEdge(n);
                            disconnected.Add(n);
                            minRemoval -= wouldBeRemoved;
                            maxRemoval -= wouldBeRemoved;
                        }
                        else if (wouldBeRemoved < maxRemoval && wouldBeRemoved != 0)
                        {
                            beingRemoved.RemoveEdge(n);
                            lastVisitedBottleneck.AddEdge(root);
                            enoughRemoved = true;
                            //root = n;
                        }
                        else if (wouldBeRemoved > maxRemoval && wouldBeRemoved != 0)
                        {
                            beingRemoved = n;
                        }
                    }

                    if(!(wouldBeRemoved > maxRemoval) && !enoughRemoved)
                    {
                        minRemoval = 0;//(int)(visitedNumber - sectionSize - sectionSize * .1);
                        maxRemoval = (int)(visitedNumber - sectionSize + sectionSize * .1);
                        foreach (MazeNode n in disconnected)
                        {
                            beingRemoved.AddEdge(n);
                        }
                        foreach(MazeNode n in beingRemoved.GetAdjacentNodes())
                        {
                            if(getDirectionalValue(beingRemoved, n) == 0)
                            {
                                beingRemoved = n;
                                break;
                            }
                        }
                    }
                }                
            }
            */
            sectionRoots.Add(root);
        }
        return sectionRoots;
    }

    public static List<MazeNode> GenerateSections2(MazeNode root, int sections, int rows, int cols)
    {
        int leftover = rows * cols;
        MazeNode endNode = new MazeNode(cols - 1, rows - 1);
        LinkedList<MazeNode> path = GetPath(root, endNode);
        SetAsExitPath(path);
        List<MazeNode> sectionRoots = new List<MazeNode>();
        sectionRoots.Add(root);
        string p = "";
        MazeNode previous = root;

        int sectionSize = rows * cols / sections;
        MazeNode cutoff = root;
        MazeNode next = new MazeNode();
        setDirectionValues(cutoff);
        int counter = 0;

        foreach (MazeNode n in path)
        {
            p += n.Col + " " + n.Row + " L" + n.leftNodes + " R" + n.rightNodes + " F" + n.forwardNodes + " B" + n.backwardNodes + " Exit Path:" + n.OnExitPath + " ";
        }

        while (leftover > sectionSize && counter < 1000)
        {
            //print(leftover);
            sectionRoots.Add(cutoff);
            foreach (MazeNode n in cutoff.GetAdjacentNodes())
                if (n.OnExitPath)
                    next = n;
            while (getDirectionalValue(cutoff, next) > leftover - sectionSize)
            {
                //print(leftover - sectionSize);
                //print(getDirectionalValue(cutoff, next));
                cutoff = next;
                foreach (MazeNode n in cutoff.GetAdjacentNodes())
                    if (n.OnExitPath)
                        next = n;
                //print(getDirectionalValue(cutoff, next));
            }
            //print(getDirectionalValue(cutoff, next));
            leftover = getDirectionalValue(cutoff, next);
            cutoff.RemoveEdge(next);
            cutoff = next;
            counter++;
        }

        print(p);
        return sectionRoots;
    }

    public static List<MazeNode> GenerateSections3(MazeNode root, int sections, int rows, int cols)
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
                print(cutoff.Col + " " + cutoff.Row + " ");
                print(next.Col + " " + next.Row + " ");
                int wouldBeRemoved = getDirectionalValue(cutoff, next);
                print(wouldBeRemoved);
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
