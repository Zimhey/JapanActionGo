using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfudaCounter : MonoBehaviour {

    //player the camera is following, set in unity
    public GameObject player;

    public Text ofudaLabel;

    private Inventory playerInventory;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerInventory = player.GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
            ofudaLabel.text = string.Format("{0:00}:{1:00}", "Ofuda tags", playerInventory.OfudaCharges);
    }
}
