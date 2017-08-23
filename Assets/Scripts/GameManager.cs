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

    // Achievements


	// Use this for initialization
	void Awake ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {
		
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
        analytics.EnteredCell(actor.ActorID, cellID);
    }

    public void ActorKilled(Actor killer, Actor dead)
    {
        analytics.ActorKilled(killer.ActorID, dead.ActorID);
    }

    public void ActorStateChange(Actor actor, int state)
    {
        analytics.ActorStateChange(actor.ActorID, state);
    }

    public int UsedItem(ItemType item)
    {
        return analytics.UsedItem(SessionID, item);
    }

    public void OfudaHit(int eventID, Actor actor)
    {
        analytics.OfudaHit(eventID, actor.ActorID);
    }

    public void MarkDrawn(int eventID, List<LineRenderer> lines)
    {
        foreach (LineRenderer line in lines)
        {
            analytics.ChalkLine(eventID, line);
        }
    }

    public void FoundItem(Actor actor)
    {
        analytics.FoundItem(actor.ActorID);
    }


    // 
}
