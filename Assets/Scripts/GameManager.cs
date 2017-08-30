using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum GameState
{
    Intro,
    Main,
    Tutorial,
    Play,
    Pause,
    GameOver
}

public enum VirtualRealityType
{
    None,
    SteamVR,
    Oculus,
    Vive,
    Cardboard // lol maybe
}

public struct MazeSection
{
    public int SectionID;
    public MazeNode Root;
    public bool Spawned;
    public List<GameObject> Actors;
    public GameObject section;
}

public class GameManager : MonoBehaviour {

    private static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new GameManager();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    public GameObject PlayerObj;

    public string PlayerTypeLoc;

    public static bool DebugOn = true;

    public static GameObject Maze;

    public static bool DebugLabelsOn;

    public Difficulty dif = Difficulty.Small;

    public static int SessionID;

    public GameState CurrentState;
    private GameState prevState;

    public VirtualRealityType PlayersVRType;

    // Collection of every section on every floor
    public static List<MazeSection> Sections;

    public static MazeSection PlayersCurrentSection;

    public int lvlID;

    private MazeNode[,] tutorial4;

    // Analytics
    public bool AnalyticsEnabled = false;

    public MazeGenerator generator;

    private void Start()
    {
        //start2();
    }

    public void start2()
    {
        if (DebugOn)
            BeginPlay();
        else
            BeginTutorial();
    }

    public void setDifficulty(int diff)
    {
        dif = (Difficulty) diff;
    }

    public void setTutorial()
    {
        if (DebugOn)
            DebugOn = false;
        else
            DebugOn = true;
    }


    // Achievements


    // Use this for initialization
    void Awake ()
    {
        // TODO set Player prefab for player spawning
        switch (PlayersVRType) {
            case VirtualRealityType.None:
                PlayerTypeLoc = "Prefabs/Player/FPS_Player";
                break;
            case VirtualRealityType.Oculus:
                PlayerTypeLoc = "Prefabs/Player/Oculus_Player";
                break;
            case VirtualRealityType.SteamVR:
                PlayerTypeLoc = "Prefabs/Player/Steam_VR_Player";
                break;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		// TODO catch escape key and call pause game
	}

    public void BeginTutorial()
    {
        Maze = new GameObject();
        // TODO make the tutorial a collection of maze nodes and ladders
        //floor 1
        MazeNode[,] tutorial1 = new MazeNode[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                tutorial1[i, j] = new MazeNode(j, i);
                tutorial1[i, j].Floor = -4;
            }

        tutorial1[2, 0].AddEdge(tutorial1[1, 0]);
        tutorial1[1, 0].AddEdge(tutorial1[0, 0]);
        tutorial1[0, 0].AddEdge(tutorial1[0, 1]);
        tutorial1[0, 1].AddEdge(tutorial1[1, 1]);
        tutorial1[1, 1].AddEdge(tutorial1[2, 1]);
        tutorial1[2, 1].AddEdge(tutorial1[2, 2]);
        tutorial1[2, 2].AddEdge(tutorial1[1, 2]);
        tutorial1[1, 2].AddEdge(tutorial1[0, 2]);

        tutorial1[0, 2].actor = ActorType.Ladder;

        //floor 2
        MazeNode[,] tutorial2 = new MazeNode[6, 5];
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                tutorial2[i, j] = new MazeNode(j, i);
                tutorial2[i, j].Floor = -3;
            }

        tutorial2[0, 0].AddEdge(tutorial2[1, 0]);
        tutorial2[1, 0].AddEdge(tutorial2[2, 0]);
        tutorial2[2, 0].AddEdge(tutorial2[2, 1]);
        tutorial2[2, 1].AddEdge(tutorial2[1, 1]);
        tutorial2[2, 1].AddEdge(tutorial2[1, 1]);
        tutorial2[1, 1].AddEdge(tutorial2[0, 1]);
        tutorial2[0, 1].AddEdge(tutorial2[0, 2]);
        tutorial2[0, 2].AddEdge(tutorial2[1, 2]);
        tutorial2[1, 2].AddEdge(tutorial2[1, 3]);
        tutorial2[1, 3].AddEdge(tutorial2[0, 3]);
        tutorial2[0, 3].AddEdge(tutorial2[0, 4]);
        tutorial2[1, 3].AddEdge(tutorial2[1, 4]);
        tutorial2[1, 4].AddEdge(tutorial2[2, 4]);
        tutorial2[1, 3].AddEdge(tutorial2[2, 3]);
        tutorial2[2, 3].AddEdge(tutorial2[3, 3]);
        tutorial2[2, 1].AddEdge(tutorial2[2, 2]);
        tutorial2[2, 2].AddEdge(tutorial2[3, 2]);
        tutorial2[3, 2].AddEdge(tutorial2[3, 1]);
        tutorial2[3, 1].AddEdge(tutorial2[3, 0]);
        tutorial2[3, 0].AddEdge(tutorial2[4, 0]);
        tutorial2[4, 0].AddEdge(tutorial2[5, 0]);
        tutorial2[5, 0].AddEdge(tutorial2[5, 1]);
        tutorial2[5, 1].AddEdge(tutorial2[4, 1]);
        tutorial2[4, 1].AddEdge(tutorial2[4, 2]);
        tutorial2[4, 2].AddEdge(tutorial2[4, 3]);
        tutorial2[4, 3].AddEdge(tutorial2[4, 4]);
        tutorial2[4, 4].AddEdge(tutorial2[3, 4]);
        tutorial2[4, 3].AddEdge(tutorial2[5, 3]);
        tutorial2[5, 3].AddEdge(tutorial2[5, 4]);
        tutorial2[5, 3].AddEdge(tutorial2[5, 2]);

        tutorial2[0, 0].actor = ActorType.Ladder;
        tutorial2[5, 2].actor = ActorType.Ladder;

        //floor 3
        MazeNode[,] tutorial3 = new MazeNode[6, 5];
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                tutorial3[i, j] = new MazeNode(j, i);
                tutorial3[i, j].Floor = -2;
            }

        tutorial3[0, 0].AddEdge(tutorial3[0, 1]);
        tutorial3[0, 1].AddEdge(tutorial3[1, 1]);
        tutorial3[1, 1].AddEdge(tutorial3[2, 1]);
        tutorial3[2, 1].AddEdge(tutorial3[3, 1]);
        tutorial3[3, 1].AddEdge(tutorial3[3, 2]);
        tutorial3[3, 2].AddEdge(tutorial3[3, 3]);
        tutorial3[3, 3].AddEdge(tutorial3[3, 4]);
        tutorial3[3, 4].AddEdge(tutorial3[2, 4]);
        tutorial3[2, 4].AddEdge(tutorial3[1, 4]);
        tutorial3[1, 4].AddEdge(tutorial3[0, 4]);
        tutorial3[0, 4].AddEdge(tutorial3[0, 3]);
        tutorial3[0, 3].AddEdge(tutorial3[1, 3]);
        tutorial3[1, 3].AddEdge(tutorial3[2, 3]);
        tutorial3[2, 3].AddEdge(tutorial3[2, 2]);
        tutorial3[2, 2].AddEdge(tutorial3[1, 2]);
        tutorial3[1, 2].AddEdge(tutorial3[0, 2]);
        tutorial3[2, 1].AddEdge(tutorial3[2, 0]);
        tutorial3[2, 0].AddEdge(tutorial3[1, 0]);
        tutorial3[2, 0].AddEdge(tutorial3[3, 0]);
        tutorial3[3, 0].AddEdge(tutorial3[4, 0]);
        tutorial3[4, 0].AddEdge(tutorial3[4, 1]);
        tutorial3[4, 1].AddEdge(tutorial3[4, 2]);
        tutorial3[4, 2].AddEdge(tutorial3[4, 3]);
        tutorial3[4, 1].AddEdge(tutorial3[5, 1]);
        tutorial3[5, 1].AddEdge(tutorial3[5, 0]);
        tutorial3[5, 1].AddEdge(tutorial3[5, 2]);
        tutorial3[5, 2].AddEdge(tutorial3[5, 3]);
        tutorial3[5, 3].AddEdge(tutorial3[5, 4]);
        tutorial3[5, 4].AddEdge(tutorial3[4, 4]);

        //tutorial3[0, 1].actor = ActorType.Oni;
        //tutorial3[5, 0].actor = ActorType.Oni;
        //tutorial3[5, 3].actor = ActorType.Oni;
        //tutorial3[0, 3].actor = ActorType.Oni;
        //tutorial3[1, 1].actor = ActorType.Spike_Trap;
        //tutorial3[2, 0].actor = ActorType.Spike_Trap;
        //tutorial3[1, 3].actor = ActorType.Spike_Trap;

        tutorial3[0, 2].actor = ActorType.Ladder;
        tutorial3[4, 4].actor = ActorType.Ladder;

        //floor 4
        tutorial4 = new MazeNode[7, 7];
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 7; j++)
            {
                tutorial4[i, j] = new MazeNode(j, i);
                tutorial4[i, j].Floor = -1;
            }

        tutorial4[3, 3].AddEdge(tutorial4[3, 2]);
        tutorial4[3, 2].AddEdge(tutorial4[3, 1]);
        tutorial4[3, 1].AddEdge(tutorial4[2, 1]);
        tutorial4[2, 1].AddEdge(tutorial4[1, 1]);
        tutorial4[1, 1].AddEdge(tutorial4[0, 1]);
        tutorial4[0, 1].AddEdge(tutorial4[0, 0]);
        tutorial4[0, 0].AddEdge(tutorial4[1, 0]);
        tutorial4[3, 3].AddEdge(tutorial4[3, 4]);
        tutorial4[3, 4].AddEdge(tutorial4[3, 5]);
        tutorial4[3, 5].AddEdge(tutorial4[2, 5]);
        tutorial4[3, 3].AddEdge(tutorial4[2, 3]);
        tutorial4[2, 3].AddEdge(tutorial4[1, 3]);
        tutorial4[1, 3].AddEdge(tutorial4[0, 3]);
        tutorial4[0, 3].AddEdge(tutorial4[0, 2]);
        tutorial4[0, 2].AddEdge(tutorial4[1, 2]);
        tutorial4[1, 2].AddEdge(tutorial4[2, 2]);
        tutorial4[0, 3].AddEdge(tutorial4[0, 4]);
        tutorial4[0, 4].AddEdge(tutorial4[1, 4]);
        tutorial4[1, 4].AddEdge(tutorial4[2, 4]);
        tutorial4[3, 3].AddEdge(tutorial4[4, 3]);
        tutorial4[4, 3].AddEdge(tutorial4[5, 3]);
        tutorial4[5, 3].AddEdge(tutorial4[6, 3]);
        tutorial4[6, 3].AddEdge(tutorial4[6, 4]);
        tutorial4[6, 4].AddEdge(tutorial4[6, 5]);
        tutorial4[6, 3].AddEdge(tutorial4[6, 2]);
        tutorial4[6, 2].AddEdge(tutorial4[6, 1]);
        tutorial4[5, 3].AddEdge(tutorial4[5, 2]);
        tutorial4[5, 2].AddEdge(tutorial4[5, 1]);
        tutorial4[5, 1].AddEdge(tutorial4[5, 0]);
        tutorial4[5, 0].AddEdge(tutorial4[6, 0]);
        tutorial4[6, 2].AddEdge(tutorial4[6, 1]);
        tutorial4[5, 0].AddEdge(tutorial4[4, 0]);
        tutorial4[4, 0].AddEdge(tutorial4[4, 1]);
        tutorial4[4, 1].AddEdge(tutorial4[4, 2]);
        tutorial4[4, 0].AddEdge(tutorial4[3, 0]);
        tutorial4[3, 0].AddEdge(tutorial4[2, 0]);
        tutorial4[5, 3].AddEdge(tutorial4[5, 4]);
        tutorial4[5, 4].AddEdge(tutorial4[5, 5]);
        tutorial4[5, 4].AddEdge(tutorial4[4, 4]);
        tutorial4[4, 4].AddEdge(tutorial4[4, 5]);
        tutorial4[4, 5].AddEdge(tutorial4[4, 6]);
        tutorial4[4, 6].AddEdge(tutorial4[5, 6]);
        tutorial4[5, 6].AddEdge(tutorial4[6, 6]);
        tutorial4[4, 6].AddEdge(tutorial4[3, 6]);
        tutorial4[3, 6].AddEdge(tutorial4[2, 6]);
        tutorial4[2, 6].AddEdge(tutorial4[1, 6]);
        tutorial4[1, 6].AddEdge(tutorial4[0, 6]);
        tutorial4[0, 6].AddEdge(tutorial4[0, 5]);
        tutorial4[0, 5].AddEdge(tutorial4[1, 5]);

        //tutorial4[1, 0].actor = ActorType.Ofuda_Pickup;
        //tutorial4[2, 0].actor = ActorType.Ofuda_Pickup;
        //tutorial4[0, 3].actor = ActorType.Ofuda_Pickup;
        //tutorial4[2, 4].actor = ActorType.Oni;
        //tutorial4[4, 2].actor = ActorType.Oni;
        //tutorial4[6, 6].actor = ActorType.Oni;
        //tutorial4[6, 1].actor = ActorType.Spike_Trap;
        //tutorial4[6, 4].actor = ActorType.Spike_Trap;

        tutorial4[3, 3].actor = ActorType.Ladder;
        tutorial4[1, 5].actor = ActorType.Ladder;

        MazeGenerator.connectLadders(tutorial1[0, 2], tutorial2[0, 0]);
        MazeGenerator.connectLadders(tutorial2[5, 2], tutorial3[0, 2]);
        MazeGenerator.connectLadders(tutorial3[4, 4], tutorial4[3, 3]);

        BeginPlay();

        for (int i = 0; i < 4; i++)
        {
            MazeNode[,] TutorialFloor = new MazeNode[7, 7];
            List<MazeNode> nodes;
            MazeSection section = new MazeSection();
            int[] sectionIDs = new int[4];
            switch (i)
            {
                case 0:
                    TutorialFloor = tutorial1;
                    break;
                case 1:
                    TutorialFloor = tutorial2;
                    break;
                case 2:
                    TutorialFloor = tutorial3;
                    break;
                case 3:
                    TutorialFloor = tutorial4;
                    break;
            }

            section.Root = TutorialFloor[0, 0];
            int sectionID = AnalyticsManager.AddSection(lvlID, 0, i);
            section.SectionID = sectionID;
            section.Spawned = false;
            foreach (MazeNode n in MazeGenerator.nodesInSection(section.Root))
                n.SectionID = section.SectionID;
            //Sections.Add(section);
            nodes = MazeGenerator.nodesInSection(section.Root);
            foreach (MazeNode n in nodes)
            {
                AnalyticsManager.AddCell(section.SectionID, n.Col, n.Row);
            }
            SpawnSection(section);
        }

        Vector3 location = new Vector3(20, -119, 8);
        PlayerObj = Instantiate(Resources.Load(PlayerTypeLoc), location, tutorial1[0, 0].GetRotation()) as GameObject;
    }

    public void BeginPlay()
    {
        if(DebugOn)
            Maze = new GameObject();
        Sections = new List<MazeSection>();
        // Start new Session in Analytics
        // Generate Level
        // Add Level to Analytics
        // Add Sections to Analytics
        // Add Cells to Analytics
        generator = new MazeGenerator();
        generator.GenerateMaze(dif);

        if (!DebugOn)
        {
            MazeGenerator.connectLadders(tutorial4[1, 5], MazeGenerator.DifferentSections[0, 0]);
        }

        lvlID = AnalyticsManager.AddLevel(MazeGenerator.Seed, (int) dif);
        SessionID = AnalyticsManager.AddSession(lvlID, (int) PlayersVRType);
        int[,] sectionIDs = new int[5,8];
        MazeNode[,] roots = MazeGenerator.DifferentSections;
        List<MazeNode> nodes;

        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 8; j++)
                if (roots[i, j] != null)
                { 
                    MazeSection section = new MazeSection();
                    section.Root = roots[i, j];
                    sectionIDs[i, j] = AnalyticsManager.AddSection(lvlID, i, j);
                    section.SectionID = sectionIDs[i, j];
                    section.Spawned = false;
                    foreach (MazeNode n in MazeGenerator.nodesInSection(roots[i, j]))
                        n.SectionID = section.SectionID;
                    Sections.Add(section);
                }

        for(int i = 0; i < 5; i++)
            for(int j = 0; j < 5; j++)
                if (roots[i, j] != null)
                {
                    nodes = MazeGenerator.nodesInSection(roots[i, j]);
                    foreach (MazeNode n in nodes)
                    {
                        AnalyticsManager.AddCell(sectionIDs[i, j], n.Col, n.Row);
                    }
                }

        // Spawn first section
        foreach(MazeSection s in Sections)
            if(s.Root.Col == 0 && s.Root.Row == 0 && s.Root.Floor == 0)
                SpawnSection(s);
        // Spawn Player

        if (DebugOn)
        {
            Vector3 location = new Vector3(8, 1, 8);
            PlayerObj = Instantiate(Resources.Load(PlayerTypeLoc), location, roots[0, 0].GetRotation()) as GameObject;
        }
    }

    public void setSeed(string newSeed)
    {
        int temp = int.Parse(newSeed);
        MazeGenerator.Seed = temp;
    }

    public static void SpawnSection(MazeSection section)
    {
        NavMeshSurface surface;
        PlayersCurrentSection = section;
        GameObject SectionObject = new GameObject();
        SectionObject.transform.parent = Maze.transform;
        // Spawn Cells
        section.Spawned = true;
        section.section = SectionObject;
        foreach(MazeNode n in MazeGenerator.nodesInSection(section.Root))
        {
            SpawnPiece(n, SectionObject);
            SpawnActor(n, SectionObject);
        }

        surface = section.section.AddComponent<NavMeshSurface>();
        if (surface != null)
            surface.BuildNavMesh();
        // Spawn Actors
        // Add Actors to Analytics
    }

    private static int piecesSpawned;

    public static void SpawnPiece(MazeNode node, GameObject section)
    {
        Vector3 location = new Vector3(node.Col * 6 + 8, node.Floor * 30, node.Row * 6 + 8);

        GameObject obj = Instantiate(Resources.Load(node.GetPrefabName()), location, node.GetRotation()) as GameObject;
        obj.transform.parent = section.transform;

        obj = Instantiate(Resources.Load("Prefabs/Level/CellLog"), location, node.GetRotation()) as GameObject;
        obj.transform.parent = section.transform;
        CellLog cellLog = obj.GetComponent<CellLog>();
        cellLog.Row = node.Row;
        cellLog.Col = node.Col;
        
        if (DebugLabelsOn)
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

    public static void SpawnActor(MazeNode node, GameObject section)
    {
        GameObject actorObject;
        Vector3 location = new Vector3(node.Col * 6 + 8, node.Floor * 30, node.Row * 6 + 8);
        if (node.actor != ActorType.Null)
        {
            actorObject = Instantiate(Actors.Prefabs[node.actor], location, node.GetRotation());
            actorObject.transform.parent = section.transform;
            if (node.actor == ActorType.Ladder)
            {
                node.ladder = actorObject;
                node.ladder.GetComponent<Ladder>().SectionID = node.SectionID;
                node.ladder.GetComponent<Ladder>().ConnectedLadderNode = node.ladderMazeNode;
            }
            actorObject.AddComponent<Actor>().ActorID = AnalyticsManager.AddActor(SessionID, node.actor);
        }
    }
    // TODO Add an Actor Component to each actor GameObject

    public void EnterSection(GameObject ladder)
    {
        // disable current section
        ladder.transform.parent.gameObject.SetActive(false);
        // check if next section is spawned
        // spawn section
        MazeSection sec = new MazeSection();
        int sID = ladder.GetComponent<Ladder>().ConnectedLadder.GetComponent<Ladder>().SectionID;
        foreach(MazeSection s in Sections)
            if (s.SectionID == sID)
                sec = s;
        if (sec.Spawned == false)
        {
            SpawnSection(sec);
        }
        // else
            // enable section
        else
        {
            ladder.GetComponent<Ladder>().ConnectedLadder.transform.parent.gameObject.SetActive(true);
            PlayersCurrentSection = sec;
        }
        // TODO change Ladder code to call this and add player movement to here
    }

    public void PauseGame()
    {
        PlayersCurrentSection.section.SetActive(false);
    }

    public void UnPause()
    {
        PlayersCurrentSection.section.SetActive(true);
    }

    public void GameOver()
    {

    }

    public void OnApplicationPause(bool pause)
    {
        if(pause)
            PauseGame();
    }

    public void ActorVisitedCell(Actor actor, int cellID)
    {
        if(AnalyticsEnabled)
            AnalyticsManager.EnteredCell(actor.ActorID, cellID);
    }

    public void ActorKilled(Actor killer, Actor dead)
    {
        if(AnalyticsEnabled)
            AnalyticsManager.ActorKilled(killer.ActorID, dead.ActorID);
    }

    public void ActorStateChange(Actor actor, int state)
    {
        if(AnalyticsEnabled)
            AnalyticsManager.ActorStateChange(actor.ActorID, state);
    }

    public int UsedItem(ItemType item)
    {
        if (AnalyticsEnabled)
            return AnalyticsManager.UsedItem(SessionID, item);
        else
            return -1;
    }

    public void OfudaHit(int eventID, Actor actor)
    {
        if (AnalyticsEnabled)
            AnalyticsManager.OfudaHit(eventID, actor.ActorID);
    }

    public void MarkDrawn(int eventID, List<LineRenderer> lines)
    {
        if (AnalyticsEnabled)
            foreach (LineRenderer line in lines)
            {
                AnalyticsManager.ChalkLine(eventID, line);
            }
    }

    public void FoundItem(Actor actor)
    {
        if (AnalyticsEnabled)
            AnalyticsManager.FoundItem(actor.ActorID);
    }
}

public enum Difficulty
{
    Small,
    Medium,
    Large,
    Excessive,
    AlreadyLost,
    JustWhy
}
