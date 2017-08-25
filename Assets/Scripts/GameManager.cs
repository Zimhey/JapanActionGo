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

    public GameObject Maze;

    public bool DebugLabelsOn;

    public Difficulty dif = Difficulty.Small;

    public int SessionID;

    public GameState CurrentState;
    private GameState prevState;

    public VirtualRealityType PlayersVRType;

    // Collection of every section on every floor
    public List<MazeSection> Sections;

    public MazeSection PlayersCurrentSection;

    // Analytics

    private AnalyticsManager analytics;
    public bool AnalyticsEnabled = false;

    private void Start()
    {
        if(DebugOn)
            BeginPlay();
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
        // TODO make the tutorial a collection of maze nodes and ladders
    }

    public void BeginPlay()
    {
        Maze = new GameObject();
        // Start new Session in Analytics
        // Generate Level
        // Add Level to Analytics
        // Add Sections to Analytics
        // Add Cells to Analytics
        MazeGenerator generator = new MazeGenerator();
        generator.GenerateMaze(dif);
        
        int lvlID = analytics.AddLevel(dif);
        SessionID = analytics.AddSession(lvlID);
        int[,] sectionIDs = new int[5,8];
        MazeNode[,] roots = MazeGenerator.DifferentSections;
        List<MazeNode> nodes;

        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 8; j++)
                if (roots[i, j] != null)
                { 
                    MazeSection section = new MazeSection();
                    section.Root = roots[i, j];
                    sectionIDs[i, j] = analytics.AddSection(lvlID, i);
                    section.SectionID = sectionIDs[i, j];
                    section.Spawned = false;
                    Sections.Add(section);
                }

        for(int i = 0; i < 5; i++)
            for(int j = 0; j < 5; j++)
                if (roots[i, j] != null)
                {
                    nodes = MazeGenerator.nodesInSection(roots[i, j]);
                    foreach (MazeNode n in nodes)
                    {
                        analytics.AddCell(sectionIDs[i, j], n.Col, n.Row);
                        if(n.actor == ActorType.Ladder)
                        {
                            n.ladder.GetComponent<Ladder>().SectionID = sectionIDs[i, j];
                        }
                    }
                }

        // Spawn first section
        foreach(MazeSection s in Sections)
            if(s.Root.Col == 0 && s.Root.Row == 0)
                SpawnSection(s);
        // Spawn Player
        Vector3 location = new Vector3(8, 1, 8);
        PlayerObj = Instantiate(Resources.Load(PlayerTypeLoc), location, roots[0, 0].GetRotation()) as GameObject;
        
    }

    public void SpawnSection(MazeSection section)
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

    public void SpawnPiece(MazeNode node, GameObject section)
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

    public void SpawnActor(MazeNode node, GameObject section)
    {
        GameObject actorObject;
        Vector3 location = new Vector3(node.Col * 6 + 8, node.Floor * 30, node.Row * 6 + 8);
        if (node.actor != ActorType.Null)
        {
            actorObject = Instantiate(Actors.Prefabs[node.actor], location, node.GetRotation());
            actorObject.transform.parent = section.transform;
            if (node.actor == ActorType.Ladder)
            {
                //node.ladder = actorObject;
            }
            actorObject.AddComponent<Actor>().ActorID = analytics.AddActor(SessionID, node.actor);
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
            analytics.EnteredCell(actor.ActorID, cellID);
    }

    public void ActorKilled(Actor killer, Actor dead)
    {
        if(AnalyticsEnabled)
            analytics.ActorKilled(killer.ActorID, dead.ActorID);
    }

    public void ActorStateChange(Actor actor, int state)
    {
        if(AnalyticsEnabled)
            analytics.ActorStateChange(actor.ActorID, state);
    }

    public int UsedItem(ItemType item)
    {
        if (AnalyticsEnabled)
            return analytics.UsedItem(SessionID, item);
        else
            return -1;
    }

    public void OfudaHit(int eventID, Actor actor)
    {
        if (AnalyticsEnabled)
            analytics.OfudaHit(eventID, actor.ActorID);
    }

    public void MarkDrawn(int eventID, List<LineRenderer> lines)
    {
        if (AnalyticsEnabled)
            foreach (LineRenderer line in lines)
            {
                analytics.ChalkLine(eventID, line);
            }
    }

    public void FoundItem(Actor actor)
    {
        if (AnalyticsEnabled)
            analytics.FoundItem(actor.ActorID);
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
