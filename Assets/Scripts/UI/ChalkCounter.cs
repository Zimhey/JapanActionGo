using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChalkCounter : MonoBehaviour {

    //player the camera is following, set in unity
    public GameObject player;

    public Text chalkLabel;
    public GameObject chalkLabelVR;

    private Inventory playerInventory;
    private PlayerActions playerActions;
    private TextMesh VRCounter;

    // Use this for initialization
    void Start () {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>(); // TODO GROSS FIX THIS LETS NOT GRAB THE COMPONENT EVERY FRAME
            playerActions = player.GetComponent<PlayerActions>();
        }

        if (chalkLabelVR != null)
        {
            VRCounter = chalkLabelVR.GetComponentInChildren<TextMesh>();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (chalkLabel != null)
        {
            chalkLabel.text = "Chalk: " + playerInventory.ChalkCharges * 100 + playerActions.DistanceDrawn / playerInventory.DistancePerCharge * 100;
        }
        if (chalkLabelVR != null)
        {
            VRCounter.text = "Chalk: " + "\n" + playerInventory.ChalkCharges * 100 + playerActions.DistanceDrawn / playerInventory.DistancePerCharge * 100 +"%";
        }
        /*
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
        */
    }
}
