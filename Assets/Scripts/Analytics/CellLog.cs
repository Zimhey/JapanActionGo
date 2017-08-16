using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellLog : MonoBehaviour {

    public int Row;
    public int Col;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null)
        {
            if (collider.gameObject.tag == "Player")
                Debug.Log("Player entered Cell R: " + Row + " C: " + Col + " at Time: " + Time.time);
        }
    }

}
