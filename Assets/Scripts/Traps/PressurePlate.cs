using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour {

    public Trap[] Traps;
    private Vector3 location;
    private List<GameObject> objectsOnTop;

	// Use this for initialization
	void Start () {
        objectsOnTop = new List<GameObject>();
        location = gameObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        for(int i = objectsOnTop.Count - 1; i >= 0; i--)
            if (objectsOnTop[i] == null)
                ObjectGone(objectsOnTop[i]);
    }

    void ObjectGone(GameObject obj)
    {
        objectsOnTop.Remove(obj);
        AnimatePlate();
    }

    void AnimatePlate()
    {
        if (objectsOnTop.Count > 0)
            gameObject.transform.position = location - new Vector3(0, 0.1f, 0);
        else
        {
            gameObject.transform.position = location;
            foreach (Trap trap in Traps)
                trap.ResetRequested = true;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null) // might need to test for Player Tag or Oni tag
        {
            objectsOnTop.Add(collider.gameObject);
            AnimatePlate();
            foreach (Trap trap in Traps)
                trap.TriggerTrap();
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject != null) // might need to test for Player Tag or Oni tag
        {
            ObjectGone(collider.gameObject);
        }
    }
}
