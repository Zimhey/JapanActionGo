using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    //player the camera is following, set in unity
    public GameObject player;

    //offset to keep camera a set distance from player
    private Vector3 offset;

    //timer to stop continous movement in stutter mode
    private int block = 0;

    //variable to track how much movement occurs so player movement can be adjusted
    public int rotation;

    //variable to hold which mode the camera is in, 0 = continous turn, 1 = stutter turn
    private int mode = 0;

    void Start()
    {
        //set offset
        offset = transform.position - player.transform.position;
    }

    private void Update()
    {
        //set turning speed of camera
        float horizontalSpeed;
        horizontalSpeed = 15.0F;
        // If Right Button is clicked Camera will turn right, left button is clicked camera will turn left.
        //decrements timer
        if (block > 0)
        {
            block--;
        }
        //adjusts mode if space is hit
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(mode > 0)
            {
                mode = 0;
            }
            else if (mode < 1)
            {
                mode = 1;
            }
        }

        //if continous set timer to 0
        if(mode == 0)
        {
            block = 0;
        }

        //if timer greater than 2 don't allow turn in stutter mode
        if (block < 2)
        {
            //right click, right turn
            if (Input.GetButton("Fire2"))
            {
                float h = horizontalSpeed;
                //if continous turn less rapidly
                if(mode == 0)
                {
                    h = h / 3;
                }
                //turn
                transform.Rotate(0, h, 0);
                //update timer
                block = 30;
                //update rotation tracker
                rotation += (int) h;
            }
            //left click, left turn
            if (Input.GetButton("Fire1"))
            {
                float h = horizontalSpeed;
                if (mode == 0)
                {
                    h = h / 3;
                }
                transform.Rotate(0, -h, 0);
                block = 30;
                rotation -= (int)h;
            }
        }
    }

    void LateUpdate()
    {
        //move camera to player's position
        transform.position = player.transform.position + offset;
    }
}
