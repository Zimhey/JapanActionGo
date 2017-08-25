using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChalkCounter : MonoBehaviour {

    //player the camera is following, set in unity
    public GameObject player;

    public Text chalkLabel;
    public Text chalkLabel2;

    private Inventory playerInventory;
    private PlayerActions playerActions;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        player = GameObject.FindGameObjectWithTag("Player");
        playerInventory = player.GetComponent<Inventory>();
        playerActions = player.GetComponent<PlayerActions>();

        //whole charges
        if (playerInventory.ChalkCharges > 0 && playerActions.DistanceDrawn > 0)
        {
            chalkLabel.text = string.Format("{0:00}:{1:00}", "Whole charges", playerInventory.ChalkCharges - 1);
        }
        else
        {
            chalkLabel.text = string.Format("{0:00}:{1:00}", "Whole charges", playerInventory.ChalkCharges);
        }

        //current charge
        if (playerInventory.ChalkCharges > 0)
        {
            chalkLabel2.text = string.Format("{0:00}:{1:00}", "Current charge", 100 - playerActions.DistanceDrawn / playerInventory.DistancePerCharge * 100);
        }
        else
        {
            chalkLabel2.text = string.Format("{0:00}:{1:00}", "Current charge", 0);
        }

    }
}
