using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

    public bool CanReset;
    public float ResetTime;
    public bool Armed;
    public float TriggerDelay;
    private bool busy;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator TriggerTrap()
    {
        if(Armed && !busy)
        {
            busy = true;
            Armed = false;
            yield return new WaitForSeconds(TriggerDelay);
            StartCoroutine(ApplyDamage());
            yield return PlayAnimation();
            busy = false;
        }
    }

    public IEnumerator ResetTrap()
    {
        if(CanReset && !busy)
        {
            busy = true;
            yield return new WaitForSeconds(ResetTime);
            Armed = true;
            busy = false;
        }
    }

    public IEnumerator PlayAnimation() // override in child trap classes
    {
        yield return null;
    }

    public IEnumerator ApplyDamage() // override in child trap classes
    {
        yield return null;
    }

}
