﻿using System.Collections;
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
        int sections = 2;
        int loops = 3;
        int floors = 4;

        for (int i = 0; i < floors; i++)
        {
            int section = 1;
            MazeNode root = DFSMazeGenerator.GenerateMaze(0, size, size, i);
            List<MazeNode> sectionroots;

            if (i == 1)
            {
                sectionroots = GenerateSections(root, sections, size, size);
            }

            else
            {
                sectionroots = GenerateSections(root, 1, size, size);
            }

            foreach (MazeNode r in sectionroots)
            {
                GenerateActors(r, 1, 1, 1, 1);
                GenerateLadders(i, section, r);
                GenerateLoops(r, loops, size);
                SpawnMaze(r, size);
                section++;
            }

            surface = GetComponent<NavMeshSurface>();
            if (surface != null)
                surface.BuildNavMesh();
        }
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
        MazeNode prev = root;
        while(current < index)
        {
            foreach(MazeNode n in node.GetAdjacentNodes())
            {
                if(n.OnExitPath && !n.Equals(prev))
                {
                    current++;
                    prev = node;
                    node = n;
                    break;
                }
            }
        }
        return node;
    }
    
    public static int DistanceBetween(MazeNode start, MazeNode finish)
    {
        List<MazeNode> prev = new List<MazeNode>();
        prev.Add(start);
        return DistanceBetweenHelper(prev, start, finish, 0);
    }

    public static int DistanceBetweenHelper(List<MazeNode> prev, MazeNode current, MazeNode finish, int distance)
    {
        int branchDistance = 0;
        foreach(MazeNode n in current.GetAdjacentNodes())
        {
            if (n.Equals(finish))
                return distance + 1;
            else if (!prev.Contains(n))
            {
                prev.Add(current);
                int branchDistance2 = DistanceBetweenHelper(prev, n, finish, distance + 1);
                if (branchDistance2 > branchDistance)
                    branchDistance = branchDistance2;
            }
        }
        return branchDistance;
    }

    public static int NumberOfDeadEndNodes(MazeNode root)
    {
        int number = 0;
        Stack<MazeNode> visited = new Stack<MazeNode>();
        Stack<MazeNode> visited2 = new Stack<MazeNode>();
        MazeNode current;

        visited.Push(root);
        visited2.Push(root);

        while (visited.Count > 0)
        {
            current = visited.Pop();
            foreach (MazeNode n in current.GetAdjacentNodes())
                if (!visited2.Contains(n))
                {
                    if (!n.OnExitPath)
                        number++;
                    visited.Push(n);
                    visited2.Push(n);
                }
        }

        return number;
    }

    public static MazeNode FindPathEnd(MazeNode root)
    {
        MazeNode previous = root;
        MazeNode current = root;

        foreach(MazeNode n in current.GetAdjacentNodes())
        {
            if(n.OnExitPath && !n.Equals(previous))
            {
                previous = current;
                current = n;
            }
        }

        return current;
    }

    public static void GenerateActors(MazeNode root, int ofuda, int oni, int chalk, int trap)
    {
        System.Random rand = new System.Random();
        int PossiblePlaces = NumberOfDeadEndNodes(root);
        int actors = ofuda + oni + chalk;
        int[] actorLocations = new int[actors];
        int of = ofuda;
        int on = oni;
        int ch = chalk;
        int tr = trap;

        int counter = 0;

        int number = 0;
        Stack<MazeNode> visited = new Stack<MazeNode>();
        Stack<MazeNode> visited2 = new Stack<MazeNode>();
        MazeNode current;

        visited.Push(root);
        visited2.Push(root);

        for (int i = 0; i < actors; i++)
        {
            actorLocations[i] = PossiblePlaces;
        }

        for (int i = 0; i < actors; i++)
        {
            int temp = 0;
            bool listContains = true;
            while(listContains)
            {
                temp = rand.Next(0, PossiblePlaces);
                listContains = false;
                for(int j = 0; j < actors; j++)
                {
                    if (actorLocations[j] == temp)
                        listContains = true;
                }
            }
            actorLocations[i] = temp;
        }

        while (visited.Count > 0 && counter < 1000)
        {
            current = visited.Pop();
            foreach (MazeNode n in current.GetAdjacentNodes())
            {
                if (!visited2.Contains(n))
                {
                    if (!n.OnExitPath)
                    {
                        number++;
                        foreach (int loc in actorLocations)
                        {
                            if (of == 0 && on == 0 && ch == 0 && tr == 0)
                                break;
                            if (loc == number)
                            {
                                bool usedUp = true;
                                while (usedUp)
                                {
                                    usedUp = false;
                                    int type = rand.Next(0, 4);
                                    if (type == 0 && of == 0)
                                        usedUp = true;
                                    else if (type == 0)
                                    {
                                        n.actor = ActorType.Ofuda_Pickup;
                                        of--;
                                    }
                                    else if (type == 1 && on == 0)
                                        usedUp = true;
                                    else if (type == 1)
                                    {
                                        n.actor = ActorType.Oni;
                                        on--;
                                    }
                                    else if (type == 2 && ch == 0)
                                        usedUp = true;
                                    else if (type == 2)
                                    {
                                        n.actor = ActorType.Chalk_Pickup;
                                        ch--;
                                    }
                                    else if (type == 3 && tr == 0)
                                        usedUp = true;
                                    else if(type == 3)
                                    {
                                        n.actor = ActorType.Spike_Trap;
                                        tr--;
                                    }
                                }
                            }
                        }
                    }
                    visited.Push(n);
                    visited2.Push(n);
                }
            }
            counter++;
        }
        print(counter);
    }

    public static void GenerateLadders(int floor, int section, MazeNode root)
    {
        if (floor == 1)
        {
            if(section == 1)
            {
                FindPathEnd(root).actor = ActorType.Ladder;
            }
            if(section == 2)
            {
                root.actor = ActorType.Ladder;
            }
        }
        if(floor == 2)
        {
            root.actor = ActorType.Ladder;
            FindPathEnd(root).actor = ActorType.Ladder;
        }
    }

    public static void GenerateLoops(MazeNode root, int loops, int size)
    {
        //print(root.Col + " " + root.Row);
        // TODO Generate Loops
        MazeNode current;
        Stack<MazeNode> visited = new Stack<MazeNode>();
        Stack<MazeNode> visited2 = new Stack<MazeNode>();
        int[,,] disconnectingWalls = new int[size, size, 4];
        MazeNode[,,] disconnectedNodes = new MazeNode[size, size, 4];
        int counter;
        int counter2;
        int counter3;

        while (loops > 0)
        {
            int largestDisconnection = 0;
            int largestCol = 0;
            int largestRow = 0;
            int largestDir = 0;
            int i;
            int j;
            int k;
            MazeNode largestDisconnectedNode = root;

            visited = new Stack<MazeNode>();
            visited2 = new Stack<MazeNode>();
            visited.Push(root);
            visited2.Push(root);

            for (counter = 0; counter < size; counter++)
                for (counter2 = 0; counter2 < size; counter2++)
                    for (counter3 = 0; counter3 < 4; counter3++)
                        disconnectingWalls[counter, counter2, counter3] = 0;

            while (visited.Count > 0)
            {
                current = visited.Pop();

                for (counter = 0; counter < 4; counter++)
                {
                    int disconnected;
                    if (counter == 0 && current.Left == null || counter == 1 && current.Forward == null || counter == 2 && current.Right == null || counter == 3 && current.Backward == null)
                    {
                        MazeNode disconnectedNode = findNode(current, counter);
                        if (disconnectedNode != null)
                        {
                            disconnected = DistanceBetween(current, disconnectedNode);
                            disconnectingWalls[current.Col, current.Row, counter] = disconnected;
                            disconnectedNodes[current.Col, current.Row, counter] = current;
                        }
                    }
                }

                foreach (MazeNode n in current.GetAdjacentNodes())
                {
                    if (!visited2.Contains(n))
                    {
                        visited.Push(n);
                        visited2.Push(n);
                    }
                }
            }

            for (i = 0; i < size; i++)
            {
                for (j = 0; j < size; j++)
                {
                    for (k = 0; k < 4; k++)
                    {
                        if (disconnectingWalls[i, j, k] > largestDisconnection)
                        {
                            largestDisconnection = disconnectingWalls[i, j, k];
                            largestDisconnectedNode = disconnectedNodes[i, j, k];
                            largestCol = i;
                            largestRow = j;
                            largestDir = k;
                        }
                    }
                }
            }

            MazeNode mazeNode = findNode(largestDisconnectedNode, largestDir);
            if (mazeNode != null)
            {
                //print("Column: " + largestDisconnectedNode.Col + " Row: " + largestDisconnectedNode.Row + " Disconnection: " + largestDisconnection + " Direction: " + largestDir);
                largestDisconnectedNode.AddEdge(mazeNode);
                disconnectingWalls[largestCol, largestRow, largestDir] = 0;
                disconnectingWalls[mazeNode.Col, mazeNode.Row, (largestDir + 2) % 4] = 0;
            }
            loops--;
            largestDisconnection = 0;
            largestDir = 0;
            largestCol = 0;
            largestRow = 0;
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
        Vector3 location = new Vector3(node.Col * 6 + 8, node.Floor * 30, node.Row * 6 + 8);

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
