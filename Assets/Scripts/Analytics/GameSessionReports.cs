using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionReports
{
	public GameSessionReports()
	{
	}

    public void RunExcessQueries()
    {

    }

    public static Dictionary<int, LevelReport> Levels;
    public static List<Section> Sections;
    public static List<Cell> Cells;

    public static List<SessionReport> Sessions;
    public static List<ActorMap> AllActors;
    public static List<Death> AllDeaths;
    public static List<ItemUse> ItemsUsed;
    public static List<itemFound> ItemsFound;
    public static List<OfudaHit> OfudaHits;
    public static List<StateChange> AllStateChanges;
    public static List<CellVisit> CellsVisited;



    public static SessionReport CurrentSession;

    public static void PrintReports()
    {
        Sessions = new List<SessionReport>();
        AllActors = new List<ActorMap>();
        AnalyticsManager.Select("Sessions", GetSessions);
        AnalyticsManager.Select("Actors", GetActors);

        Debug.Log("Number of Sessions: " + Sessions.Count);
        foreach (SessionReport sp in Sessions)
        {
            CurrentSession = sp;

        }
    }

    public static int GetSessions(SqliteDataReader reader)
    {
        while(reader.Read())
        {
            SessionReport sp = new SessionReport();
            sp.SessionID = reader.GetInt32(0);
            float? playTime = reader.GetValue(1) as float?;
            if (playTime.HasValue)
                sp.PlayTime = playTime.Value;
            sp.LevelID = reader.GetInt32(2);
            sp.VRType = reader.GetInt32(3);
            Sessions.Add(sp);
        }
        return 0;
    }

    public static int GetActors(SqliteDataReader reader)
    {
        while(reader.Read())
        {
            ActorMap a = new ActorMap();
            a.ActorID = reader.GetInt32(0);
            a.SessionID = reader.GetInt32(1);
            a.Type = (ActorType)reader.GetInt32(2);
            AllActors.Add(a);
        }
        return 0;
    }

    public static int GetDeaths(SqliteDataReader reader)
    {
        while (reader.Read())
        {
            Death d = new Death();
            d.DeadActorID = reader.GetInt32(0);
            d.KillerActorID = reader.GetInt32(1);
            float? deathTime = reader.GetValue(2) as float?;
            if (deathTime.HasValue)
                d.TimeDied = deathTime.Value;
            AllDeaths.Add(d);
        }
        return 0;
    }

    public static int GetItemUses(SqliteDataReader reader)
    {
        while (reader.Read())
        {
            ItemUse item = new ItemUse();
            item.EventID = reader.GetInt32(0);
            item.SessionID = reader.GetInt32(1);
            item.item = (ItemType)reader.GetInt32(2);
            float? timeUsed = reader.GetValue(2) as float?;
            if (timeUsed.HasValue)
                item.TimeUsed = timeUsed.Value;
            ItemsUsed.Add(item);
        }
        return 0;
    }

    public static int GetItemsFound(SqliteDataReader reader)
    {
        while (reader.Read())
        {
            itemFound item = new itemFound();
            item.ActorID = reader.GetInt32(0);
            float? timeFound = reader.GetValue(2) as float?;
            if (timeFound.HasValue)
                item.TimeFound = timeFound.Value;
            ItemsFound.Add(item);
        }
        return 0;
    }

}
