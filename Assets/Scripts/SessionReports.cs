using System;

public class SessionReports
{
	public SessionReports()
	{
	}

    public void printReports()
    {
        String actorID = "SELECT 'ActorID' FROM 'Actors' WHERE 'ActorType' = " + ActorType.Player + ";";
        int actorIDint = AnalyticsManager.ReturnQuery(actorID);
    }
}