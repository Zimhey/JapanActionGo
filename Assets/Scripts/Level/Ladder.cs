using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {

    public GameObject ConnectedLadder;
    public bool teleportable = true;
    MazeNode SectionRoot;
    public int SectionID;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null && teleportable && ConnectedLadder.GetComponent<Ladder>().teleportable)
        {
            //Debug.Log(collider.gameObject.tag + " entered Cell R: " + Row + " C: " + Col + " at Time: " + Time.time);
            if (collider.gameObject.tag == "Player")
            {
                teleportable = false;
                collider.gameObject.transform.position = ConnectedLadder.transform.position;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //teleportable = true;
            ConnectedLadder.GetComponent<Ladder>().teleportable = true;
        }
    }
    
}
