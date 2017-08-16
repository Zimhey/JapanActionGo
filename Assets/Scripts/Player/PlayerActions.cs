using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    public LayerMask levelLayer;
    public Inventory PlayerInventory;

    public float PullLeverRadius;
    
    public float DrawingDistance;
    public bool CanDrawAcrossObjects;
    public float DistanceDrawn;
    public List<GameObject> Marks;

    private bool drawing;
    private GameObject chalkPrefab;
    private LineRenderer recent;
    private GameObject lastDrawnOn;

    private GameObject ofudaPrefab;
    private bool thrown;

    private Camera cam;
    //array of locations the oni has been
    private ArrayList previousLocations = new ArrayList();
    private int lessenough = 5;

    // Use this for initialization
    void Start()
    {
        chalkPrefab = Resources.Load("Prefabs/ChalkMark") as GameObject;
        ofudaPrefab = Resources.Load("Prefabs/OfudaProjectile") as GameObject;
        cam = gameObject.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        // Use Lever
        if (Input.GetButtonDown("Use"))
        {
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.tag == "Lever" && Vector3.Distance(transform.position, obj.transform.position) < PullLeverRadius)
                    obj.SendMessage("Pull");
                else if(obj.tag == "Ofuda" && Vector3.Distance(transform.position, obj.transform.position) < PullLeverRadius)
                {
                    Destroy(obj);
                    PlayerInventory.Found(ItemType.Ofuda);
                }
            }
        }

        // Chalk Drawing
        if (Input.GetButton("Fire1") && PlayerInventory.CanUse(ItemType.Chalk))
            DrawChalk();
        else
            drawing = false;

        if (Input.GetButton("Fire2") && PlayerInventory.CanUse(ItemType.Ofuda))
            ThrowOfuda();
        else
            thrown = false;

        //if any locations exist
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
                        Vector3 dest = firstlocation + norm * (float)lessenough - new Vector3(0, 0.95F, 0);
                        //make rotation
                        Quaternion rot = Quaternion.Euler(0, 0, 0);
                        //add to level
                        Instantiate(Resources.Load("Prefabs/Footprint"), dest, rot);
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
            previousLocations.Add(gameObject.transform.position);
        }

    }

    void DrawChalk()
    {
        if (!drawing) // just started drawing
        {
            drawing = true;
            GameObject chalkMark = Instantiate(chalkPrefab);
            Marks.Add(chalkMark);
            recent = chalkMark.GetComponentInChildren<LineRenderer>();
        }

        Vector3 dir = cam.transform.rotation * Vector3.forward;

        Ray ray = new Ray(cam.transform.position, dir);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, DrawingDistance, levelLayer)) // if object to draw on
        {
            // If can't draw across objects, start a new line when objects change
            if (!CanDrawAcrossObjects && lastDrawnOn != rayHit.collider.gameObject)
            {
                GameObject chalkMark = Instantiate(chalkPrefab);
                Marks.Add(chalkMark);
                recent = chalkMark.GetComponentInChildren<LineRenderer>();
                lastDrawnOn = rayHit.collider.gameObject;
            }

            // Add point to the line render
            recent.positionCount++;
            recent.SetPosition(recent.positionCount - 1, rayHit.point);

            // Track distance drawn
            if (recent.positionCount > 1)
                DistanceDrawn += Vector3.Distance(recent.GetPosition(recent.positionCount - 2), rayHit.point);

            // Update Inventory
            if (DistanceDrawn > PlayerInventory.DistancePerCharge)
            {
                PlayerInventory.Used(ItemType.Chalk);
                DistanceDrawn -= PlayerInventory.DistancePerCharge;
            }

        }
        else
            drawing = false;
    }

    void ThrowOfuda()
    {
        if(!thrown)
        {
            Debug.Log("Throwing Ofuda");
            Instantiate(ofudaPrefab, cam.transform.position + cam.transform.forward, cam.transform.rotation);
            PlayerInventory.Used(ItemType.Ofuda);
            thrown = true;
        }
    }


    void Die()
    {
        Debug.Log("I've been killed!");
    }


}
