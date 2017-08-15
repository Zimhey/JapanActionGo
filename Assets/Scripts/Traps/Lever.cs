using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour {

    public Trap[] Traps;

    public bool Pulled;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Pull()
    {
        Pulled = !Pulled;
        if (Pulled)
            foreach (Trap trap in Traps)
                trap.TriggerTrap();
        else
            foreach (Trap trap in Traps)
                trap.ResetRequested = true;
    }
}
