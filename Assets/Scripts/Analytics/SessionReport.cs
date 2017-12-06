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
    int ActorID;
    ActorType Type;
    // TODO add state changes here
    // TODO add death here?
}

public struct ChalkDrawing
{
    int EventID;
    List<ChalkLine> Lines;
}

public struct ChalkLine
{
    int LineID;
    List<ChalkPoint> Points;
}

public struct ChalkPoint
{
    int Index;
    float x;
    float y;
    float z;
}

public struct Death
{
    int DeadActorID;
    int KillerActorID;
    float TimeDied;
}

public struct ItemUse
{
    int EventID;
    ItemType item;
    float TimeUsed;
}

public struct itemFound
{
    int ActorID;
    float TimeFound;
}

public struct OfudaHit
{
    int EventID;
    int ActorID;
    float HitTime;
}

public struct StateChange
{
    int ActorID;
    int State;
    float TimeChanged;
}

public struct CellVisit
{
    int ActorID;
    int CellID;
    float VisitTime;
}