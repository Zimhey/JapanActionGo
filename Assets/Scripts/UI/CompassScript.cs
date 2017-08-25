using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassScript : MonoBehaviour
{

    //player the camera is following, set in unity
    public GameObject player;

    public Text compassLabel;

    private Camera cam;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam = player.GetComponentInChildren<Camera>();
        Quaternion dir = cam.transform.rotation;
        Vector3 curdir = new Vector3(0, dir.y, 0);
        float currentRotation = Vector3.Dot(curdir, new Vector3(0, 1, 0));
        if (currentRotation * 100 < 40 && currentRotation * 100 > -40)
        {
            compassLabel.text = string.Format("{0:00}:{1:00}", "Orientation", "N");
        }
        else if (currentRotation * 100 > 80 || currentRotation * 100 < -80)
        {
            compassLabel.text = string.Format("{0:00}:{1:00}", "Orientation", "S");
        }
        else
        {
            compassLabel.text = string.Format("{0:00}:{1:00}", "Orientation", " ");
        }
    }
}