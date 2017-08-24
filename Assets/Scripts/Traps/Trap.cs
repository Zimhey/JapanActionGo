using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrapState
{
    Armed, // armed and ready to fire
    Firing, // running the firing animation
    Unarmed, // waiting to be reset
    Resetting, // resetting animation
}

public class Trap : GameManager
{
    public TrapState state;
    public float FireAnimTime;
    public float ResetAnimTime;
    // public float TriggerDelay;
    public bool CanReset;
   // public float ResetTime;
    public bool ResetRequested;

    protected float activationTime;

    private Actor actorID;

    public TrapState State
    {
        set
        {
            state = value;

            actorID = GetComponent<Actor>();
            ActorStateChange(actorID, (int)state);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void TriggerTrap()
    {
        if (state != TrapState.Armed)
            return;
        activationTime = Time.time;
        state = TrapState.Firing;
    }

    protected void tryReset()
    {
        if (CanReset && ResetRequested)
        {
            activationTime = Time.time;
            state = TrapState.Resetting;
        }
    }
}
