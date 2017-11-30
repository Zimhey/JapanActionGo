using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfudaCounter : MonoBehaviour {

    //player the camera is following, set in unity
    public GameObject player;

    public Text ofudaLabel;
    public GameObject ofudaLabelVR;

    private Inventory playerInventory;

    private TextMesh VRCounter;

    // Use this for initialization
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>(); // TODO GROSS FIX THIS LETS NOT GRAB THE COMPONENT EVERY FRAME
        }

        if (ofudaLabelVR != null)
        {
            VRCounter = ofudaLabelVR.GetComponentInChildren<TextMesh>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ofudaLabel != null)
        {
            //ofudaLabel.text = string.Format("{0:00}:{1:00}", "Ofuda tags", playerInventory.OfudaCharges);
            ofudaLabel.text = "Ofuda: " + playerInventory.OfudaCharges;
        }

        if(ofudaLabelVR != null)
        {
            VRCounter.text = "Ofuda: " + playerInventory.OfudaCharges; 
        }
    }
}
