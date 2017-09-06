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

public class Trap : MonoBehaviour
{
    public TrapState state;
    public float FireAnimTime;
    public float HoldAnimTime;
    public float ResetAnimTime;
    // public float TriggerDelay;
    public bool CanReset;
   // public float ResetTime;
    public bool ResetRequested;

    protected float activationTime;

    private Actor actorID;

    public Vector3 ArmedPosition;
    public Vector3 ArmedRotation;
    public Vector3 UnarmedPosition;
    public Vector3 UnarmedRotation;

    public TrapState State
    {
        set
        {
            state = value;

            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorStateChange(actorID, (int)state);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        switch (state)
        {
            case TrapState.Armed:
                gameObject.transform.localPosition = ArmedPosition;
                ResetRequested = false;
                break;
            case TrapState.Firing:
                PlayFireAnimation();
                break;
            case TrapState.Unarmed:
                gameObject.transform.localPosition = UnarmedPosition;
                tryReset();
                break;
            case TrapState.Resetting:
                PlayResetAnimation();
                break;
        }
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
        if (CanReset && ResetRequested && Time.time > activationTime + HoldAnimTime)
        {
            activationTime = Time.time;
            state = TrapState.Resetting;
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
            state = TrapState.Unarmed;
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
            state = TrapState.Armed;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null)
        {
            Debug.Log(gameObject.name + " Kill " + collider.gameObject.name);
            GameManager.Instance.ActorKilled(collider.gameObject.GetComponent<Actor>(), GetComponentInParent<Actor>());
            print("killing" + collider.gameObject);
            collider.gameObject.SendMessage("Die");
        }
    }
}
