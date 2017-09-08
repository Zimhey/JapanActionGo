using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data;
using Mono.Data.Sqlite;
using UnityEngine;



public class AnalyticsManager : MonoBehaviour
{
    private static string dbName = "Analytics/GameAnalytics.sqlite";
    private static string ConnectionStr;
    private static bool initialized = false;

    public static string DatabaseName
    {
        get
        {
            return dbName;
        }
        set
        {
            dbName = value;
            init();
        }
    }

    public void Start()
    {
        init();

        /*
         // Tests
        // AddLevel
        int levelID = AddLevel(2323, (int)Difficulty.Small);
        // AddCells
        int numCells = 3;
        int[] cellIDs = new int[numCells];

        for(int i = 0; i < numCells; i++)
        {
            int x = AddSection(levelID, i, 1);
            cellIDs[i] = AddCell(x, i, i);
        }
        // AddSession
        ses = AddSession(levelID, 0);

        // AddActors
        int a = AddActor(ses, ActorType.Oni);
        int b = AddActor(ses, ActorType.Ofuda_Pickup);

        ActorStateChange(a, (int)OniState.Chase);

        ActorKilled(a, b);

        EnteredCell(a, cellIDs[0]);
        FoundItem(b);
        int u = UsedItem(ses, ItemType.Chalk);
        OfudaHit(u, a);
     
        */
    }

    public void Update()
    {
        // Update Session PlayTime;
        // Tests UpdateSessionTime(ses);
    }

    private static void init()
    {
        initialized = true;
        ConnectionStr = "Data Source=" + dbName + ";Version=3;";
        CreateDatabase();
    }

    private static string[] tables =
    {
        "CREATE TABLE `Levels` ( `LevelID` INTEGER, `Seed` INTEGER, `Difficulty` INTEGER, PRIMARY KEY(`LevelID`));",
        "CREATE TABLE `Sections` ( `SectionID` INTEGER, `LevelID` INTEGER, `Index` INTEGER, `Floor` INTEGER, PRIMARY KEY(`SectionID`));",
        "CREATE TABLE `Cells` ( `SectionID` INTEGER, `CellID` INTEGER, `CellRow` INTEGER, `CellCol` INTEGER, PRIMARY KEY(`CellID`));",
        "CREATE TABLE `Sessions` ( `SessionID` INTEGER, `PlayTime` REAL, `LevelID` INTEGER, `VRType` INTEGER, PRIMARY KEY(`SessionID`));",
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

    private static void CreateDatabase()
    {
        string directoryPath = System.IO.Path.GetDirectoryName(dbName);

        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        if (!System.IO.File.Exists(dbName))
        {
            SqliteConnection.CreateFile(dbName);
            //SqliteConnection.ClearAllPools();
            foreach (string sql in tables)
            {
                SimpleQuery(sql);
            }
        }
    }

    public static void SimpleQuery(string query)
    {
        SqliteConnection con;
        SqliteCommand cmd;

        if (!initialized)
            init();

        using (con = new SqliteConnection(ConnectionStr))
        {
            con.Open();
            using (cmd = new SqliteCommand(query, con))
                cmd.ExecuteNonQuery();
            con.Close();
        }
    }

    public static object ReturnSecondQuery(string query1, string query2)
    {
        SqliteConnection con;
        SqliteCommand cmd;
        object o = null;

        if (!initialized)
            init();

        using (con = new SqliteConnection(ConnectionStr))
        {
            con.Open();
            using (cmd = new SqliteCommand(query1, con))
                cmd.ExecuteNonQuery();
            using (cmd = new SqliteCommand(query2, con))
            {
                SqliteDataReader reader = cmd.ExecuteReader();
                o = reader.GetValue(0); // This is ugly, why isn't Convert available?
                reader.Close();
            }
            con.Close();
        }

        return o;
    }

    public static int ReturnSecondQueryAsInt(string query1, string query2)
    {
        object o = ReturnSecondQuery(query1, query2);
        if (o != null)
            return int.Parse(o.ToString());
        return -1;
    }

    public static int AddLevel(int seed, int difficulty)
    {
        string insert = "INSERT INTO `Levels` (`Seed`, `Difficulty`)" +
                        " SELECT " + seed + ", " + difficulty + 
            " WHERE NOT EXISTS (SELECT 1 FROM `Levels` WHERE `Seed` = " + seed + " AND `Difficulty` = " +
            difficulty + ");";
        string getID = "SELECT `LevelID` FROM `Levels` WHERE `Seed` = " + seed + " AND `Difficulty` = " +
                        difficulty + ";";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public static int AddSection(int levelID, int index, int floor)
    {
        string insert = "INSERT INTO `Sections` (`LevelID`, `Index`, `Floor`) SELECT " + levelID + ", " +
            index + ", " + floor + " WHERE NOT EXISTS (SELECT 1 FROM `Sections` WHERE `LevelID` = " +
            levelID + " AND `Index` = " + index + " AND `Floor` = " + floor + ");";
        string getID = "SELECT `SectionID` FROM `Sections` WHERE `LevelID` = " + levelID +" AND `Index` = " +
            index + " AND `Floor` = " + floor + ";";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public static int AddCell(int SectionID, int row, int col)
    {
        string insert = "INSERT INTO `Cells` (`SectionID`, `CellRow`, `CellCol`)" +
            " SELECT " + SectionID + ", " + row + ", " + col +
            " WHERE NOT EXISTS (SELECT 1 FROM `Cells` WHERE `SectionID` = " + SectionID + " AND `CellRow` = " +
            row + " AND `CellCol` = " + col + ");";
        string getID = "SELECT `CellID` FROM `Cells` WHERE `SectionID` = " + SectionID + " AND `CellRow` = " +
            row + " AND `CellCol` = " + col + ";";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public static int AddSession(int levelID, int VRType)
    {
        string insert = "INSERT INTO `Sessions` (`LevelID`, `VRType`) VALUES (" + levelID + ", " + VRType + ");";
        string getID = "SELECT last_insert_rowid()";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public static void UpdateSessionTime(int sessionID, float time)
    {
        string update = "UPDATE `Sessions` SET `PlayTime` = " + time + " WHERE `SessionID` = " + sessionID + ";";
        SimpleQuery(update);
    }

    public static int AddActor(int sessionID, ActorType type)
    {
        string insert = "INSERT INTO `Actors` (`SessionID`, `ActorType`) VALUES (" + sessionID + ", " + (int)type + ");";
        string getID = "SELECT last_insert_rowid()";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public static void ActorKilled(int deadActorID, int killerID)
    {
        string insert = "INSERT INTO `Deaths` (`DeadActorID`, `KillerActorID`, `TimeDied`) VALUES (" + deadActorID + ", " + killerID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public static void EnteredCell(int actorID, int cellID)
    {
        string insert = "INSERT INTO `VisitedCells` (`ActorID`, `CellID`, `VisitTime`) VALUES (" + actorID + ", " + cellID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public static void FoundItem(int actorID)
    {
        string insert = "INSERT INTO `ItemsFound` (`ActorID`, `TimeFound`) VALUES (" + actorID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public static int UsedItem(int sessionID, ItemType item)
    {
        string insert = "INSERT INTO `ItemUses` (`SessionID`, `ItemType`, `TimeUsed`) VALUES (" + sessionID + ", " + (int)item + ", " + Time.time + ");";
        string getID = "SELECT last_insert_rowid()";
        return ReturnSecondQueryAsInt(insert, getID);
    }

    public static void OfudaHit(int eventID, int actorID)
    {
        string insert = "INSERT INTO `OfudaHits` (`eventID`, `ActorID`, `HitTime`) VALUES (" + eventID + ", " + actorID + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

    public static void ChalkLine(int eventID, LineRenderer line)
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

    public static void ActorStateChange(int actorID, int state)
    {
        string insert = "INSERT INTO `StateChanges` (`ActorID`, `State`, `TimeChanged`) VALUES (" + actorID + ", " + state + ", " + Time.time + ");";
        SimpleQuery(insert);
    }

}
