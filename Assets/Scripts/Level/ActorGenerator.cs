using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorGenerator : MonoBehaviour {

    public static int Oni;
    public static int Ofuda;
    public static int Chalk;
    public static int SpikeTrap;
    public static int Inu;
    public static int PitTrap;
    public static int CrushingTrap;
    public static int Nyudo;

    // Use this for initialization
    void Start () {
		
	}

    public static void GenerateActorsHelper(Difficulty difficulty, MazeNode root, int seed)
    {
        switch((int) difficulty)
        {
            case 0:
                Oni = 2;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 1;
                Inu = 1;
                PitTrap = 1;
                CrushingTrap = 1;
                Nyudo = 1;
                break;
            case 1:
                Oni = 3;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 2;
                Inu = 1;
                PitTrap = 1;
                CrushingTrap = 2;
                Nyudo = 2;
                break;
            case 2:
                Oni = 9;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 2;
                Inu = 2;
                PitTrap = 1;
                CrushingTrap = 3;
                Nyudo = 4;
                break;
            case 3:
                Oni = 15;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 2;
                Inu = 3;
                PitTrap = 2;
                CrushingTrap = 4;
                Nyudo = 10;
                break;
            case 4:
                Oni = 20;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 3;
                Inu = 4;
                PitTrap = 3;
                CrushingTrap = 4;
                Nyudo = 12;
                break;
            case 5:
                Oni = 25;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 4;
                Inu = 5;
                PitTrap = 3;
                CrushingTrap = 5;
                Nyudo = 18;
                break;
        }

        MazeGenerator.GenerateActors(root, Ofuda, Oni, Chalk, SpikeTrap, Nyudo, Inu, CrushingTrap, PitTrap, seed);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
