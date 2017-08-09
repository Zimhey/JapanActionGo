using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{

    public GameObject player;

    private Vector3 offset;

    private int block = 0;

    public int rotation;

    private int mode = 0;

    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    private void Update()
    {
        float horizontalSpeed;
        horizontalSpeed = 15.0F;
        // If Right Button is clicked Camera will move.
        if(block > 0)
        {
            block--;
        }

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

        if(mode == 0)
        {
            block = 0;
        }

        if (block < 2)
        {
            if (Input.GetButton("Fire2"))
            {
                float h = horizontalSpeed;
                if(mode == 0)
                {
                    h = h / 3;
                }
                transform.Rotate(0, h, 0);
                block = 30;
                rotation += (int) h;
            }
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
        transform.position = player.transform.position + offset;
    }
}
