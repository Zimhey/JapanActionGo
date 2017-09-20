using System;

public class SessionReports
{
	public SessionReports()
	{
	}

    public void printReports()
    {
        String actorID = "SELECT 'ActorID' FROM 'Actors' WHERE 'ActorType' = " + ActorType.Player + ";";
        int actorIDint = AnalyticsManager.ReturnQueryAsInt(actorID);
        String visitedCells = "SELECT 'CellID' FROM 'VisitedCells' WHERE 'ActorID' = " + actorIDint + ";";
        String chalkUses = "SELECT 'TimeUsed' FROM 'ItemUses' WHERE 'ItemType' = " + (int)ActorType.chalk + ";";
        String ofudaUses = "SELECT 'TimeUsed' FROM 'ItemUses' WHERE 'ItemType' = " + (int)ActorType.ofuda + ";";
        String ofudaHits = "SELECT 'HitTime' From 'OfudaHits';";
    }
}
