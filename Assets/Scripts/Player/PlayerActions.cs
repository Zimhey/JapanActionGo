using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    private LayerMask drawingLayerMask;
    private int levelLayer;
    private int dynamicObjectLayer;

    public Inventory PlayerInventory;

    public float PullLeverRadius;
    
    public float DrawingDistance;
    public float DistanceDrawn;
    public List<GameObject> Marks;
    private GameObject chalkMarksParent;

    private bool drawing;
    private GameObject chalkPrefab;
    private LineRenderer currentMark;
    private Vector3 chalkFaceNormal;
    private Vector3 lastDrawnPoint;
    private GameObject lastObjectDrawnOn;


    private GameObject ofudaPrefab;
    private GameObject thrownOfudaParent;
    private bool thrown;

	public bool UsingVR;
	public GameObject DrawingHand;
	private SteamVR_TrackedController drawingController;
	public GameObject ThrowingHand;
	private SteamVR_TrackedController throwingController;

    public GameObject Compass;

    private Camera cam;

    // Use this for initialization
    void Start()
    {
        chalkPrefab = Actors.Prefabs[ActorType.Chalk_Mark];
        ofudaPrefab = Actors.Prefabs[ActorType.Ofuda_Projectile];
        cam = Camera.main;
        chalkMarksParent = new GameObject("Chalk Marks");
        chalkMarksParent.transform.parent = GameManager.Instance.GameParent.transform;
        thrownOfudaParent = new GameObject("Thrown Ofuda");
        thrownOfudaParent.transform.parent = GameManager.Instance.GameParent.transform;
        levelLayer = LayerMask.NameToLayer("Level");
        dynamicObjectLayer = LayerMask.NameToLayer("DynamicObject");
        drawingLayerMask =  1 << levelLayer | 1 << dynamicObjectLayer;

        if (Compass != null)
            Compass.SetActive(false);

        if (UsingVR) 
		{
			drawingController = DrawingHand.GetComponent<SteamVR_TrackedController>();
			throwingController = ThrowingHand.GetComponent<SteamVR_TrackedController>();
		}
    }

    // Update is called once per frame
    void Update()
    {
        //bool attemptDraw = UsingVR ? drawingController.triggerPressed : (Input.GetKey(KeyBindingScript.buttons["Draw"]) || Input.GetKey(KeyBindingScript.controller["Draw"]) || Input.GetKey(KeyBindingScript.vr["Draw"]));
        //bool attemptThrow = UsingVR ? throwingController.triggerPressed : (Input.GetKeyDown(KeyBindingScript.buttons["Throw"]) || Input.GetKey(KeyBindingScript.controller["Throw"]) || Input.GetKey(KeyBindingScript.vr["Throw"]));

        bool attemptDraw = (KeyBindingScript.DrawPressedVR() || Input.GetKey(KeyBindingScript.buttons["Draw"]) || Input.GetKey(KeyBindingScript.controller["C_Draw"]));
        bool attemptThrow = (KeyBindingScript.ThrowPressedVR() || Input.GetKeyDown(KeyBindingScript.buttons["Throw"]) || Input.GetKey(KeyBindingScript.controller["C_Throw"]));
        
        // Use Lever
        if (Input.GetButtonDown("Grab_Items"))
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

        if (Input.GetButtonDown("Use_Compass") && Compass != null)
            Compass.SetActive(true);
        if (Input.GetButtonUp("Use_Compass") && Compass != null)
            Compass.SetActive(false);
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
        Transform pointer;

		if (UsingVR)
			pointer = DrawingHand.transform;
		else
			pointer = cam.transform;

		Vector3 dir = pointer.transform.rotation * Vector3.forward; 
		Ray ray = new Ray(pointer.transform.position, dir);	         
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, DrawingDistance, drawingLayerMask)) // if object to draw on
        {

            // If can't draw across objects, start a new line when objects change
            if (rayHit.normal != chalkFaceNormal || !drawing || (lastObjectDrawnOn != rayHit.collider.gameObject && rayHit.collider.gameObject.layer == dynamicObjectLayer))
            {
                drawing = true;
                StartNewMark();
                currentMark.transform.rotation = Quaternion.LookRotation(-rayHit.normal);
                chalkFaceNormal = rayHit.normal;
                lastObjectDrawnOn = rayHit.collider.gameObject;
                if (rayHit.collider.gameObject.layer == dynamicObjectLayer)
                {
                    AttachTo a = currentMark.gameObject.AddComponent<AttachTo>();
                    a.To = rayHit.collider.gameObject;
                }
            }

            //
            Vector3 offset = rayHit.normal * 0.002f;
            Vector3 position = (Vector3)(currentMark.transform.worldToLocalMatrix * (rayHit.point + offset)) - currentMark.transform.position;
            
            // Add point to the line render
            currentMark.positionCount++;
            currentMark.SetPosition(currentMark.positionCount - 1, position);

            // Track distance drawn
            if (currentMark.positionCount > 1)
                DistanceDrawn += Vector3.Distance(lastDrawnPoint, rayHit.point);
            lastDrawnPoint = rayHit.point;

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
        GameManager.Instance.GameOver();
    }
    

    void SafeZoneCollision()
    {
        
    }

}
