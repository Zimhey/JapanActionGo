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
    public static int FallingTrap;
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
                FallingTrap = 1;
                Nyudo = 1;
                break;
            case 1:
                Oni = 1;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 1;
                Inu = 1;
                PitTrap = 1;
                FallingTrap = 1;
                Nyudo = 1;
                break;
            case 2:
                Oni = 1;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 1;
                Inu = 1;
                PitTrap = 1;
                FallingTrap = 1;
                Nyudo = 1;
                break;
            case 3:
                Oni = 1;
                Ofuda = 3;
                Chalk = 3;
                SpikeTrap = 1;
                Inu = 1;
                PitTrap = 1;
                FallingTrap = 1;
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
                FallingTrap = 1;
                Nyudo = 1;
                break;
        }

        MazeGenerator.GenerateActors(root, Ofuda, Oni, Chalk, SpikeTrap, Nyudo, Inu, seed);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
