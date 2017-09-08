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
                Oni = 1;
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
                SpikeTrap = 1;
                Inu = 1;
                PitTrap = 1;
                CrushingTrap = 1;
                Nyudo = 1;
                break;
            case 2:
                Oni = 9;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 1;
                Inu = 4;
                PitTrap = 1;
                CrushingTrap = 1;
                Nyudo = 1;
                break;
            case 3:
                Oni = 1;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 1;
                Inu = 1;
                PitTrap = 1;
                CrushingTrap = 1;
                Nyudo = 1;
                break;
            case 4:
                break;
            case 5:
                Oni = 1;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 1;
                Inu = 1;
                PitTrap = 1;
                CrushingTrap = 1;
                Nyudo = 1;
                break;
        }

        MazeGenerator.GenerateActors(root, Ofuda, Oni, Chalk, SpikeTrap, Nyudo, Inu, CrushingTrap, PitTrap, seed);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
