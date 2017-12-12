using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour {

    public GameObject ConnectedLadder;
    public MazeNode ConnectedLadderNode;
    public bool teleportable = true;
    public MazeNode location;
    public int SectionID;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            //print("Floor: " + location.Floor);
            //print("SectionNumber: " + location.sectionNumber);
            //print("Is it a root?: " + location.isRoot);
            //print("Number of sections on the bottom floor at this difficulty: " + MazeGenerator.howManySectionsOnBottomFloor(GameManager.difficulty));
            if (location.Floor == 0 && location.sectionNumber == MazeGenerator.howManySectionsOnBottomFloor(GameManager.difficulty) - 1 && !location.isRoot)
                GameManager.Instance.Win();
            else
            {
                print("Made it here!");
                GameManager.Instance.EnterSection(this.gameObject, collider.gameObject);
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
