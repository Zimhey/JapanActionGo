using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellLog : MonoBehaviour {

    public int Row;
    public int Col;
    public int CellID;

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
            Debug.Log(collider.gameObject.tag + " entered Cell R: " + Row + " C: " + Col + " at Time: " + Time.time);
        }
    }

}
