using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : Trap {


    public Vector3 ArmedPosition;
    public Vector3 UnarmedPosition;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		switch(state)
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


    void PlayFireAnimation()
    {
        float complete = (Time.time - activationTime) / FireAnimTime;
        if (complete < 1.0)
        {
            gameObject.transform.localPosition = Vector3.Lerp(ArmedPosition, UnarmedPosition, complete);
        }
        else
            state = TrapState.Unarmed;
    }

    void PlayResetAnimation()
    {
        float complete = (Time.time - activationTime) / ResetAnimTime;
        if (complete < 1.0)
        {
            gameObject.transform.localPosition = Vector3.Lerp(UnarmedPosition, ArmedPosition, complete);
        }
        else
            state = TrapState.Armed;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null)
        {
            Debug.Log(gameObject.name + " Kill " + collider.gameObject.name);
            collider.gameObject.SendMessage("Die");
        }
    }

}
