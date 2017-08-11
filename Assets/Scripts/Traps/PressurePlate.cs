using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour {

    public Trap[] Traps;
    private int objectsOnTop;
    private Vector3 location;

	// Use this for initialization
	void Start () {
        objectsOnTop = 0;
        location = gameObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void AnimatePlate()
    {
        if (objectsOnTop > 0)
            gameObject.transform.position = location - new Vector3(0, 0.1f, 0);
        else
            gameObject.transform.position = location;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null) // might need to test for Player Tag or Oni tag
        {
            objectsOnTop++;
            AnimatePlate();
            foreach (Trap trap in Traps)
                trap.SendMessage("TriggerTrap");
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject != null) // might need to test for Player Tag or Oni tag
        {
            objectsOnTop--;
            AnimatePlate();
            //foreach (Trap trap in Traps)
            //    trap.SendMessage("ResetTrap");
        }
    }
}
