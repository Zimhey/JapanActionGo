using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    public static int Seed;
    public bool DebugLabelsOn;
    private NavMeshSurface surface;
    public static Difficulty dif = Difficulty.Small;
    public static MazeNode[,] DifferentSections = new MazeNode[5,8];

    // Use this for initialization
    void Start()
    {
        GenerateMaze(dif);
    }

    public void AddRandomActors(MazeNode root)
    {
        List<MazeNode> visited = new List<MazeNode>();
        Stack<MazeNode> nodesToVisit = new Stack<MazeNode>();

        nodesToVisit.Push(root);

        while (nodesToVisit.Count > 0)
        {
            MazeNode node = nodesToVisit.Pop();
            AddRandomActor(node);
            visited.Add(node);
            foreach (MazeNode n in node.GetAdjacentNodes())
                if (!nodesToVisit.Contains(n) && !visited.Contains(n))
                    nodesToVisit.Push(n);
        }
    }

    System.Random rand = new System.Random();

    public void AddRandomActor(MazeNode node)
    {
        int type = rand.Next() % 7;
        if (type == 0)
            node.actor = ActorType.Chalk_Pickup;
        else if (type == 1)
            node.actor = ActorType.Ofuda_Pickup;
        else if (type == 2)
            node.actor = ActorType.Oni;
        else if (type == 3)
            node.actor = ActorType.Okuri_Inu;
        else if (type == 4)
            node.actor = ActorType.Taka_Nyudo;
        else if (type == 5)
            node.actor = ActorType.Spike_Trap;
        else if (type == 6)
            node.actor = ActorType.Crush_Trap;
        else
            node.actor = ActorType.Null;

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

    public void GenerateMaze(Difficulty difficulty)
    {
        print("started generating maze");
        int size = 0;
        int[] sections = new int[] { };
        int loops = 0;
        int floors = 0;
        if (difficulty == Difficulty.Small)
        {
            size = 10;
            sections = new int[] { 2, 2, 1 };
            loops = 3;
            floors = 3;
        }
        if (difficulty == Difficulty.Medium)
        {
            size = 15;
            sections = new int[] { 2, 2, 1 };
            loops = 3;
            floors = 3;
        }
        if (difficulty == Difficulty.Large)
        {
            size = 20;
            sections = new int[] { 2, 3, 2 };
            loops = 4;
            floors = 3;
        }
        if (difficulty == Difficulty.Excessive)
        {
            size = 25;
            sections = new int[] { 3, 4, 4, 2 };
            loops = 4;
            floors = 4;
        }
        if (difficulty == Difficulty.AlreadyLost)
        {
            size = 30;
            sections = new int[] { 4, 6, 6, 3 };
            loops = 5;
            floors = 4;
        }
        if (difficulty == Difficulty.JustWhy)
        {
            size = 40;
            sections = new int[] { 5, 8, 8, 8, 4 };
            loops = 5;
            floors = 5;
        }

        GenerateMazeHelper(size, sections, loops, floors, difficulty);
    }

    public void GenerateMazeHelper(int size, int[] sections, int loops, int floors, Difficulty difficulty)
    {
        MazeNode[,] roots = new MazeNode[5, 8];
        for (int i = 0; i < floors; i++)
        {
            int section = 0;
            MazeNode root = DFSMazeGenerator.GenerateMaze(Seed, size, size, i);
            List<MazeNode> sectionroots;

            sectionroots = GenerateSections(root, sections[i], size, size);

            foreach (MazeNode r in sectionroots)
            {
                GenerateActors(r, 0, 0, 0, 0, Seed);
                GenerateLadders(i, section, r, floors, sections[i]);
                SetIntersectionNodes(r);
                roots[i, section] = r;
                GenerateLoops(r, loops, size);
                //SpawnMaze(r, size);
                section++;
            }
        }
        DifferentSections = roots;
        connectLadderNodes(difficulty, roots);
    }

    public void setSeed(int newSeed)
    {
        Seed = newSeed;
    }

    public static void connectLadderNodes(Difficulty difficulty, MazeNode[,] roots)
    {
        if (difficulty == Difficulty.Small || difficulty == Difficulty.Medium)
        {
            connectLadders(FindPathEnd(roots[0, 0]), roots[1, 0]);
            connectLadders(FindPathEnd(roots[1, 0]), roots[2, 0]);
            connectLadders(FindPathEnd(roots[2, 0]), roots[1, 1]);
            connectLadders(FindPathEnd(roots[1, 1]), roots[0, 1]);
        }
        if(difficulty == Difficulty.Large)
        {
            connectLadders(FindPathEnd(roots[0, 0]), roots[1, 0]);
            connectLadders(FindPathEnd(roots[1, 0]), roots[2, 0]);
            connectLadders(FindPathEnd(roots[2, 0]), roots[1, 1]);
            connectLadders(FindPathEnd(roots[1, 1]), roots[2, 1]);
            connectLadders(FindPathEnd(roots[2, 1]), roots[1, 2]);
            connectLadders(FindPathEnd(roots[1, 2]), roots[0, 1]);
        }
        if(difficulty == Difficulty.Excessive)
        {
            connectLadders(FindPathEnd(roots[0, 0]), roots[1, 0]);
            connectLadders(FindPathEnd(roots[1, 0]), roots[2, 0]);
            connectLadders(FindPathEnd(roots[2, 0]), roots[3, 0]);
            connectLadders(FindPathEnd(roots[3, 0]), roots[2, 1]);
            connectLadders(FindPathEnd(roots[2, 1]), roots[1, 1]);
            connectLadders(FindPathEnd(roots[1, 1]), roots[0, 1]);
            connectLadders(FindPathEnd(roots[0, 1]), roots[1, 2]);
            connectLadders(FindPathEnd(roots[1, 2]), roots[2, 2]);
            connectLadders(FindPathEnd(roots[2, 2]), roots[3, 1]);
            connectLadders(FindPathEnd(roots[3, 1]), roots[2, 3]);
            connectLadders(FindPathEnd(roots[2, 3]), roots[1, 3]);
            connectLadders(FindPathEnd(roots[1, 3]), roots[0, 2]);
        }
        if (difficulty == Difficulty.AlreadyLost)
        {
            connectLadders(FindPathEnd(roots[0, 0]), roots[1, 0]);
            connectLadders(FindPathEnd(roots[1, 0]), roots[2, 0]);
            connectLadders(FindPathEnd(roots[2, 0]), roots[3, 0]);
            connectLadders(FindPathEnd(roots[3, 0]), roots[2, 1]);
            connectLadders(FindPathEnd(roots[2, 1]), roots[1, 1]);
            connectLadders(FindPathEnd(roots[1, 1]), roots[0, 1]);
            connectLadders(FindPathEnd(roots[0, 1]), roots[1, 2]);
            connectLadders(FindPathEnd(roots[1, 2]), roots[2, 2]);
            connectLadders(FindPathEnd(roots[2, 2]), roots[3, 1]);
            connectLadders(FindPathEnd(roots[3, 1]), roots[2, 3]);
            connectLadders(FindPathEnd(roots[2, 3]), roots[1, 3]);
            connectLadders(FindPathEnd(roots[1, 3]), roots[0, 2]);
            connectLadders(FindPathEnd(roots[0, 2]), roots[1, 4]);
            connectLadders(FindPathEnd(roots[1, 4]), roots[2, 4]);
            connectLadders(FindPathEnd(roots[2, 4]), roots[3, 2]);
            connectLadders(FindPathEnd(roots[3, 2]), roots[2, 5]);
            connectLadders(FindPathEnd(roots[2, 5]), roots[1, 5]);
            connectLadders(FindPathEnd(roots[1, 5]), roots[0, 3]);
        }
        if (difficulty == Difficulty.JustWhy)
        {
            connectLadders(FindPathEnd(roots[0, 0]), roots[1, 0]);
            connectLadders(FindPathEnd(roots[1, 0]), roots[2, 0]);
            connectLadders(FindPathEnd(roots[2, 0]), roots[3, 0]);
            connectLadders(FindPathEnd(roots[2, 0]), roots[4, 0]);
            connectLadders(FindPathEnd(roots[2, 0]), roots[3, 1]);
            connectLadders(FindPathEnd(roots[3, 0]), roots[2, 1]);
            connectLadders(FindPathEnd(roots[2, 1]), roots[1, 1]);
            connectLadders(FindPathEnd(roots[1, 1]), roots[0, 1]);
            connectLadders(FindPathEnd(roots[0, 1]), roots[1, 2]);
            connectLadders(FindPathEnd(roots[1, 2]), roots[2, 2]);
            connectLadders(FindPathEnd(roots[2, 2]), roots[3, 2]);
            connectLadders(FindPathEnd(roots[2, 2]), roots[4, 1]);
            connectLadders(FindPathEnd(roots[2, 2]), roots[3, 3]);
            connectLadders(FindPathEnd(roots[3, 1]), roots[2, 3]);
            connectLadders(FindPathEnd(roots[2, 3]), roots[1, 3]);
            connectLadders(FindPathEnd(roots[1, 3]), roots[0, 2]);
            connectLadders(FindPathEnd(roots[0, 2]), roots[1, 4]);
            connectLadders(FindPathEnd(roots[1, 4]), roots[2, 4]);
            connectLadders(FindPathEnd(roots[2, 4]), roots[3, 4]);
            connectLadders(FindPathEnd(roots[2, 4]), roots[4, 2]);
            connectLadders(FindPathEnd(roots[2, 4]), roots[3, 5]);
            connectLadders(FindPathEnd(roots[3, 2]), roots[2, 5]);
            connectLadders(FindPathEnd(roots[2, 5]), roots[1, 5]);
            connectLadders(FindPathEnd(roots[1, 5]), roots[0, 3]);
            connectLadders(FindPathEnd(roots[2, 4]), roots[1, 6]);
            connectLadders(FindPathEnd(roots[3, 2]), roots[2, 6]);
            connectLadders(FindPathEnd(roots[2, 5]), roots[3, 6]);
            connectLadders(FindPathEnd(roots[2, 4]), roots[4, 3]);
            connectLadders(FindPathEnd(roots[2, 4]), roots[3, 7]);
            connectLadders(FindPathEnd(roots[3, 2]), roots[2, 7]);
            connectLadders(FindPathEnd(roots[2, 5]), roots[1, 7]);
            connectLadders(FindPathEnd(roots[1, 5]), roots[0, 4]);
        }
    }

    public static void connectLadders(MazeNode node1, MazeNode node2)
    {
        node1.ladderMazeNode = node2;
        node2.ladderMazeNode = node1;
    }

    public static LinkedList<MazeNode> GetPath(MazeNode start, MazeNode end)
    {
        Stack visited = new Stack();
        LinkedList<MazeNode> path = GetPathHelper(start, end, visited);
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

    public static void SetIntersectionNodes(MazeNode root)
    {
        foreach(MazeNode n in nodesInSection(root))
            if (n.GetAdjacentNodes().Count > 2)
                n.Intersection = true;
    }

    public static List<MazeNode> GetIntersectionNodes(MazeNode root)
    {
        List<MazeNode> intersections = new List<MazeNode>();
        foreach (MazeNode n in nodesInSection(root))
            if (n.Intersection)
                intersections.Add(n);
        return intersections;
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
                return node.backwardNodes;
            }
            else
            {
                return node.forwardNodes;
            }
        }
        else if(node.Row == other.Row)
        {
            if (node.Col == other.Col + 1)
            {
                return node.leftNodes;
            }
            else
            {
                return node.rightNodes;
            }
        }
        else
        {
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
                next = n;
                int wouldBeRemoved = getDirectionalValue(cutoff, next);
                if (wouldBeRemoved < leftover - sectionSize)
                {
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

    public static List<MazeNode> nodesInSection(MazeNode root)
    {
        List<MazeNode> nodes = new List<MazeNode>();
        Stack<MazeNode> visited = new Stack<MazeNode>();
        visited.Push(root);
        nodes.Add(root);

        while(visited.Count > 0)
        {
            MazeNode current = visited.Pop();
            foreach(MazeNode n in current.GetAdjacentNodes())
                if(!nodes.Contains(n))
                {
                    visited.Push(n);
                    nodes.Add(n);
                }
        }

        return nodes;
    }

    public static int GetSize()
    {
        if (dif == Difficulty.Small)
            return 10;
        if (dif == Difficulty.Medium)
            return 15;
        if (dif == Difficulty.Large)
            return 20;
        if (dif == Difficulty.Excessive)
            return 25;
        if (dif == Difficulty.AlreadyLost)
            return 30;
        if (dif == Difficulty.JustWhy)
            return 40;
        else
            return 0;
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
                    visited.Push(node);
                    visited2.Push(node);
                    if (node.Col == n.Col + deltaX && node.Row == n.Row + deltaY)
                    {
                        return node;
                    }
                }
            }
        }

        return null;
    }

    public static int getConnectedNodes(MazeNode node)
    {
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
        MazeNode previous = new MazeNode(-1, -1);
        MazeNode current = root;
        bool currentChanged = false;
        int counter = 0;

        while (!previous.Equals(current) && counter < 1000)
        {
            foreach (MazeNode n in current.GetAdjacentNodes())
            {
                if (n.OnExitPath && !n.Equals(previous))
                {
                    previous = current;
                    current = n;
                    currentChanged = true;
                    break;
                }
            }

            if(!currentChanged)
                previous = current;
            currentChanged = false;
            counter++;
        }
        return current;
    }

    public static void GenerateActors(MazeNode root, int ofuda, int oni, int chalk, int trap, int seed)
    {
        System.Random rand = new System.Random();//(seed);
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
            counter = 0;
            int temp = 0;
            bool listContains = true;
            while(listContains && counter < 1000)
            {
                temp = rand.Next(0, PossiblePlaces);
                listContains = false;
                for(int j = 0; j < actors; j++)
                {
                    if (actorLocations[j] == temp)
                        listContains = true;
                }
                counter++;
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

                                int counter2 = 0;
                                while (usedUp && counter2 < 1000)
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
                                    else if(type == 3 && n.GetAdjacentNodes().Count > 1)
                                    {
                                        n.actor = ActorType.Spike_Trap;
                                        tr--;
                                    }
                                    counter2++;
                                }
                            }
                        }
                    }
                    visited.Push(n);
                    visited2.Push(n);
                }
            }
        }
    }
    
    public static void GenerateLadders(int floor, int section, MazeNode root, int TotalFloors, int TotalSections)
    {
        if (floor == 0)
        {
            if (section == 0)
                if (!GameManager.DebugOn)
                    root.actor = ActorType.Ladder;
            if (section > 0)
            {
                root.actor = ActorType.Ladder;
            }
            if (section < TotalSections - 1)
            {
                FindPathEnd(root).actor = ActorType.Ladder;
            }
        }
        else
        {
            root.actor = ActorType.Ladder;
            FindPathEnd(root).actor = ActorType.Ladder;
        }
    }

    public static void GenerateLoops(MazeNode root, int loops, int size)
    {
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

            counter = 0;

            while (visited.Count > 0 && counter < 1000)
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
            SpawnActor(node);

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

        obj = Instantiate(Resources.Load("Prefabs/Level/CellLog"), location, node.GetRotation()) as GameObject;
        obj.transform.parent = this.transform;
        CellLog cellLog = obj.GetComponent<CellLog>();
        cellLog.Row = node.Row;
        cellLog.Col = node.Col;

        if(DebugLabelsOn)
        {
            GameObject textObj = Instantiate(Resources.Load("Prefabs/Level/CellTextPrefab"), location + new Vector3(0, 0.5f, -1), new Quaternion()) as GameObject;
            textObj.transform.parent = obj.transform;

            TextMesh t = textObj.GetComponentInChildren<TextMesh>();
            if (t != null)
                t.text = "R: " + node.Row + " C: " + node.Col;

            textObj = Instantiate(Resources.Load("Prefabs/Level/CellTextPrefab"), location + new Vector3(0, 0.5f, 0), new Quaternion()) as GameObject;
            textObj.transform.parent = obj.transform;

            t = textObj.GetComponentInChildren<TextMesh>();
            if (t != null)
                t.text = "P" + piecesSpawned++;
        }

    }

    public void SpawnActor(MazeNode node)
    {
        Vector3 location = new Vector3(node.Col * 6 + 8, node.Floor * 30, node.Row * 6 + 8);
        if (node.actor != ActorType.Null)
        {
            //if (node.actor == ActorType.Ladder)
                //node.ladder = Instantiate(Actors.Prefabs[node.actor], location, node.GetRotation());
            //else
                Instantiate(Actors.Prefabs[node.actor], location, node.GetRotation());
        }
    }
    
    public static MazeNode getSectionBasedOnLocation(Vector3 location)
    {
        int column = (int) ((location.x - 8) / 6);
        int floor = (int) (location.y / 30);
        int row = (int)((location.z - 8) / 6);

        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 8; j++)
                foreach (MazeNode n in nodesInSection(DifferentSections[i, j]))
                    if (n.Col == column && n.Row == row && n.Floor == floor)
                        return DifferentSections[i, j];
        return null;
    }
}
