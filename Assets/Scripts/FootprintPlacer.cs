using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintPlacer : MonoBehaviour {

    //what type of footprint to be placed
    public GameObject Prefab;
    //how far apart are the footprints
    public int PlaceDistance;
    //layermask to raycast against
    public LayerMask LevelMask;

    //array of locations the actor has been
    private ArrayList previousLocations = new ArrayList();
    //actor rigidbody
    private Rigidbody rigidBody;
    private float distToGround;
    private CharacterController controller;
    private GameObject previousFootprint;
    private GameObject currentFootprint;
    private GameObject nextFootprint;

    private GameObject footPrintParent;

    private void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        controller = gameObject.GetComponent<CharacterController>();
        footPrintParent = new GameObject(gameObject.tag + " Footprints");
        footPrintParent.transform.parent = GameManager.Instance.GameParent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.CompareTag("Player"))
        {
            if (controller.isGrounded)
            {
                PlaceFootprints(previousLocations, PlaceDistance, Prefab, rigidBody);
            }
        }
        else
        {
            PlaceFootprints(previousLocations, PlaceDistance, Prefab, rigidBody);
        }
    }

    public void PlaceFootprints(ArrayList previousLocations, int enoughDistance, GameObject footprintPrefab, Rigidbody rigidBody)
    {
        float distanceToFloor = 0F;
        if (previousLocations.Count > 0)
        {
            //get latest
            int lastentry = previousLocations.Count - 1;
            Vector3 lastlocation = (Vector3)previousLocations[lastentry];
            //make sure it is not current location
            if (lastlocation != gameObject.transform.position)
            {
                //add current location
                previousLocations.Add(gameObject.transform.position);
            }

            //get oldest
            Vector3 firstlocation = (Vector3)previousLocations[0];
            //check to see if player has gone far enough for footprints to form
            if ((lastlocation - firstlocation).magnitude > enoughDistance)
            {
                //iterate through
                for (int iter = 0; iter < lastentry; iter++)
                {
                    //check locations along path
                    Vector3 currentlocation = (Vector3)previousLocations[iter];
                    //if locations are far enough apart make footprint at a given distance
                    if ((currentlocation - firstlocation).magnitude > enoughDistance)
                    {
                        //get direction
                        Vector3 norm = (currentlocation - firstlocation);
                        norm.Normalize();
                        //multiply by desired distance to get desired vector and add to first location
                        if (transform.position.y != distanceToFloor)
                        {
                            float maxDistance = 25;
                            Vector3 rayDirection = gameObject.transform.TransformDirection(Vector3.down);
                            maxDistance = rayDirection.magnitude;
                            //rayDirection = Vector3.MoveTowards
                            rayDirection.Normalize();
                            Ray ray = new Ray(gameObject.transform.position, rayDirection);
                            RaycastHit rayHit;

                            if (Physics.Raycast(ray, out rayHit, maxDistance, LevelMask))
                            {
                                Vector3 dist = rayHit.collider.transform.position;
                                distanceToFloor = dist.y;
                            }
                            
                        }
                        Vector3 dest = firstlocation + norm * (float)enoughDistance;
                        dest.y = distanceToFloor + 0.5f;
                        //make rotation
                        Quaternion rot = Quaternion.Euler(0, 0, 0);
                        //add to level
                        Instantiate(footprintPrefab, dest, rot).transform.parent = footPrintParent.transform;
                        //remove old steps
                        for (int remove = 0; remove < iter; remove++)
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
            previousLocations.Add(rigidBody.position);
        }
    }

    public bool IsGrounded()
    {
        Vector3 rayDirection = gameObject.transform.TransformDirection(Vector3.down);
        //rayDirection = Vector3.MoveTowards
        rayDirection.Normalize();
        Ray ray = new Ray(gameObject.transform.position, rayDirection);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, distToGround + 0.5F, LevelMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
