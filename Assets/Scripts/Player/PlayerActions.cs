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
    private GameObject chalkMarksParent;

    private bool drawing;
    private GameObject chalkPrefab;
    private LineRenderer currentMark;
    private GameObject lastDrawnOn;

    private GameObject ofudaPrefab;
    private GameObject thrownOfudaParent;
    private bool thrown;

	public bool UsingVR;
	public GameObject DrawingHand;
	private SteamVR_TrackedController drawingController;
	public GameObject ThrowingHand;
	private SteamVR_TrackedController throwingController;

    private Camera cam;

    // Use this for initialization
    void Start()
    {
        chalkPrefab = Actors.Prefabs[ActorType.Chalk_Mark];
        ofudaPrefab = Actors.Prefabs[ActorType.Ofuda_Projectile];
        cam = gameObject.GetComponentInChildren<Camera>();
        chalkMarksParent = new GameObject("Chalk Marks");
        thrownOfudaParent = new GameObject("Thrown Ofuda");

        if (UsingVR) 
		{
			drawingController = DrawingHand.GetComponent<SteamVR_TrackedController>();
			throwingController = ThrowingHand.GetComponent<SteamVR_TrackedController>();
		}
    }

    // Update is called once per frame
    void Update()
    {
		bool attemptDraw = UsingVR ? drawingController.triggerPressed : Input.GetButton("Fire1");
		bool attemptThrow = UsingVR ? throwingController.triggerPressed : Input.GetButton("Fire2");

        // Use Lever
        if (Input.GetButtonDown("Use"))
            Use();

        // Chalk Drawing

		if (attemptDraw && PlayerInventory.CanUse(ItemType.Chalk))
            DrawChalk();
        else
            drawing = false;

		if (attemptThrow && PlayerInventory.CanUse(ItemType.Ofuda))
            ThrowOfuda();
        else
            thrown = false;
    }

    public void Use()
    {
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.tag == "Lever" && Vector3.Distance(transform.position, obj.transform.position) < PullLeverRadius)
                obj.SendMessage("Pull");
            else if (obj.tag == "Ofuda" && Vector3.Distance(transform.position, obj.transform.position) < PullLeverRadius)
            {
                Destroy(obj);
                PlayerInventory.Found(ItemType.Ofuda);
            }
        }
    }

    public GameObject StartNewMark()
    {
        GameObject chalkMark = Instantiate(chalkPrefab);
        chalkMark.transform.parent = chalkMarksParent.transform;
        Marks.Add(chalkMark);
        currentMark = chalkMark.GetComponentInChildren<LineRenderer>();
        return chalkMark;
    }

    void DrawChalk()
    {
        if (!drawing) // just started drawing
        {
            drawing = true;
            StartNewMark();
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
                StartNewMark();
                lastDrawnOn = rayHit.collider.gameObject;
            }

            // Add point to the line render
            currentMark.positionCount++;
            currentMark.SetPosition(currentMark.positionCount - 1, rayHit.point);

            // Track distance drawn
            if (currentMark.positionCount > 1)
                DistanceDrawn += Vector3.Distance(currentMark.GetPosition(currentMark.positionCount - 2), rayHit.point);

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
			GameObject ofuda = Instantiate(ofudaPrefab, pointer.position + pointer.forward, pointer.rotation);
            ofuda.transform.parent = thrownOfudaParent.transform;
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
