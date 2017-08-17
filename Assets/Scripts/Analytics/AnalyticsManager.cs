using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public enum ActorType
{
    Player,
    Oni,
    Taka_Nyudo,
    Okuri_Inu,
    Spike_Trap,
    Crush_Trap,
    Pit_Trap,
    Dart_Trap,
    Dart_Projectile,
    Tripwire,
    Lantern_Trap,
    Chalk_Pickup,
    Ofuda_Pickup,
    Mirror_Pickup,
    Compass_Pickup,
    PressurePlate,
    Lever,
    Ladder
}

public class AnalyticsManager : MonoBehaviour
{
    public string DBName = "Analytics/GameAnalytics.sqlite";
    private string ConnectionStr;
    private int ses;

    public void Start()
    {
        ConnectionStr = "Data Source=" + DBName + ";Version=3;";
        //CreateDatabase();

        /* // Tests
        // AddLevel
        int levelID = AddLevel(2323, 20, 20, 2, 5);
        // AddCells
        int numCells = 3;
        int[] cellIDs = new int[numCells];

        for(int i = 0; i < numCells; i++)
        {
            cellIDs[i] = AddCell(levelID, i, i);
        }
        // AddSession
        ses = AddSession(levelID);


        // AddActors
        int a = AddActor(ses, ActorType.Oni);
        int b = AddActor(ses, ActorType.Ofuda_Pickup);

        ActorStateChange(a, (int)onistate.Chase);

        ActorKilled(a, b);

        EnteredCell(a, cellIDs[0]);
        FoundItem(b);
        int u = UsedItem(ses, ItemType.Chalk);
        OfudaHit(u, a);
       GameObject chalkPrefab = Resources.Load("Prefabs/ChalkMark") as GameObject;
        GameObject chalkMark = Instantiate(chalkPrefab);
        LineRenderer l = chalkMark.GetComponentInChildren<LineRenderer>();
        l.positionCount = 15;
        ChalkLine(u, l);
        */
    }

    public void Update()
    {
        // Update Session PlayTime;
        // Tests UpdateSessionTime(ses);
    }

    private string[] tables =
    {
        "CREATE TABLE `Levels` ( `LevelID` INTEGER, `Seed` INTEGER, `MazeRows` INTEGER, `MazeCols` INTEGER, `MazeSections` INTEGER, `MazeLoops` INTEGER, PRIMARY KEY(`LevelID`));",
        "CREATE TABLE `Cells` ( `LevelID` INTEGER, `CellID` INTEGER, `CellRow` INTEGER, `CellCol` INTEGER, PRIMARY KEY(`CellID`));",
        "CREATE TABLE `Sessions` ( `SessionID` INTEGER, `PlayTime` REAL, `LevelID` INTEGER, PRIMARY KEY(`SessionID`));",
        "CREATE TABLE `Actors` ( `ActorID` INTEGER, `SessionID` INTEGER, `ActorType` INTEGER, PRIMARY KEY(`ActorID`));",
        "CREATE TABLE `Deaths` ( `DeadActorID` INTEGER, `KillerActorID` INTEGER, `TimeDied` REAL, PRIMARY KEY(`DeadActorID`));",
        "CREATE TABLE `VisitedCells` ( `EventID` INTEGER, `ActorID` INTEGER, `CellID` INTEGER, `VisitTime` REAL, PRIMARY KEY(`EventID`));",
        "CREATE TABLE `ItemsFound` ( `ActorID` INTEGER, `TimeFound` REAL, PRIMARY KEY(`ActorID`));",
        "CREATE TABLE `ItemUses` ( `EventID` INTEGER, `SessionID` INTEGER, `ItemType` INTEGER, `TimeUsed` REAL, PRIMARY KEY(`EventID`));",
        "CREATE TABLE `OfudaHits` ( `EventID` INTEGER, `ActorID` INTEGER, `HitTime` REAL, PRIMARY KEY(`EventID`));",
        "CREATE TABLE `ChalkLines` ( `EventID` INTEGER, `LineID` INTEGER, PRIMARY KEY(`LineID`));",
        "CREATE TABLE `ChalkPoints` ( `LineID` INTEGER, `PointID` INTEGER, `Index` INTEGER, `x` INTEGER, `y` INTEGER, `z` INTEGER, PRIMARY KEY(`PointID`));",
        "CREATE TABLE `StateChanges` ( `EventID` INTEGER, `ActorID` INTEGER, `State` INTEGER, `TimeChanged`	REAL, PRIMARY KEY(`EventID`));"
    };

    public void CreateDatabase()
    {
        SqliteConnection.CreateFile(DBName);
        foreach (string sql in tables)
        {
            SimpleQuery(sql);
        }
    }

    public void SimpleQuery(string query)
    {
        SqliteConnection con;
        SqliteCommand cmd;
        using (con = new SqliteConnection(ConnectionStr))
        {
            con.Open();
            using (cmd = new SqliteCommand(query, con))
                cmd.ExecuteNonQuery();
            con.Close();
        }
    }

    public object ReturnSecondQuery(string query1, string query2)
    {
        SqliteConnection con;
        SqliteCommand cmd;
        object o = null;

        using (con = new SqliteConnection(ConnectionStr))
        {
            con.Open();
            using (cmd = new SqliteCommand(query1, con))
                cmd.ExecuteNonQuery();
            using (cmd = new SqliteCommand(query2, con))
            {
                SqliteDataReader reader = cmd.ExecuteReader();
                o = reader.GetValue(0); // This is ugly, why isn't Convert available?
            }
            con.Close();
        }

        return o;
    }

    public int ReturnSecondQueryAsInt(string query1, string query2)
    {
        object o = ReturnSecondQuery(query1, query2);
        if (o != null)
            return int.Parse(o.ToString());
        return -1;
    }

    public int AddLevel(int seed, int rows, int columns, int sections, int loops)
    {
        string insert = "INSERT INTO `Levels` (`Seed`, `MazeRows`, `MazeCols`, `MazeSections`, `MazeLoops`)" +
            " SELECT " + seed + ", " + rows + ", " + columns + ", " + sections + ", " + loops +
            " WHERE NOT EXISTS (SELECT 1 FROM `Levels` WHERE `Seed` = " + seed + " AND `MazeRows` = " +
            rows + " AND `MazeCols` = " + columns + " AND `MazeSections` = " + sections +
            " AND `MazeLoops` = " + loops + ");";
        string getID = "SELECT `LevelID` FROM `Levels` WHERE `Seed` = " + seed + " AND `MazeRows` = " +
            rows + " AND `MazeCols` = " + columns + " AND `MazeSections` = " + sections +
            " AND `MazeLoops` = " + loops + ";";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public int AddCell(int levelID, int row, int col)
    {
        string insert = "INSERT INTO `Cells` (`LevelID`, `CellRow`, `CellCol`)" +
            " SELECT " + levelID + ", " + row + ", " + col +
            " WHERE NOT EXISTS (SELECT 1 FROM `Cells` WHERE `LevelID` = " + levelID + " AND `CellRow` = " +
            row + " AND `CellCol` = " + col + ");";
        string getID = "SELECT `CellID` FROM `Cells` WHERE `LevelID` = " + levelID + " AND `CellRow` = " +
            row + " AND `CellCol` = " + col + ";";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public int AddSession(int levelID)
    {
        string insert = "INSERT INTO `Sessions` (`LevelID`) VALUES (" + levelID + ");";
        string getID = "SELECT last_insert_rowid()";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public void UpdateSessionTime(int sessionID)
    {
        string update = "UPDATE `Sessions` SET `PlayTime` = " + Time.time + " WHERE `SessionID` = " + sessionID + ";";
        SimpleQuery(update);
    }

    public int AddActor(int sessionID, ActorType type)
    {
        string insert = "INSERT INTO `Actors` (`SessionID`, `ActorType`) VALUES (" + sessionID + ", " + (int)type + ");";
        string getID = "SELECT last_insert_rowid()";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public void ActorKilled(int deadActorID, int killerID)
    {
        string insert = "INSERT INTO `Deaths` (`DeadActorID`, `KillerActorID`, `TimeDied`) VALUES (" + deadActorID + ", " + killerID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public void EnteredCell(int actorID, int cellID)
    {
        string insert = "INSERT INTO `VisitedCells` (`ActorID`, `CellID`, `VisitTime`) VALUES (" + actorID + ", " + cellID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public void FoundItem(int actorID)
    {
        string insert = "INSERT INTO `ItemsFound` (`ActorID`, `TimeFound`) VALUES (" + actorID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public int UsedItem(int sessionID, ItemType item)
    {
        string insert = "INSERT INTO `ItemUses` (`SessionID`, `ItemType`, `TimeUsed`) VALUES (" + sessionID + ", " + (int)item + ", " + Time.time + ");";
        string getID = "SELECT last_insert_rowid()";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public void OfudaHit(int eventID, int actorID)
    {
        string insert = "INSERT INTO `OfudaHits` (`eventID`, `ActorID`, `HitTime`) VALUES (" + eventID + ", " + actorID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public void ChalkLine(int eventID, LineRenderer line)
    {
        string insertLine = "INSERT INTO `ChalkLines` (`EventID`) VALUES (" + eventID + ");";
        string getID = "SELECT last_insert_rowid()";
        int lineID = ReturnSecondQueryAsInt(insertLine, getID);

        for(int i = 0; i < line.positionCount; i++)
        {
            Vector3 point = line.GetPosition(i);
            string insertPoint = "INSERT INTO `ChalkPoints` (`LineID`, `Index`, `x`, `y`, `z`) VALUES (" + lineID + ", " + i + ", " + point.x + ", " + point.y + ", " + point.z + ");";
            SimpleQuery(insertPoint);
        }
    }

    public void ActorStateChange(int actorID, int state)
    {
        string insert = "INSERT INTO `StateChanges` (`ActorID`, `State`, `TimeChanged`) VALUES (" + actorID + ", " + state + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

}
