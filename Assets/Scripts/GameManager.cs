using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.AI;

public enum GameState
{
    Intro,
    Main,
    Play,
    Pause,
    GameOver,
    Win
}

public enum VirtualRealityType
{
    None,
    SteamVR,
    Oculus,
    Vive,
    Cardboard // lol maybe
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
            if (instance == null)
            {
                instance = new GameManager();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    public static GameObject PlayerObj;

    public string PlayerTypeLoc;

    public bool TutorialOn;
    public bool DebugLabelsOn;
    public bool DebugPlay;
    public bool CanPause;
    public Dictionary<int, MazeNode[,]>[] storedMaps = new Dictionary<int, MazeNode[,]>[6];
    public Color[] lanternColors = { Color.red, Color.magenta, Color.blue, Color.green, Color.yellow };
    public Color sectionLanternColor;
    System.Random rand;
    System.Random messageRand;

    private GameObject parent;

    public GameObject GameParent
    {
        get
        {
            if (parent == null)
                parent = new GameObject("Game Objects");
            return parent;
        }
    }

    public GameObject Maze;

    public static Difficulty difficulty;

    public int SessionID;

    private float sessionTime;
    private float lastUpdateTime;

    public float SessionTime
    {
        get
        {
            return sessionTime;
        }
        set
        {
            sessionTime = value;
            if (sessionTime < 1)
                lastUpdateTime = sessionTime;

            if (AnalyticsEnabled && sessionTime > lastUpdateTime + 1000) // only update once a second
            {
                lastUpdateTime = sessionTime;
                AnalyticsManager.UpdateSessionTime(SessionID, sessionTime);
            }
        }
    }

    private static GameState prevState;
    private static GameState currState;

    public static GameState CurrentState
    {
        get
        {
            return currState;
        }
        set
        {
            if(value == GameState.Play)
                AnalyticsManager.PauseThread();
            else
                AnalyticsManager.ResumeThread();
            prevState = currState;
            currState = value;
        }
    }

    public Color getRandomColor()
    {
        return lanternColors[(int) rand.Next(0, 5)];
    }

    public VirtualRealityType PlayersVRType;

    // Collection of every section on every floor
    public static List<MazeSection> Sections;

    public static MazeSection PlayersCurrentSection;

    public int lvlID;

    private MazeNode[,] tutorial4;

    // Analytics
    public bool AnalyticsEnabled;

    private static UIGameManagerInterface ui;

    public static UIGameManagerInterface UserInterface
    {
        get
        {
            if (ui == null)
                ui = GameObject.FindGameObjectWithTag("UserInterface").GetComponentInChildren<UIGameManagerInterface>();
            return ui;
        }
    }

    // Achievements

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Attempted to spawn a second GameManager, DON'T DO THIS, THIS IS BAD, IT IS A SINGLETON");
            Debug.Log("GameManager killing itself");
            Destroy(gameObject);
            return;
        }

        if (UnityEngine.XR.XRDevice.isPresent)
            PlayersVRType = VirtualRealityType.SteamVR;
        else
        {
            PlayersVRType = VirtualRealityType.None;
        }

        switch (PlayersVRType) {
            case VirtualRealityType.None:
                PlayerTypeLoc = "Prefabs/Player/FPS_Player";
                break;
            case VirtualRealityType.SteamVR:
                PlayerTypeLoc = "Prefabs/Player/SteamVR_Player";
                break;
        }
    }

    void Start()
    {
        difficulty = Difficulty.Medium;

        if (DebugPlay)
        {
            CurrentState = GameState.Play;
            AnalyticsEnabled = false;
        }

        for(int i = 0; i < 6; i++)
        {
            storedMaps[i] = new Dictionary<int, MazeNode[,]>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO catch escape key and call pause game    
        if (Input.GetKeyDown("escape") && CanPause)
        {
            if (CurrentState == GameState.Pause)
                UnPause();
            else if (CurrentState == GameState.Play)
                PauseGame();
        }

        // update session time during play
        if (CurrentState == GameState.Play)
        {
            SessionTime += Time.deltaTime;
        }
    }

    public void SetDifficulty(Difficulty diff)
    {
        difficulty = diff;
    }

    public void EnableTutorial(bool tutorialEnabled)
    {
        TutorialOn = tutorialEnabled;
    }

    // TODO why is state being stored in MazeGenerator
    public void SetSeed(int newSeed)
    {
        MazeGenerator.Seed = newSeed;
    }

    public void StartGame()
    {
        if (TutorialOn)
            BeginTutorial();
        else
            BeginPlay();

        CurrentState = GameState.Play;
        SessionTime = 0;
    }

    public void BeginTutorial()
    {
        Maze = new GameObject("Maze");
        Maze.transform.parent = GameParent.transform;

        //floor 1
        MazeNode[,] tutorial1 = TutorialGenerator.GenerateFloor1();

        //floor 2
        MazeNode[,] tutorial2 = TutorialGenerator.GenerateFloor2();

        //floor 3
        MazeNode[,] tutorial3 = TutorialGenerator.GenerateFloor3();

        //floor 4
        tutorial4 = TutorialGenerator.GenerateFloor4();

        tutorial1[0, 2].AddLadderTo(tutorial2[0, 0]);
        tutorial2[5, 2].AddLadderTo(tutorial3[0, 2]);
        tutorial3[4, 4].AddLadderTo(tutorial4[3, 3]);

        BeginPlay();

        for (int i = 0; i < 4; i++)
        {
            MazeNode[,] TutorialFloor = new MazeNode[7, 7];
            List<MazeNode> nodes;
            MazeSection section = new MazeSection();
            //int[] sectionIDs = new int[4];
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
            int sectionID = AnalyticsManager.AddSection(lvlID, 0, -4 + i);
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
        GameObject.FindGameObjectWithTag("Player").AddComponent<Actor>().ActorID = AnalyticsManager.AddActor(SessionID, ActorType.Player);
        PlayerObj.transform.parent = GameParent.transform;
    }

    public void BeginPlay()
    {
        rand = new System.Random(MazeGenerator.Seed);
        messageRand = new System.Random(MazeGenerator.Seed);
        if (!TutorialOn)
        {
            Maze = new GameObject("Maze");
            Maze.transform.parent = GameParent.transform;
        }

        Sections = new List<MazeSection>();
        // Start new Session in Analytics
        // Generate Level
        // Add Level to Analytics
        // Add Sections to Analytics
        // Add Cells to Analytics
        Dictionary<int, MazeNode[,]> storedDifficultyMaps = storedMaps[(int)difficulty];
        print(storedDifficultyMaps);
        if (storedDifficultyMaps.ContainsKey(MazeGenerator.Seed))
        {
            MazeGenerator.DifferentSections = storedMaps[(int)difficulty][MazeGenerator.Seed];
            MazeGenerator.connectLadderNodes(difficulty, MazeGenerator.DifferentSections);
        }
        
        else
        {
            MazeGenerator.GenerateMaze(difficulty);
            storedMaps[(int)difficulty].Add(MazeGenerator.Seed, MazeGenerator.DifferentSections);
        }

        print("Something");

        if (TutorialOn)
        {
            tutorial4[1, 5].AddLadderTo(MazeGenerator.DifferentSections[0, 0]);
        }

        lvlID = AnalyticsManager.AddLevel(MazeGenerator.Seed, (int)difficulty);
        SessionID = AnalyticsManager.AddSession(lvlID, (int)PlayersVRType);
        int[,] sectionIDs = new int[5, 8];
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

        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
                if (roots[i, j] != null)
                {
                    nodes = MazeGenerator.nodesInSection(roots[i, j]);
                    foreach (MazeNode n in nodes)
                    {
                        AnalyticsManager.AddCell(sectionIDs[i, j], n.Col, n.Row);
                    }
                }

        // Spawn first section
        foreach (MazeSection s in Sections)
            if (s.Root.Col == 0 && s.Root.Row == 0 && s.Root.Floor == 0)
            {
                SpawnSection(s);
            }

        // Spawn Player

        if (!TutorialOn)
        {
            Vector3 location = new Vector3(8, 1, 8);
            PlayerObj = Instantiate(Resources.Load(PlayerTypeLoc), location, roots[0, 0].GetRotation()) as GameObject;
            PlayerObj.AddComponent<Actor>().ActorID = AnalyticsManager.AddActor(SessionID, ActorType.Player);
            PlayerObj.transform.parent = GameParent.transform;
        }
    }

    public void SpawnSection(MazeSection msection)
    {
        sectionLanternColor = getRandomColor();
        NavMeshSurface surface;
        GameObject SectionObject = new GameObject("Section " + msection.SectionID);
        SectionObject.transform.parent = Maze.transform;
        msection.Spawned = true;
        msection.section = SectionObject;
        if (!TutorialOn || msection.Root.Floor > 0 || msection.Root.Floor == -4)
        {
            PlayersCurrentSection = msection;
        }

        GameObject cells = new GameObject("Cells");
        cells.transform.parent = SectionObject.transform;
        GameObject actors = new GameObject("Actors");
        actors.transform.parent = SectionObject.transform;
        GameObject lanterns = new GameObject("Lanterns");
        lanterns.transform.parent = SectionObject.transform;
        GameObject messages = new GameObject("Messages");
        messages.transform.parent = SectionObject.transform;

        foreach (MazeNode n in MazeGenerator.nodesInSection(msection.Root))
            SpawnPiece(n, cells);

        surface = msection.section.AddComponent<NavMeshSurface>();
        if (surface != null)
            surface.BuildNavMesh();

        foreach (MazeNode n in MazeGenerator.nodesInSection(msection.Root))
        {
            SpawnActor(n, actors);
            SpawnLantern(n, lanterns);
            SpawnMessage(n, messages);
        }
        // Spawn Actors
        // Add Actors to Analytics
    }

    private static int piecesSpawned;

    public void SpawnPiece(MazeNode node, GameObject cells)
    {
        Vector3 location = new Vector3(node.Col * 6 + 8, node.Floor * 30, node.Row * 6 + 8);

        GameObject obj = Instantiate(Resources.Load(node.GetPrefabName()), location, node.GetRotation()) as GameObject;
        obj.transform.parent = cells.transform;
        node.floorPrefab = obj.transform.GetChild(0).gameObject;
        obj = Instantiate(Resources.Load("Prefabs/Level/CellLog"), location, node.GetRotation()) as GameObject;
        obj.transform.parent = cells.transform;
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

    public void SpawnActor(MazeNode node, GameObject actors)
    {
        GameObject actorObject;
        Vector3 location = new Vector3(node.Col * 6 + 8, node.Floor * 30, node.Row * 6 + 8);
        if (node.actor != ActorType.Null)
        {
            if (node.actor == ActorType.Okuri_Inu || node.actor == ActorType.Oni || node.actor == ActorType.Taka_Nyudo)
            {
                //print("Column: " + node.Col + " Row: " + node.Row);
                node.EnemyPathNode = true;
            }
            if (node.actor == ActorType.Pit_Trap)// || node.actor == ActorType.Spike_Trap)
            {
                node.floorPrefab.SetActive(false);
            }
            
            if (node.actor == ActorType.Ladder)
            {
                if(node.ladderMazeNode.Floor > node.Floor)
                    actorObject = Instantiate(Resources.Load("Prefabs/Level/LadderUp"), location, node.GetRotation()) as GameObject;
                else
                    actorObject = Instantiate(Resources.Load("Prefabs/Level/LadderDown"), location, node.GetRotation()) as GameObject;
            }
            
            else
                actorObject = Instantiate(Actors.Prefabs[node.actor], location, node.GetRotation());

            actorObject.transform.parent = actors.transform;
            if (node.actor == ActorType.Ladder)
            {
                node.ladder = actorObject;
                node.ladder.GetComponent<Ladder>().SectionID = node.SectionID;
                node.ladder.GetComponent<Ladder>().ConnectedLadderNode = node.ladderMazeNode;
                node.ladder.GetComponent<Ladder>().location = node;
            }
            actorObject.AddComponent<Actor>().ActorID = AnalyticsManager.AddActor(SessionID, node.actor);
            actorObject.GetComponent<Actor>().type = node.actor;
        }
    }
    // TODO Add an Actor Component to each actor GameObject

    public void SpawnLantern(MazeNode node, GameObject lanterns)
    {
        GameObject lantern;
        Vector3 loc = new Vector3(node.Col * 6 + 8, (float) (node.Floor * 30 + 6.5), node.Row * 6 + 8);
        if ((node.Col + node.Row) % 2 == 0)
        {
            lantern = Instantiate(Resources.Load("Prefabs/Level/Lantern"), loc, node.GetRotation()) as GameObject;
            lantern.transform.GetChild(1).GetComponent<Light>().color = sectionLanternColor;
            lantern.transform.parent = lanterns.transform;
        }
    }

    public void SpawnMessage(MazeNode node, GameObject messages)
    {
        int messageType = (int) messageRand.Next(0, 3) + 1;
        GameObject message;
        Vector3 loc = new Vector3(node.Col * 6 + 8, (float)(node.Floor * 30 + 3), node.Row * 6 + 8);
        if(node.MessageNode)
        {
            message = Instantiate(Resources.Load("Prefabs/Messages/Message" + messageType.ToString()), loc, node.GetRotation()) as GameObject;
            message.transform.parent = messages.transform;
        }
    }

    public void EnterSection(GameObject ladder, GameObject player)
    {
        if (ladder.GetComponent<Ladder>().ConnectedLadder == null)
        {
            foreach (MazeSection sec in GameManager.Sections)
            {
                if (sec.SectionID == ladder.GetComponent<Ladder>().ConnectedLadderNode.SectionID)
                {
                    if (!sec.Spawned)
                        SpawnSection(sec);
                }
            }

            ladder.GetComponent<Ladder>().ConnectedLadder = ladder.GetComponent<Ladder>().ConnectedLadderNode.ladder;
            ladder.GetComponent<Ladder>().ConnectedLadder.GetComponent<Ladder>().ConnectedLadder = ladder;
        }

        //Debug.Log(collider.gameObject.tag + " entered Cell R: " + Row + " C: " + Col + " at Time: " + Time.time);
        //ladder.GetComponent<Ladder>().ConnectedLadder.transform.parent.gameObject.SetActive(true);

        ladder.GetComponent<Ladder>().ConnectedLadder.transform.parent.gameObject.transform.parent.gameObject.SetActive(true);
        ladder.GetComponent<Ladder>().ConnectedLadder.GetComponent<Ladder>().teleportable = true;

        if (ladder.GetComponent<Ladder>().teleportable == true && ladder.GetComponent<Ladder>().ConnectedLadder.GetComponent<Ladder>().teleportable == true)
        {
            if (!(ladder.GetComponent<Ladder>().ConnectedLadderNode.Floor == -1 && ladder.GetComponent<Ladder>().ConnectedLadderNode.Col == 1 && ladder.GetComponent<Ladder>().ConnectedLadderNode.Row == 5))
            {
                ladder.GetComponent<Ladder>().teleportable = false;
                ladder.GetComponent<Ladder>().ConnectedLadder.GetComponent<Ladder>().teleportable = false;
                player.transform.position = ladder.GetComponent<Ladder>().ConnectedLadder.transform.position;
                ladder.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
                foreach (MazeSection sec in GameManager.Sections)
                {
                    if (sec.SectionID == ladder.GetComponent<Ladder>().ConnectedLadderNode.SectionID)
                    {
                        PlayersCurrentSection = sec;
                    }
                }
                PlayersCurrentSection.section = ladder.GetComponent<Ladder>().ConnectedLadder.transform.parent.gameObject.transform.parent.gameObject;
            }
        }

        else
            ladder.GetComponent<Ladder>().ConnectedLadder.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
        // TODO change Ladder code to call this and add player movement to here
    }

    private void SetCurrentActive(bool state)
    {
        if (!DebugPlay)
        {
            PlayersCurrentSection.section.SetActive(state);
            PlayerObj.SetActive(state);
        }
    }

    public void PauseGame()
    {
        SetCurrentActive(false);
        CurrentState = GameState.Pause;
        if (UserInterface != null)
            UserInterface.ShowPauseMenu();
        else
            Debug.Log("UI is null");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UnPause()
    {
        SetCurrentActive(true);
        CurrentState = GameState.Play;
        if (UserInterface != null)
            UserInterface.ShowHUD();
        else
            Debug.Log("UI is null");
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    public void GameOver()
    {
        if (DebugPlay)
            return;
        SetCurrentActive(false);
        CurrentState = GameState.GameOver;
        if (UserInterface != null)
            UserInterface.ShowGameOverMenu();
        else
            Debug.Log("UI is null");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Win()
    {
        SetCurrentActive(false);
        CurrentState = GameState.Win;
        if (UserInterface != null)
            UserInterface.ShowWinMenu();
        else
            Debug.Log("UI is null");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void MainMenu()
    {
        CurrentState = GameState.Main;
        Destroy(GameParent);
    }

    public void OnApplicationPause(bool pause)
    {
        if (pause)
            PauseGame();
    }

    public void ActorVisitedCell(Actor actor, int cellID)
    {
        if (AnalyticsEnabled)
            AnalyticsManager.EnteredCell(actor.ActorID, cellID);
    }

    public void ActorKilled(Actor killer, Actor dead)
    {
        if (AnalyticsEnabled)
            AnalyticsManager.ActorKilled(killer.ActorID, dead.ActorID);
    }

    public void ActorStateChange(Actor actor, int state)
    {
        if (AnalyticsEnabled)
            AnalyticsManager.ActorStateChange(actor.ActorID, state);
    }

    public int UsedItem(ItemType item)
    {
        if (AnalyticsEnabled)
            return AnalyticsManager.UsedItem(SessionID, item);
        else
            return -1;
    }

    public static bool trapNode(MazeNode node)
    {
        return node.actor == ActorType.Crush_Trap || node.actor == ActorType.Pit_Trap || node.actor == ActorType.Spike_Trap;
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

    public static string PathToString(LinkedList<MazeNode> nodes)
    {
        string path = "";
        foreach (MazeNode n in nodes)
        {
            path += "Col: " + n.Col + " Row: " + n.Row + " ";
        }
        return path;
    }
}
