using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {

    public GameObject ConnectedLadder;
    public MazeNode ConnectedLadderNode;
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
        if (collider.gameObject != null)
        {
            if (ConnectedLadder == null && collider.gameObject.tag == "Player")
            {
                foreach(MazeSection sec in GameManager.Sections)
                {
                    if (sec.SectionID == ConnectedLadderNode.SectionID && !sec.Spawned)
                        GameManager.SpawnSection(sec);
                }
                ConnectedLadder = ConnectedLadderNode.ladder;
                ConnectedLadder.GetComponent<Ladder>().ConnectedLadder = this.gameObject;
            }
            //Debug.Log(collider.gameObject.tag + " entered Cell R: " + Row + " C: " + Col + " at Time: " + Time.time);
            if (collider.gameObject.tag == "Player")
            {
                if (teleportable == true && ConnectedLadder.GetComponent<Ladder>().teleportable == true)
                {
                    if (!(ConnectedLadderNode.Floor == -1 && ConnectedLadderNode.Col == 1 && ConnectedLadderNode.Row == 5))
                    {
                        teleportable = false;
                        ConnectedLadder.GetComponent<Ladder>().teleportable = false;
                        collider.gameObject.transform.position = ConnectedLadder.transform.position;
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            teleportable = true;
        }
    }
}
