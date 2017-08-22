using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null && collider.gameObject.tag == "Oni")
        {
            collider.gameObject.SendMessage("SafeZoneCollision");
        }
        if (collider.gameObject != null && collider.gameObject.tag == "Inu")
        {
            collider.gameObject.SendMessage("SafeZoneCollision");
        }
    }
}
