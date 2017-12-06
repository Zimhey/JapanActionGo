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

    public static List<SessionReport> Sessions;

    public static void PrintReports()
    {
        Sessions = new List<SessionReport>();
        AnalyticsManager.Select("Sessions", GetSessions);
        Debug.Log("Sessions: " + Sessions.Count);
        foreach (SessionReport sp in Sessions)
            Debug.Log("SessionID: " + sp.SessionID);
    }

    public static int GetSessions(SqliteDataReader reader)
    {
        Debug.Log("Getting Sessions");
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

}
