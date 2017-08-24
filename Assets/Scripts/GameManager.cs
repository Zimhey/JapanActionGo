using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    int SectionID;
    MazeNode Root;
    bool Spawned;
    List<GameObject> Actors;
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


    // Achievements


	// Use this for initialization
	void Awake ()
    {
        // TODO set Player prefab for player spawning
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
        // Start new Session in Analytics
        // Generate Level
        // Add Level to Analytics
        // Add Sections to Analytics
        // Add Cells to Analytics
        int lvlID = analytics.AddLevel(Difficulty.Small);
        int secID = analytics.AddSection(lvlID);
        int sesID = analytics.AddSession(lvlID);

        // Spawn First Section
        // Spawn Player

    }

    public void SpawnSection(MazeSection section)
    {
        // Spawn Cells
        // Add C
        // Spawn Actors
        // Add Actors to Analytics
    }

    // TODO Move Spawn Piece here
    // TODO Move Spawn Actor Here
    // TODO Add an Actor Component to each actor GameObject

    public void EnterSection(GameObject ladder)
    {
        // disable current section
        // check if next section is spawned
            // spawn section
        // else
            // enable section
        // TODO change Ladder code to call this and add player movement to here
    }

    public void PauseGame()
    {
        // disable current section
    }

    public void UnPause()
    {
        // enable current section
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


    // 
}
