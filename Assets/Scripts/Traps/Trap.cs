using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrapState
{
    Armed, // armed and ready to fire
    Triggered, // delay before fire
    Firing, // running the firing animation
    Unarmed, // waiting to be reset
    Resetting, // resetting animation
}

public class Trap : MonoBehaviour
{
    public TrapState state;

    public float TriggerDelay;
    public float FireAnimTime;
    public float HoldAnimTime;
    public float ResetAnimTime;

    public bool CanReset;
    public bool ResetRequested;

    protected float activationTime;

    public bool CanLog; 

    public Actor actor;

    public Actor ThisActor
    {
        get
        {
            if (actor == null)
              actor = GetComponentInParent<Actor>();
            return actor;
        }
    }

    public Vector3 ArmedPosition;
    public Vector3 ArmedRotation;
    public Vector3 UnarmedPosition;
    public Vector3 UnarmedRotation;

    public TrapState State
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            if (CanLog) ;
            GameManager.Instance.ActorStateChange(ThisActor, (int)state);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        switch (State)
        {
            case TrapState.Armed:
                gameObject.transform.localPosition = ArmedPosition;
                ResetRequested = false;
                break;
            case TrapState.Triggered:
                gameObject.transform.localPosition = ArmedPosition;
                TryFire();
                break;
            case TrapState.Firing:
                PlayFireAnimation();
                break;
            case TrapState.Unarmed:
                gameObject.transform.localPosition = UnarmedPosition;
                TryReset();
                break;
            case TrapState.Resetting:
                PlayResetAnimation();
                break;
        }
    }

    public void TriggerTrap()
    {
        if (State == TrapState.Armed)
        {
            activationTime = Time.time;
            State = TrapState.Triggered;
        }
    }

    private void TryFire()
    {
        if(Time.time > activationTime + TriggerDelay)
        {
            activationTime = Time.time;
            State = TrapState.Firing;
        }
    }

    protected void TryReset()
    {
        if (CanReset && ResetRequested && Time.time > activationTime + HoldAnimTime)
        {
            activationTime = Time.time;
            State = TrapState.Resetting;
        }
    }

    void PlayFireAnimation()
    {
        float complete = (Time.time - activationTime) / FireAnimTime;
        if (complete < 1.0)
        {
            gameObject.transform.localPosition = Vector3.Lerp(ArmedPosition, UnarmedPosition, complete);
            gameObject.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(ArmedRotation), Quaternion.Euler(UnarmedRotation), complete);
        }
        else
        {
            activationTime = Time.time;
            State = TrapState.Unarmed;
        }
            
    }

    void PlayResetAnimation()
    {
        float complete = (Time.time - activationTime) / ResetAnimTime;
        if (complete < 1.0)
        {
            gameObject.transform.localPosition = Vector3.Lerp(UnarmedPosition, ArmedPosition, complete);
            gameObject.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(UnarmedRotation), Quaternion.Euler(ArmedRotation), complete);
        }
        else
            State = TrapState.Armed;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null && State != TrapState.Resetting)
        {
            GameManager.Instance.ActorKilled(collider.gameObject.GetComponent<Actor>(), ThisActor);
            print("Killing " + collider.gameObject);
            collider.gameObject.SendMessage("Die");
        }
    }
}
