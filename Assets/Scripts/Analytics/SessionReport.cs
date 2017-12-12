using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SessionReport
{
    // session data
    public int SessionID;
    public float PlayTime;
    public int LevelID;
    public int VRType;

    List<ActorMap> Actors;
    List<CellVisit> VisitedCells;
    List<ChalkDrawing> Drawings;
    List<Death> Deaths;
    List<ItemUse> ItemUses;
    List<itemFound> ItemsFound;
    List<OfudaHit> OfudaHits;
    List<StateChange> StateChanges;
}

public struct ActorMap
{
    public int SessionID;
    public int ActorID;
    public ActorType Type;
    public List<StateChange> StateChanges;
    public Death MyDeath;
    public List<Death> Kills;
}

public struct ChalkDrawing
{
    public int EventID;
    public List<ChalkLine> Lines;
}

public struct ChalkLine
{
    public int LineID;
    public List<ChalkPoint> Points;
}

public struct ChalkPoint
{
    public int Index;
    public float x;
    public float y;
    public float z;
}

public struct Death
{
    public int DeadActorID;
    public int KillerActorID;
    public float TimeDied;
}

public struct ItemUse
{
    public int SessionID;
    public int EventID;
    public ItemType item;
    public float TimeUsed;
}

public struct itemFound
{
    public int ActorID;
    public float TimeFound;
}

public struct OfudaHit
{
    public int EventID;
    public int ActorID;
    public float HitTime;
}

public struct StateChange
{
    public int ActorID;
    public int State;
    public float TimeChanged;
}

public struct CellVisit
{
    public int ActorID;
    public int CellID;
    public float VisitTime;
}
