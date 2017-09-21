using System;

public class GameSessionReports
{
	public GameSessionReports()
	{
	}

    public void printReports()
    {
        String actorID = "SELECT 'ActorID' FROM 'Actors' WHERE 'ActorType' = " + ActorType.Player + ";";
        int actorIDint = AnalyticsManager.ReturnQueryAsInt(actorID);
        String visitedCells = "SELECT 'CellID' FROM 'VisitedCells' WHERE 'ActorID' = " + actorIDint + ";";
        String chalkUses = "SELECT 'TimeUsed' FROM 'ItemUses' WHERE 'ItemType' = " + (int)ActorType.Chalk_Mark + ";";
        String ofudaUses = "SELECT 'TimeUsed' FROM 'ItemUses' WHERE 'ItemType' = " + (int)ActorType.Ofuda_Projectile + ";";
        String ofudaHits = "SELECT 'HitTime' From 'OfudaHits';";
    }
}
