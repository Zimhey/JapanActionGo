using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : FootprintPlacer
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

	public bool UsingVR;
	public GameObject DrawingHand;
	private SteamVR_TrackedController drawingController;
	public GameObject ThrowingHand;
	private SteamVR_TrackedController throwingController;

    private Camera cam;
    //array of locations the oni has been
    private ArrayList previousLocations = new ArrayList();
    private int lessEnough = 5;
    private float distanceToFloor = 0.95F;
    private GameObject footprintPrefab;
    //player physics body
    private Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        chalkPrefab = Actors.Prefabs[ActorType.Chalk_Mark];
        ofudaPrefab = Actors.Prefabs[ActorType.Ofuda_Projectile];
        footprintPrefab = Actors.Prefabs[ActorType.Player_Footprint];
        cam = gameObject.GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();

        if (UsingVR) 
		{
			drawingController = DrawingHand.GetComponent<SteamVR_TrackedController>();
			throwingController = ThrowingHand.GetComponent<SteamVR_TrackedController>();
		}
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
		if ((Input.GetButton("Fire1") || drawingController.triggerPressed) && PlayerInventory.CanUse(ItemType.Chalk))
            DrawChalk();
        else
            drawing = false;

		if ((Input.GetButton("Fire2") || throwingController.triggerPressed) && PlayerInventory.CanUse(ItemType.Ofuda))
            ThrowOfuda();
        else
            thrown = false;

        PlaceFootprints(previousLocations, lessEnough, footprintPrefab, rb, distanceToFloor);
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

		Transform pointer;
		if (UsingVR)
			pointer = DrawingHand.transform;
		else
			pointer = cam.transform;
		Vector3 dir = pointer.transform.rotation * Vector3.forward; 
		Ray ray = new Ray(pointer.transform.position, dir);	         
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
			Transform pointer;
			if (UsingVR)
				pointer = ThrowingHand.transform;
			else
				pointer = cam.transform;
			Instantiate(ofudaPrefab, pointer.position + pointer.forward, pointer.rotation);
            PlayerInventory.Used(ItemType.Ofuda);
            thrown = true;
        }
    }


    void Die()
    {
        
    }
    

    void SafeZoneCollision()
    {
        
    }

}
