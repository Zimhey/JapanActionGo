using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //player movement speed, set in unity
    public float speed;

    //array of locations the player has been
    private ArrayList previousLocations = new ArrayList();

    //length before a footstep is placed
    //private int enough = 50;
    private int lessenough = 5;

    //camera following player, assigned in unity
    public CameraController cam;

    private Rigidbody rb;
    private Quaternion originalRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalRotation = transform.localRotation;
    }

    void FixedUpdate()
    {
        //gets keyboard input
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        //gets base movement direction
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        //adjusts for camera direction
        //Vector3 curmovement = Quaternion.Euler(0, cam.rotation, 0) * movement;

        rb.transform.localRotation = originalRotation * cam.rotation;

        //causes movement in desired direction with set speed
        rb.AddForceAtPosition(movement * speed, rb.worldCenterOfMass);

        //if no input no movement
        if (!Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.S))
        {
            if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.W))
            {
                if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.A))
                {
                    if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.D))
                    {
                        rb.velocity = new Vector3(0,0,0);
                    }
                }
            }
        }
        
        //if any locations exist
        if (previousLocations.Count > 0)
        {
            //get latest
            int lastentry = previousLocations.Count - 1;
            Vector3 lastlocation = (Vector3)previousLocations[lastentry];
            //make sure it is not current location
            if (lastlocation != rb.position)
            {
                //add current location
                previousLocations.Add(rb.position);
            }

            //get oldest
            Vector3 firstlocation = (Vector3)previousLocations[0];
            //check to see if player has gone far enough for footprints to form
            if ((lastlocation - firstlocation).magnitude > lessenough)
            {
                //iterate through
                for (int iter = 0; iter < lastentry; iter++)
                {
                    //check locations along path
                    Vector3 currentlocation = (Vector3)previousLocations[iter];
                    //if locations are far enough apart make footprint at a given distance
                    if ((currentlocation - firstlocation).magnitude > lessenough)
                    {
                        //get direction
                        Vector3 norm = (currentlocation - firstlocation);
                        norm.Normalize();
                        //multiply by desired distance to get desired vector and add to first location
                        Vector3 dest = firstlocation + norm * (float)lessenough - new Vector3(0,0.5F,0);
                        //make rotation
                        Quaternion rot = Quaternion.Euler(0, 0, 0);
                        //add to level
                        Instantiate(Resources.Load("Prefabs/Footprint"), dest, rot);
                        //remove old steps
                        for(int remove = 0; remove < iter; remove++)
                        {
                            previousLocations.RemoveAt(0);
                        }
                        break;
                    }
                }
            }
        }
        else
        {
            previousLocations.Add(rb.position);
        }
        
    }
}