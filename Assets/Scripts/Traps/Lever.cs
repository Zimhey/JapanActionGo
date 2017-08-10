using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour {

    public Trap[] Traps;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Pull()
    {

        foreach (Trap trap in Traps)
            trap.SendMessage("TriggerTrap");
    }
}
