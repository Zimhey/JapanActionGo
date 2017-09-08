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
            if (location.Col == MazeGenerator.GetSize(GameManager.difficulty) - 1 && location.Col == MazeGenerator.GetSize(GameManager.difficulty) - 1 && location.Floor == 0)
                GameManager.Win();
            else
                GameManager.Instance.EnterSection(this.gameObject, collider.gameObject);
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
