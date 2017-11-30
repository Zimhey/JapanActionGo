using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TutorialGenerator {

    public static MazeNode[,] GenerateFloor1()
    {
        MazeNode[,] tutorial1 = new MazeNode[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                tutorial1[i, j] = new MazeNode(j, i);
                tutorial1[i, j].Floor = -4;
            }

        tutorial1[2, 0].AddEdge(tutorial1[1, 0]);
        tutorial1[1, 0].AddEdge(tutorial1[0, 0]);
        tutorial1[0, 0].AddEdge(tutorial1[0, 1]);
        tutorial1[0, 1].AddEdge(tutorial1[1, 1]);
        tutorial1[1, 1].AddEdge(tutorial1[2, 1]);
        tutorial1[2, 1].AddEdge(tutorial1[2, 2]);
        tutorial1[2, 2].AddEdge(tutorial1[1, 2]);
        tutorial1[1, 2].AddEdge(tutorial1[0, 2]);

        tutorial1[0, 2].actor = ActorType.Ladder;
        MazeGenerator.tutorialSections[0] = tutorial1[2, 0];

        return tutorial1;
    }

    public static MazeNode[,] GenerateFloor2()
    {
        MazeNode[,] tutorial2 = new MazeNode[6, 5];
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                tutorial2[i, j] = new MazeNode(j, i);
                tutorial2[i, j].Floor = -3;
            }

        tutorial2[0, 0].AddEdge(tutorial2[1, 0]);
        tutorial2[1, 0].AddEdge(tutorial2[2, 0]);
        tutorial2[2, 0].AddEdge(tutorial2[2, 1]);
        tutorial2[2, 1].AddEdge(tutorial2[1, 1]);
        tutorial2[2, 1].AddEdge(tutorial2[1, 1]);
        tutorial2[1, 1].AddEdge(tutorial2[0, 1]);
        tutorial2[0, 1].AddEdge(tutorial2[0, 2]);
        tutorial2[0, 2].AddEdge(tutorial2[1, 2]);
        tutorial2[1, 2].AddEdge(tutorial2[1, 3]);
        tutorial2[1, 3].AddEdge(tutorial2[0, 3]);
        tutorial2[0, 3].AddEdge(tutorial2[0, 4]);
        tutorial2[1, 3].AddEdge(tutorial2[1, 4]);
        tutorial2[1, 4].AddEdge(tutorial2[2, 4]);
        tutorial2[1, 3].AddEdge(tutorial2[2, 3]);
        tutorial2[2, 3].AddEdge(tutorial2[3, 3]);
        tutorial2[2, 1].AddEdge(tutorial2[2, 2]);
        tutorial2[2, 2].AddEdge(tutorial2[3, 2]);
        tutorial2[3, 2].AddEdge(tutorial2[3, 1]);
        tutorial2[3, 1].AddEdge(tutorial2[3, 0]);
        tutorial2[3, 0].AddEdge(tutorial2[4, 0]);
        tutorial2[4, 0].AddEdge(tutorial2[5, 0]);
        tutorial2[5, 0].AddEdge(tutorial2[5, 1]);
        tutorial2[5, 1].AddEdge(tutorial2[4, 1]);
        tutorial2[4, 1].AddEdge(tutorial2[4, 2]);
        tutorial2[4, 2].AddEdge(tutorial2[4, 3]);
        tutorial2[4, 3].AddEdge(tutorial2[4, 4]);
        tutorial2[4, 4].AddEdge(tutorial2[3, 4]);
        tutorial2[4, 3].AddEdge(tutorial2[5, 3]);
        tutorial2[5, 3].AddEdge(tutorial2[5, 4]);
        tutorial2[5, 3].AddEdge(tutorial2[5, 2]);

        tutorial2[0, 0].actor = ActorType.Ladder;
        tutorial2[5, 2].actor = ActorType.Ladder;
        MazeGenerator.tutorialSections[1] = tutorial2[0, 0];

        return tutorial2;
    }

    public static MazeNode[,] GenerateFloor3()
    {
        MazeNode[,] tutorial3 = new MazeNode[6, 5];
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                tutorial3[i, j] = new MazeNode(j, i);
                tutorial3[i, j].Floor = -2;
            }

        tutorial3[0, 0].AddEdge(tutorial3[0, 1]);
        tutorial3[0, 1].AddEdge(tutorial3[1, 1]);
        tutorial3[1, 1].AddEdge(tutorial3[2, 1]);
        tutorial3[2, 1].AddEdge(tutorial3[3, 1]);
        tutorial3[3, 1].AddEdge(tutorial3[3, 2]);
        tutorial3[3, 2].AddEdge(tutorial3[3, 3]);
        tutorial3[3, 3].AddEdge(tutorial3[3, 4]);
        tutorial3[3, 4].AddEdge(tutorial3[2, 4]);
        tutorial3[2, 4].AddEdge(tutorial3[1, 4]);
        tutorial3[1, 4].AddEdge(tutorial3[0, 4]);
        tutorial3[0, 4].AddEdge(tutorial3[0, 3]);
        tutorial3[0, 3].AddEdge(tutorial3[1, 3]);
        tutorial3[1, 3].AddEdge(tutorial3[2, 3]);
        tutorial3[2, 3].AddEdge(tutorial3[2, 2]);
        tutorial3[2, 2].AddEdge(tutorial3[1, 2]);
        tutorial3[1, 2].AddEdge(tutorial3[0, 2]);
        tutorial3[2, 1].AddEdge(tutorial3[2, 0]);
        tutorial3[2, 0].AddEdge(tutorial3[1, 0]);
        tutorial3[2, 0].AddEdge(tutorial3[3, 0]);
        tutorial3[3, 0].AddEdge(tutorial3[4, 0]);
        tutorial3[4, 0].AddEdge(tutorial3[4, 1]);
        tutorial3[4, 1].AddEdge(tutorial3[4, 2]);
        tutorial3[4, 2].AddEdge(tutorial3[4, 3]);
        tutorial3[4, 1].AddEdge(tutorial3[5, 1]);
        tutorial3[5, 1].AddEdge(tutorial3[5, 0]);
        tutorial3[5, 1].AddEdge(tutorial3[5, 2]);
        tutorial3[5, 2].AddEdge(tutorial3[5, 3]);
        tutorial3[5, 3].AddEdge(tutorial3[5, 4]);
        tutorial3[5, 4].AddEdge(tutorial3[4, 4]);

        tutorial3[0, 1].actor = ActorType.Oni;
        tutorial3[4, 3].actor = ActorType.Oni;
        tutorial3[5, 3].actor = ActorType.Oni;
        tutorial3[0, 3].actor = ActorType.Oni;
        tutorial3[1, 1].actor = ActorType.Spike_Trap;
        tutorial3[2, 0].actor = ActorType.Spike_Trap;
        tutorial3[1, 3].actor = ActorType.Spike_Trap;

        tutorial3[0, 2].actor = ActorType.Ladder;
        tutorial3[4, 4].actor = ActorType.Ladder;
        MazeGenerator.tutorialSections[2] = tutorial3[0, 2];

        return tutorial3;
    }

    public static MazeNode[,] GenerateFloor4()
    {
        MazeNode[,] tutorial4 = new MazeNode[7, 7];
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 7; j++)
            {
                tutorial4[i, j] = new MazeNode(j, i);
                tutorial4[i, j].Floor = -1;
            }

        tutorial4[3, 3].AddEdge(tutorial4[3, 2]);
        tutorial4[3, 2].AddEdge(tutorial4[3, 1]);
        tutorial4[3, 1].AddEdge(tutorial4[2, 1]);
        tutorial4[2, 1].AddEdge(tutorial4[1, 1]);
        tutorial4[1, 1].AddEdge(tutorial4[0, 1]);
        tutorial4[0, 1].AddEdge(tutorial4[0, 0]);
        tutorial4[0, 0].AddEdge(tutorial4[1, 0]);
        tutorial4[3, 3].AddEdge(tutorial4[3, 4]);
        tutorial4[3, 4].AddEdge(tutorial4[3, 5]);
        tutorial4[3, 5].AddEdge(tutorial4[2, 5]);
        tutorial4[3, 3].AddEdge(tutorial4[2, 3]);
        tutorial4[2, 3].AddEdge(tutorial4[1, 3]);
        tutorial4[1, 3].AddEdge(tutorial4[0, 3]);
        tutorial4[0, 3].AddEdge(tutorial4[0, 2]);
        tutorial4[0, 2].AddEdge(tutorial4[1, 2]);
        tutorial4[1, 2].AddEdge(tutorial4[2, 2]);
        tutorial4[0, 3].AddEdge(tutorial4[0, 4]);
        tutorial4[0, 4].AddEdge(tutorial4[1, 4]);
        tutorial4[1, 4].AddEdge(tutorial4[2, 4]);
        tutorial4[3, 3].AddEdge(tutorial4[4, 3]);
        tutorial4[4, 3].AddEdge(tutorial4[5, 3]);
        tutorial4[5, 3].AddEdge(tutorial4[6, 3]);
        tutorial4[6, 3].AddEdge(tutorial4[6, 4]);
        tutorial4[6, 4].AddEdge(tutorial4[6, 5]);
        tutorial4[6, 3].AddEdge(tutorial4[6, 2]);
        tutorial4[6, 2].AddEdge(tutorial4[6, 1]);
        tutorial4[5, 3].AddEdge(tutorial4[5, 2]);
        tutorial4[5, 2].AddEdge(tutorial4[5, 1]);
        tutorial4[5, 1].AddEdge(tutorial4[5, 0]);
        tutorial4[5, 0].AddEdge(tutorial4[6, 0]);
        tutorial4[6, 2].AddEdge(tutorial4[6, 1]);
        tutorial4[5, 0].AddEdge(tutorial4[4, 0]);
        tutorial4[4, 0].AddEdge(tutorial4[4, 1]);
        tutorial4[4, 1].AddEdge(tutorial4[4, 2]);
        tutorial4[4, 0].AddEdge(tutorial4[3, 0]);
        tutorial4[3, 0].AddEdge(tutorial4[2, 0]);
        tutorial4[5, 3].AddEdge(tutorial4[5, 4]);
        tutorial4[5, 4].AddEdge(tutorial4[5, 5]);
        tutorial4[5, 4].AddEdge(tutorial4[4, 4]);
        tutorial4[4, 4].AddEdge(tutorial4[4, 5]);
        tutorial4[4, 5].AddEdge(tutorial4[4, 6]);
        tutorial4[4, 6].AddEdge(tutorial4[5, 6]);
        tutorial4[5, 6].AddEdge(tutorial4[6, 6]);
        tutorial4[4, 6].AddEdge(tutorial4[3, 6]);
        tutorial4[3, 6].AddEdge(tutorial4[2, 6]);
        tutorial4[2, 6].AddEdge(tutorial4[1, 6]);
        tutorial4[1, 6].AddEdge(tutorial4[0, 6]);
        tutorial4[0, 6].AddEdge(tutorial4[0, 5]);
        tutorial4[0, 5].AddEdge(tutorial4[1, 5]);

        tutorial4[1, 0].actor = ActorType.Ofuda_Pickup;
        tutorial4[2, 0].actor = ActorType.Ofuda_Pickup;
        tutorial4[0, 3].actor = ActorType.Ofuda_Pickup;
        tutorial4[2, 4].actor = ActorType.Oni;
        tutorial4[4, 2].actor = ActorType.Oni;
        tutorial4[6, 6].actor = ActorType.Oni;
        tutorial4[6, 1].actor = ActorType.Spike_Trap;
        tutorial4[6, 4].actor = ActorType.Spike_Trap;

        tutorial4[3, 3].actor = ActorType.Ladder;
        tutorial4[1, 5].actor = ActorType.Ladder;
        MazeGenerator.tutorialSections[3] = tutorial4[3, 3];

        return tutorial4;
    }
}
